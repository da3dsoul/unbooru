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

        public async Task<Image> GetById(int id)
        {
            var image = await _context.Images.Include(a => a.Sources).Include(a => a.Tags)
                .Include(a => a.ArtistAccounts).AsSplitQuery().OrderBy(a => a.ImageId)
                .FirstOrDefaultAsync(a => a.ImageId == id);
            return image;
        }

        public async Task<List<Image>> GetLatest(int limit = 0, int offset = 0)
        {
            var images = _context.Images.Include(a => a.Sources).Include(a => a.Tags).Include(a => a.ArtistAccounts)
                .AsSplitQuery().OrderByDescending(a => a.ImageId).Skip(offset);
            if (limit > 0)
                images = images.Take(limit);

            return await images.ToListAsync();
        }

        public async Task<List<Image>> GetSafe(int limit = 0, int offset = 0)
        {
            var exclusions = new HashSet<string> (new List<string>{"penis", "vaginal", "sex"}, StringComparer.OrdinalIgnoreCase);
            var images = _context.Images.Include(a => a.Sources).Include(a => a.Tags)
                .Include(a => a.ArtistAccounts).AsSplitQuery()
                .Where(a => a.Tags.Any() && !a.Tags.Any(b => exclusions.Contains(b.Name)))
                .OrderByDescending(a => a.ImageId).Skip(offset);
            if (limit > 0)
                images = images.Take(limit);

            return await images.ToListAsync();
        }
    }
}