using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using unbooru.Abstractions.Poco;

namespace unbooru.Web.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : Controller
    {
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
            image.TagSources.Sort((a, b) =>
            {
                if (a.Tag?.Type == null && b.Tag?.Type == null) return 0;
                if (a.Tag?.Type == null) return 1;
                if (b.Tag?.Type == null) return -1;
                var aIndex = Array.IndexOf(DatabaseHelper.TagOrder, a.Tag.Type.ToLowerInvariant());
                var bIndex = Array.IndexOf(DatabaseHelper.TagOrder, b.Tag.Type.ToLowerInvariant());
                var compare = aIndex.CompareTo(bIndex);
                if (compare != 0) return compare;
                if (a.Tag.Name == null && b.Tag.Name == null) return 0;
                if (a.Tag.Name == null) return 1;
                if (b.Tag.Name == null) return -1;
                var nameCompare = string.Compare(a.Tag.Name, b.Tag.Name, StringComparison.InvariantCultureIgnoreCase);
                if (nameCompare != 0) return nameCompare;
                return string.Compare(a.Source, b.Source, StringComparison.InvariantCultureIgnoreCase);
            });
            return image;
        }

        [HttpGet("{id:int}/{filename}")]
        public async Task<ActionResult> GetImageById(int id, string filename, [FromQuery]string size, [FromQuery]bool upscale = false)
        {
            var blob = await _dbHelper.GetImageBlobById(id);
            if (blob == default) return new NotFoundResult();
            blob = ResizeImage(size, blob);
            return File(blob, MimeTypes.GetMimeType(filename));
        }

        private static byte[] ResizeImage(string size, byte[] blob)
        {
            if ("small".Equals(size, StringComparison.InvariantCultureIgnoreCase))
            {
                var image = new MagickImage(blob);
                image.Format = MagickFormat.Pjpeg;
                image.Resize(new MagickGeometry("500x500>"));
                image.Quality = 100;
                blob = image.ToByteArray();
            } else if ("medium".Equals(size, StringComparison.InvariantCultureIgnoreCase))
            {
                var image = new MagickImage(blob);
                image.Format = MagickFormat.Pjpeg;
                image.Resize(new MagickGeometry("800x800>"));
                image.Quality = 100;
                blob = image.ToByteArray();
            } else if ("large".Equals(size, StringComparison.InvariantCultureIgnoreCase))
            {
                var image = new MagickImage(blob);
                image.Format = MagickFormat.Pjpeg;
                image.Resize(new MagickGeometry("1200x1200>"));
                image.Quality = 100;
                blob = image.ToByteArray();
            }

            return blob;
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
