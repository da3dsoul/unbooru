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
    }
}