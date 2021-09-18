using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByteSizeLib;
using unbooru.Abstractions.Poco;
using unbooru.Web.SearchParameters;
using unbooru.Web.SortParameters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable StringLiteralTypo

namespace unbooru.Web.Controllers
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

            var images = await _dbHelper.Search(searchParameters, sortParameters, limit, offset);
            if (images.Count == 0) return new NotFoundResult();
            return images;
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
