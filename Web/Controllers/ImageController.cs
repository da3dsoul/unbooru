using System.Collections.Generic;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Poco;
using Microsoft.AspNetCore.Mvc;

namespace ImageInfrastructure.Web.Controllers
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
            return image;
        }

        [HttpGet("{id:int}/{filename}")]
        public async Task<object> GetImageById(int id, string filename)
        {
            var blob = await _dbHelper.GetImageBlobById(id);
            if (blob == default) return new NotFoundResult();
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
    }
}
