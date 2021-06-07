using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImageInfrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : Controller
    {
        private readonly CoreContext _context;
        
        public ImageController(CoreContext context)
        {
            _context = context;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Image>> GetById(int id)
        {
            var image = await _context.Images.Include(a => a.Sources).Include(a => a.Tags).Include(a => a.ArtistAccounts)
                .AsSplitQuery().OrderBy(a => a.ImageId).FirstOrDefaultAsync(a => a.ImageId == id);
            if (image == null) return new NotFoundResult();
            return image;
        }
        
        [HttpGet("{id:int}/{filename}")]
        public async Task<object> GetImageById(int id, string filename)
        {
            var image = await _context.Images.OrderBy(a => a.ImageId).FirstOrDefaultAsync(a => a.ImageId == id);
            if (image == null) return new NotFoundResult();
            return File(image.Blob, MimeTypes.GetMimeType(filename));
        }

        [HttpGet("Latest")]
        public async Task<ActionResult<List<Image>>> GetLatest(int limit = 0, int offset = 0)
        {
            List<Image> images;
            if (limit > 0)
                images = await _context.Images.Include(a => a.Sources).Include(a => a.Tags).Include(a => a.ArtistAccounts).AsSplitQuery().OrderByDescending(a => a.ImageId).Skip(offset).Take(limit).ToListAsync();
            else
                images = await _context.Images.Include(a => a.Sources).Include(a => a.Tags).Include(a => a.ArtistAccounts).AsSplitQuery().OrderByDescending(a => a.ImageId).Skip(offset).ToListAsync();
            if (images.Count == 0) return new NotFoundResult();
            return images;
        }
    }
}