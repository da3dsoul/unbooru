using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using unbooru.Abstractions.Poco;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoreLinq;

// ReSharper disable StringLiteralTypo

namespace unbooru.Web.Controllers
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
        public async Task<ActionResult<List<Image>>> Random(int limit = 1, int? seed = null)
        {
            var query = HttpContext.Request.Query;

            var searchParameters = SearchHelper.ParseSearchParameters(query, HttpContext.RequestServices);
            var sortParameters = SearchHelper.ParseSortParameters(query);

            IEnumerable<Image> images = await _dbHelper.Search(searchParameters, sortParameters).ToListAsync();
            var random = seed == null ? new Random() : new Random(seed.Value); 
            images = images.Shuffle(random);
            if(limit > 0) images = images.Take(limit);
            var results = images.ToList();
            if (!results.Any()) return new NotFoundResult();
            return results;
        }
        
        [HttpGet("Image")]
        public async Task<ActionResult> RandomImage(int? seed = null)
        {
            var query = HttpContext.Request.Query;

            var searchParameters = SearchHelper.ParseSearchParameters(query, HttpContext.RequestServices);
            var sortParameters = SearchHelper.ParseSortParameters(query);

            IEnumerable<Image> images = await _dbHelper.Search(searchParameters, sortParameters).ToListAsync();
            var random = seed == null ? new Random() : new Random(seed.Value);
            var result = images.Shuffle(random).FirstOrDefault();
            if (result == null) return new NotFoundResult();
            var blob = await _dbHelper.GetImageBlobById(result.ImageId);
            if (blob == default) return new NotFoundResult();
            return File(blob, MimeTypes.GetMimeType(result.Sources.FirstOrDefault()?.OriginalFilename ?? "file.jpg"));
        }
    }
}
