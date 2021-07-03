using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Core;
using Microsoft.EntityFrameworkCore;

namespace ImageInfrastructure.Web
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
            var image = await _context.Images.AsNoTracking().Include(a => a.Sources).ThenInclude(a => a.RelatedImages).Include(a => a.Tags)
                .Include(a => a.ArtistAccounts).AsSplitQuery().OrderBy(a => a.ImageId)
                .FirstOrDefaultAsync(a => a.ImageId == id);
            return image;
        }

        public async Task<byte[]> GetImageBlobById(int id)
        {
            var fkPropertyName = _context.Model
                .FindEntityType(typeof(ImageBlob))?
                .FindNavigation(nameof(ImageBlob.Image))?
                .ForeignKey.Properties[0].Name;
            var image = await _context.ImageBlobs.AsNoTracking().Where(a => id == EF.Property<int>(a, fkPropertyName))
                .Select(a => a.Data).FirstOrDefaultAsync();
            return image;
        }

        public async Task<List<ImageTag>> GetTags(string query)
        {
            var tags = _context.ImageTags.Where(a => a.Name.StartsWith(query)).OrderBy(a => a.Name);

            return await tags.ToListAsync();
        }

        public async Task<List<Image>> Search(IEnumerable<string> included, IEnumerable<string> excluded, bool any = false, int limit = 0, int offset = 0)
        {
            var includedSet = included.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var includedTags = await _context.ImageTags.AsNoTracking().Where(a => includedSet.Contains(a.Name)).Select(a => a.ImageTagId).ToListAsync();
            var excludedSet = excluded.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var excludedTags = await _context.ImageTags.AsNoTracking().Where(a => excludedSet.Contains(a.Name)).Select(a => a.ImageTagId).ToListAsync();

            var images = _context.Images.Include(a => a.RelatedImages).Include(a => a.Sources)
                .ThenInclude(a => a.RelatedImages).Include(a => a.Tags).Include(a => a.ArtistAccounts).AsSplitQuery();

            if (any)
                images = images.Where(a =>
                    a.Tags.Any() && (!includedTags.Any() || a.Tags.Any(b => includedTags.Contains(b.ImageTagId))) &&
                    !a.Tags.Any(b => excludedTags.Contains(b.ImageTagId))).Skip(offset);
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
            var includedTags = await _context.ImageTags.Where(a => includedSet.Contains(a.Name)).Select(a => a.ImageTagId).ToListAsync();
            var excludedSet = excluded.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var excludedTags = await _context.ImageTags.Where(a => excludedSet.Contains(a.Name)).Select(a => a.ImageTagId).ToListAsync();

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
            var images = _context.Images.Include(a => a.RelatedImages).Include(a => a.Sources)
                .ThenInclude(a => a.RelatedImages).Include(a => a.Tags).Include(a => a.ArtistAccounts).AsSplitQuery();

            images = images.Where(a => !a.Tags.Any()).OrderByDescending(a => a.ImageId).Skip(offset);
            if (limit > 0) images = images.Take(limit);

            return await images.ToListAsync();
        }

        public async Task<int> GetMissingDataCount()
        {
            return await _context.Images.CountAsync(a => !a.Tags.Any());
        }
    }
}
