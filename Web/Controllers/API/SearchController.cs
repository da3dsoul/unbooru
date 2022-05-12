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

            var searchParameters = SearchHelper.ParseSearchParameters(query, HttpContext.RequestServices);
            var sortParameters = SearchHelper.ParseSortParameters(query);

            var images = _dbHelper.Search(searchParameters, sortParameters);
            if (offset > 0) images = images.Skip(offset);
            if (limit > 0) images = images.Take(limit);
            var results = await images.ToListAsync();
            if (!results.Any()) return new NotFoundResult();
            return results;
        }

        [HttpGet("Count")]
        public async Task<ActionResult<int>> GetSearchPostCount()
        {
            var query = HttpContext.Request.Query;
            var searchParameters = SearchHelper.ParseSearchParameters(query, HttpContext.RequestServices);

            return await _dbHelper.GetSearchPostCount(searchParameters);
        }
    }
}
