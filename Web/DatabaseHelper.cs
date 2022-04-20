using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using unbooru.Abstractions.Poco;
using unbooru.Core;
using unbooru.Web.SearchParameters;
using unbooru.Web.SortParameters;
using unbooru.Web.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace unbooru.Web
{
    public class DatabaseHelper
    {
        private readonly CoreContext _context;
        public DatabaseHelper(CoreContext context)
        {
            _context = context;
        }

        public async Task<T> ExecuteExpression<T>(Func<CoreContext, Task<T>> func) where T : class
        {
            return await func.Invoke(_context);
        }

        public async Task<Image> GetImageById(int id)
        {
            var image = await _context.Set<Image>().AsSplitQuery().AsNoTrackingWithIdentityResolution()
                .Include(a => a.Sources).ThenInclude(a => a.RelatedImages).ThenInclude(a => a.Image)
                .Include(a => a.TagSources.Where(t => t.Tag.Type != null))
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

        public async Task<ArtistAccount> GetArtistAccountById(int id)
        {
            var account = await _context.Set<ArtistAccount>().OrderBy(a => a.ArtistAccountId).FirstOrDefaultAsync(a => a.ArtistAccountId == id);
            return account;
        }

        public async Task<ArtistAccount> GetArtistAccountByExternalId(string id)
        {
            var account = await _context.Set<ArtistAccount>().OrderBy(a => a.Id).FirstOrDefaultAsync(a => a.Id == id);
            return account;
        }

        public async Task<List<ArtistAccount>> GetArtistAccounts(string name)
        {
            IOrderedQueryable<ArtistAccount> account;
            if (name.Length > 3)
                account = _context.Set<ArtistAccount>().Where(a => a.Name.Contains(name)).OrderBy(a => a.Name);
            else
                account = _context.Set<ArtistAccount>().Where(a => a.Name.StartsWith(name)).OrderBy(a => a.Name);
            return await account.ToListAsync();
        }

        public async Task<byte[]> GetArtistAvatarById(int id)
        {
            var account = await _context.Set<ArtistAccount>().OrderBy(a => a.ArtistAccountId).FirstOrDefaultAsync(a => a.ArtistAccountId == id);
            return account?.Avatar;
        }

        public async Task<ImageTag> GetTag(int id)
        {
            var tags = await _context.Set<ImageTag>().AsNoTracking().OrderBy(a => a.ImageTagId).FirstOrDefaultAsync(a => a.ImageTagId == id);

            return tags;
        }

        public async Task<List<ImageTag>> GetTags(string query)
        {
            IOrderedQueryable<ImageTag> tags;
            if (query.Length > 3)
                tags = _context.Set<ImageTag>().AsNoTracking().Where(a => a.Name.Contains(query)).OrderBy(a => a.Name);
            else
                tags = _context.Set<ImageTag>().AsNoTracking().Where(a => a.Name.StartsWith(query)).OrderBy(a => a.Name);

            return await tags.ToListAsync();
        }

        /*public async Task<List<ImageTag>> GetTagsWeighted(string query)
        {
            List<(int id, string name)> tags = await _context.Set<ImageTag>().Select(a => ValueTuple.Create(a.ImageTagId, a.Name)).ToListAsync();

            return tags;
        }*/

        public async Task<List<int>> GetTagIds(IEnumerable<string> tags)
        {
            return await _context.Set<ImageTag>().Where(a => tags.Contains(a.Name)).Select(a => a.ImageTagId).ToListAsync();
        }

        public IQueryable<Image> Search(List<SearchParameter> searchParameters, List<SortParameter> sortParameters)
        {
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

            return images.Select(a => a.Image);
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
            // this mess is a bunch of left joins. It's necessary
            var images = input.Select(a => new SearchViewModel { Image = a });
            if (searchParameters.Any(a => a.Types.Contains(typeof(ImageTag))) ||
                sortParameters.Any(a => a.Types.Contains(typeof(ImageTag))))
            {
                images = images.Select(a => new SearchViewModel { Image = a.Image, Tags = a.Image.Tags });
            }

            if (searchParameters.Any(a => a.Types.Contains(typeof(ArtistAccount))) ||
                sortParameters.Any(a => a.Types.Contains(typeof(ArtistAccount))))
            {
                images = images.Select(a => new SearchViewModel { Image = a.Image, Tags = a.Tags, ArtistAccounts = a.Image.ArtistAccounts});
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
                        source => EF.Property<int>(source, "ImageId"), (model, source) => new { model, source })
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

        public async Task<ActionResult<IEnumerable<ArtistAccount>>> GetAllArtistAccounts(int limit, int offset)
        {
            return await _context.Set<ArtistAccount>().OrderBy(a => a.Name).Skip(offset).Take(limit).ToListAsync();
        }

        public async Task<ActionResult<IEnumerable<ImageTag>>> GetAllTags(int limit, int offset)
        {
            return await _context.Set<ImageTag>().OrderBy(a => a.Name).Skip(offset).Take(limit).ToListAsync();
        }
    }
}
