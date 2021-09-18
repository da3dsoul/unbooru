using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using unbooru.Abstractions.Poco;
using Microsoft.AspNetCore.Mvc;

namespace unbooru.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : Controller
    {
        private static readonly string[] TagOrder = {"character", "copyright", "trivia", "metadata"};
        private readonly DatabaseHelper _dbHelper;

        public ImageController(DatabaseHelper helper)
        {
            _dbHelper = helper;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Image>> GetById(int id)
        {
            var image = await _dbHelper.GetImageById(id);
            if (image == null) return new NotFoundResult();
            image.Tags.Sort((a, b) =>
            {
                if (a.Type == null && b.Type == null) return 0;
                if (a.Type == null) return 1;
                if (b.Type == null) return -1;
                var aIndex = Array.IndexOf(TagOrder, a.Type.ToLowerInvariant());
                var bIndex = Array.IndexOf(TagOrder, b.Type.ToLowerInvariant());
                var compare = aIndex.CompareTo(bIndex);
                if (compare != 0) return compare;
                if (a.Name == null && b.Name == null) return 0;
                if (a.Name == null) return -1;
                if (b.Name == null) return -1;
                return string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase);
            });
            return image;
        }

        [HttpGet("{id:int}/{filename}")]
        public async Task<object> GetImageById(int id, string filename, [FromQuery]string size)
        {
            var blob = await _dbHelper.GetImageBlobById(id);
            if (blob == default) return new NotFoundResult();
            if ("small".Equals(size, StringComparison.InvariantCultureIgnoreCase))
            {
                var image = new MagickImage(blob);
                image.Format = MagickFormat.Pjpeg;
                image.Resize(new MagickGeometry("500x500>"));
                image.Quality = 60;
                blob = image.ToByteArray();
            }
            return File(blob, MimeTypes.GetMimeType(filename));
        }

        [HttpGet("Missing")]
        public async Task<ActionResult<List<Image>>> Missing(int limit = 0, int offset = 0)
        {
            var images = await _dbHelper.GetMissingData(limit, offset);
            if (images.Count == 0) return new NotFoundResult();
            return images;
        }

        [HttpGet("Missing/Count")]
        public async Task<ActionResult<int>> GetSearchPostCount()
        {
            return await _dbHelper.GetMissingDataCount();
        }

        [HttpGet("Post/Count")]
        public async Task<ActionResult<int>> GetDownloadedPostCount()
        {
            return await _dbHelper.GetDownloadedPostCount();
        }
    }
}
