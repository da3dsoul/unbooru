using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using unbooru.Abstractions.Poco;
using unbooru.Core;
using unbooru.Web.SearchParameters;
using unbooru.Web.SortParameters;
using unbooru.Web.ViewModel;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace unbooru.Web
{
    public class DatabaseHelper
    {
        private readonly CoreContext _context;
        public DatabaseHelper(CoreContext context)
        {
            _context = context;
        }

        public async Task<Image> GetImageById(int id)
        {
            var image = await _context.Set<Image>().AsSplitQuery().AsNoTrackingWithIdentityResolution()
                .Include(a => a.Sources).ThenInclude(a => a.RelatedImages).ThenInclude(a => a.Image)
                .Include(a => a.Tags)
                .Include(a => a.ArtistAccounts)
                .OrderBy(a => a.ImageId)
                .FirstOrDefaultAsync(a => a.ImageId == id);
            return image;
        }

        public async Task<byte[]> GetImageBlobById(int id)
        {
            var fkPropertyName = _context.Model
                .FindEntityType(typeof(ImageBlob))?
                .FindNavigation(nameof(ImageBlob.Image))?
                .ForeignKey.Properties[0].Name;
            var image = await _context.Set<ImageBlob>().AsNoTracking().Where(a => id == EF.Property<int>(a, fkPropertyName))
                .Select(a => a.Data).FirstOrDefaultAsync();
            return image;
        }

        public async Task<List<ImageTag>> GetTags(string query)
        {
            var tags = _context.Set<ImageTag>().AsNoTracking().Where(a => a.Name.StartsWith(query)).OrderBy(a => a.Name);

            return await tags.ToListAsync();
        }

        public async Task<List<int>> GetTagIds(IEnumerable<string> tags)
        {
            return await _context.Set<ImageTag>().Where(a => tags.Contains(a.Name)).Select(a => a.ImageTagId).ToListAsync();
        }

        public async Task<List<Image>> Search(List<SearchParameter> searchParameters, List<SortParameter> sortParameters, int limit = 0, int offset = 0)
        {
            // this mess is 2 left joins. It's necessary
            var input = _context.Set<Image>().AsNoTracking();

            var images = IncludeModels(input, searchParameters, sortParameters);

            var expr = EvaluateSearchParameters(searchParameters);
            if (expr != null) images = images.Where(expr);

            var first = sortParameters.FirstOrDefault();
            if (first != null)
            {
                images =  first.Descending ? images.OrderByDescending(first.Selector) : images.OrderBy(first.Selector);
                foreach (var para in sortParameters.Skip(1))
                {
                    images = para.Descending
                        ? ((IOrderedQueryable<SearchViewModel>)images).ThenByDescending(para.Selector)
                        : ((IOrderedQueryable<SearchViewModel>)images).ThenBy(para.Selector);
                }
            }
            else
            {
                images = images.OrderByDescending(a => a.Image.ImageId);
            }

            images = images.Skip(offset);
            if (limit > 0) images = images.Take(limit);

            var result = images.Select(a => a.Image);

            return await result.ToListAsync();
        }

        public async Task<int> GetSearchPostCount(List<SearchParameter> searchParameters)
        {
            var expr = EvaluateSearchParameters(searchParameters);
            if (expr == null) return await _context.Set<Image>().CountAsync();
            var input = _context.Set<Image>().AsNoTracking();
            var images = IncludeModels(input, searchParameters, new List<SortParameter>());

            return await images.CountAsync(expr);
        }

        private IQueryable<SearchViewModel> IncludeModels(IQueryable<Image> input, List<SearchParameter> searchParameters, List<SortParameter> sortParameters)
        {
            var images = input.Select(a => new SearchViewModel { Image = a });
            if (searchParameters.Any(a => a.Types.Contains(typeof(ImageTag))) ||
                sortParameters.Any(a => a.Types.Contains(typeof(ImageTag))))
            {
                images = images.Select(a => new SearchViewModel { Image = a.Image, Tags = a.Image.Tags });
            }

            if (searchParameters.Any(a => a.Types.Contains(typeof(ImageBlob))) ||
                sortParameters.Any(a => a.Types.Contains(typeof(ImageBlob))))
            {
                images = images
                    .GroupJoin(_context.Set<ImageBlob>(), model => model.Image.ImageId,
                        blob => EF.Property<int>(blob, "ImageId"), (model, blobs) => new { model, blob = blobs })
                    .SelectMany(a => a.blob.DefaultIfEmpty(),
                        (a, b) => new SearchViewModel { Image = a.model.Image, Blob = b, Tags = a.model.Tags });
            }

            if (searchParameters.Any(a => a.Types.Contains(typeof(ImageSource))) ||
                sortParameters.Any(a => a.Types.Contains(typeof(ImageSource))))
            {
                images = images
                    .GroupJoin(_context.Set<ImageSource>(), model => model.Image.ImageId,
                        source => EF.Property<int>(source, "ImageSourceId"), (model, source) => new { model, source })
                    .SelectMany(a => a.source.DefaultIfEmpty(),
                        (a, b) => new SearchViewModel
                            { Image = a.model.Image, Blob = a.model.Blob, PixivSource = b, Tags = a.model.Tags })
                    .Where(a => a.PixivSource.Source == "Pixiv");
            }

            return images;
        }

        private static Expression<Func<SearchViewModel, bool>> EvaluateSearchParameters(IEnumerable<SearchParameter> searchParameters)
        {
            Expression<Func<SearchViewModel, bool>> result = null;
            foreach (var searchParameter in searchParameters)
            {
                if (result == null) result = searchParameter.Evaluate();
                else
                    result = searchParameter.Or
                        ? result.OrExpression(searchParameter.Evaluate())
                        : result.AndExpression(searchParameter.Evaluate());
            }

            return result;
        }

        public async Task<List<Image>> Search(IEnumerable<string> included, IEnumerable<string> excluded, bool anyTag = false, int limit = 0, int offset = 0)
        {
            var includedSet = included.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var includedTags = await _context.ImageTags.Where(a => includedSet.Contains(a.Name)).Select(a => a.ImageTagId).ToListAsync();
            var excludedSet = excluded.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var excludedTags = await _context.ImageTags.Where(a => excludedSet.Contains(a.Name)).Select(a => a.ImageTagId).ToListAsync();

            var images = _context.Images.AsNoTracking();

            if (anyTag)
                images = images.Where(a =>
                    a.Tags.Any() && (!includedTags.Any() || a.Tags.Any(b => includedTags.Contains(b.ImageTagId))) &&
                    !a.Tags.Any(b => excludedTags.Contains(b.ImageTagId)));
            else
                images = images.Where(a =>
                    a.Tags.Any() &&
                    (!includedTags.Any() ||
                     a.Tags.Count(b => includedTags.Contains(b.ImageTagId)) == includedTags.Count) &&
                    !a.Tags.Any(b => excludedTags.Contains(b.ImageTagId)));

            images = images.OrderByDescending(a => a.ImageId).Skip(offset);
            if (limit > 0) images = images.Take(limit);

            return await images.ToListAsync();
        }

        public async Task<int> GetSearchPostCount(IEnumerable<string> included, IEnumerable<string> excluded, bool any = false)
        {
            var includedSet = included.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var includedTags = await _context.ImageTags.AsNoTracking().Where(a => includedSet.Contains(a.Name)).Select(a => a.ImageTagId).ToListAsync();
            var excludedSet = excluded.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var excludedTags = await _context.ImageTags.AsNoTracking().Where(a => excludedSet.Contains(a.Name)).Select(a => a.ImageTagId).ToListAsync();

            if (any)
                return await _context.Images.CountAsync(a =>
                    a.Tags.Any() && (!includedTags.Any() || a.Tags.Any(b => includedTags.Contains(b.ImageTagId))) &&
                    !a.Tags.Any(b => excludedTags.Contains(b.ImageTagId)));

            return await _context.Images.CountAsync(a =>
                a.Tags.Any() &&
                (!includedTags.Any() || a.Tags.Count(b => includedTags.Contains(b.ImageTagId)) == includedTags.Count) &&
                !a.Tags.Any(b => excludedTags.Contains(b.ImageTagId)));
        }

        public async Task<List<Image>> GetMissingData(int limit = 0, int offset = 0)
        {
            var images = _context.Images.AsNoTracking()
                .Where(a => !a.Tags.Any())
                .OrderByDescending(a => a.ImageId)
                .Skip(offset);
            if (limit > 0) images = images.Take(limit);

            return await images.ToListAsync();
        }

        public async Task<int> GetMissingDataCount()
        {
            return await _context.Images.CountAsync(a => !a.Tags.Any());
        }

        public async Task<int> GetDownloadedPostCount()
        {
            return await _context.Set<ImageSource>().Where(a => a.Source == "Pixiv").Select(a => a.PostUrl).Distinct().CountAsync();
        }

        public async Task FixSizes(IServiceProvider provider)
        {
            var total = await _context.Images.CountAsync();
            var logger = provider.GetRequiredService<ILogger<DatabaseHelper>>();
            for (var i = 0; i < Math.Ceiling(total / 20D); i++)
            {
                using var scope = provider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<CoreContext>();
                
                logger.LogInformation("{Current}/{Total} Pages fixing sizes", i, Math.Ceiling(total / 20D));
                var images = await context.Images.Include(a => a.Blobs).Skip(i * 20).Take(20).ToArrayAsync();
                foreach (var image in images)
                {
                    var pic = new MagickImage(image.Blob);
                    image.Width = pic.Width;
                    image.Height = pic.Height;
                }

                await context.SaveChangesAsync();
            }
            logger.LogInformation("Done fixing sizes!");
        }
    }
}
