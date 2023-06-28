using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using unbooru.Abstractions.Poco;

// ReSharper disable StringLiteralTypo

namespace unbooru.Web.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class RandomController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public RandomController(DatabaseHelper helper)
        {
            _dbHelper = helper;
        }

        [HttpGet]
        public async Task<ActionResult<List<Image>>> Random(int limit = 1)
        {
            var query = HttpContext.Request.Query;

            var searchParameters = SearchHelper.ParseSearchParameters(query, HttpContext.RequestServices);

            IQueryable<Image> images = _dbHelper.Search(searchParameters).OrderBy(a => Guid.NewGuid());
            if(limit > 0) images = images.Take(limit);
            var results = await images.ToListAsync();
            if (!results.Any()) return new NotFoundResult();
            return results;
        }
        
        [HttpGet("Image")]
        public async Task<ActionResult> RandomImage()
        {
            var query = HttpContext.Request.Query;
            var searchParameters = SearchHelper.ParseSearchParameters(query, HttpContext.RequestServices);
            IQueryable<Image> images = _dbHelper.Search(searchParameters).OrderBy(a => Guid.NewGuid());
            var result = await images.FirstOrDefaultAsync();
            if (result == null) return new NotFoundResult();
            var blob = await _dbHelper.GetImageBlobById(result.ImageId);
            if (blob == default) return new NotFoundResult();
            var source = await _dbHelper.ExecuteExpression(c =>
                c.Set<ImageSource>().FirstOrDefaultAsync(a => a.Image.ImageId == result.ImageId));
            var name = source?.OriginalFilename ?? "file.jpg";
            return File(blob, MimeTypes.GetMimeType(name), name);
        }

        [HttpGet("Redirect")]
        public async Task<ActionResult> RandomRedirect()
        {
            var query = HttpContext.Request.Query;
            var searchParameters = SearchHelper.ParseSearchParameters(query, HttpContext.RequestServices);
            IQueryable<Image> images = _dbHelper.Search(searchParameters).Include(a => a.Sources).OrderBy(a => Guid.NewGuid());
            var result = await images.FirstOrDefaultAsync();
            if (result == null) return new NotFoundResult();
            var source = result.Sources?.FirstOrDefault(a => !string.IsNullOrEmpty(a.OriginalFilename));
            if (source == null) return new NotFoundResult();
            return RedirectToAction("GetImageById", "Image", new { id = result.ImageId, filename = source.OriginalFilename });
        }
    }
}
