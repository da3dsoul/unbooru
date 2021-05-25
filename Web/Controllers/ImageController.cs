using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImageInfrastructure.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : Controller
    {

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Image>> GetById(int id)
        {
            var context = HttpContext.RequestServices.GetService<CoreContext>();
            if (context == null) return new NotFoundResult();
            var image = await context.Images.Include(a => a.Sources).Include(a => a.Tags).Include(a => a.ArtistAccounts)
                .AsSplitQuery().FirstOrDefaultAsync(a => a.ImageId == id);
            if (image == null) return new NotFoundResult();
            return image;
        }
        
        [HttpGet("{id:int}/{filename}")]
        public async Task<object> GetImageById(int id, string filename)
        {
            var context = HttpContext.RequestServices.GetService<CoreContext>();
            if (context == null) return new NotFoundResult();
            var image = await context.Images.FirstOrDefaultAsync(a => a.ImageId == id);
            if (image == null) return new NotFoundResult();
            return File(image.Blob, MimeTypes.GetMimeType(filename));
        }

        [HttpGet("Latest")]
        public async Task<ActionResult<List<Image>>> GetLatest(int limit = 0, int offset = 0)
        {
            var context = HttpContext.RequestServices.GetService<CoreContext>();
            if (context == null) return new NotFoundResult();
            List<Image> images;
            if (limit > 0)
                images = await context.Images.Include(a => a.Sources).Include(a => a.Tags).Include(a => a.ArtistAccounts).AsSplitQuery().OrderByDescending(a => a.ImageId).Skip(offset).Take(limit).ToListAsync();
            else
                images = await context.Images.Include(a => a.Sources).Include(a => a.Tags).Include(a => a.ArtistAccounts).AsSplitQuery().OrderByDescending(a => a.ImageId).Skip(offset).ToListAsync();
            if (images.Count == 0) return new NotFoundResult();
            return images;
        }
        
        public async Task<ActionResult> Index()
        {
            return View(new IndexViewModel()
            {
                Images = (await GetLatest(20)).Value.AsReadOnly(),
                ImagesPerPage = 20,
                Page = 1
            });
        }
        
        public class IndexViewModel
        {
            public IReadOnlyList<Image> Images { get; set; }
            public int ImagesPerPage { get; set; }
            public int Page { get; set; }
        }
    }
}