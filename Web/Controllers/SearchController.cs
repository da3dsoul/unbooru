using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Poco;
using Microsoft.AspNetCore.Mvc;

namespace ImageInfrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public SearchController(DatabaseHelper helper)
        {
            _dbHelper = helper;
        }

        [HttpGet]
        public async Task<ActionResult<List<Image>>> Search(int limit = 0, int offset = 0)
        {
            var query = HttpContext.Request.Query;

            var any = false;
            var mode = query["mode"].FirstOrDefault();
            if (mode != null && mode.Equals("any", StringComparison.OrdinalIgnoreCase)) any = true;

            var tagStrings = query["tag"];
            var includedTags = tagStrings.Where(a => !a.StartsWith("!"));
            var excludedTags = tagStrings.Where(a => a.StartsWith("!")).Select(a => a.Substring(1));

            var images = await _dbHelper.Search(includedTags, excludedTags, any, limit, offset);
            if (images.Count == 0) return new NotFoundResult();
            return images;
        }

        [HttpGet("Count")]
        public async Task<ActionResult<int>> GetSearchPostCount()
        {
            var query = HttpContext.Request.Query;

            var any = false;
            var mode = query["mode"].FirstOrDefault();
            if (mode != null && mode.Equals("any", StringComparison.OrdinalIgnoreCase)) any = true;

            var tagStrings = query["tag"];
            var includedTags = tagStrings.Where(a => !a.StartsWith("!"));
            var excludedTags = tagStrings.Where(a => a.StartsWith("!")).Select(a => a[1..]);

            return await _dbHelper.GetSearchPostCount(includedTags, excludedTags, any);
        }
    }
}