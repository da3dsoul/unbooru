using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Web.SearchParameters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

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
            var searchParameters = new List<SearchParameter>();
            var query = HttpContext.Request.Query;

            ParseSearchParameters(query, searchParameters, HttpContext.RequestServices);

            var images = await _dbHelper.Search(searchParameters, limit, offset);
            if (images.Count == 0) return new NotFoundResult();
            return images;
        }

        [HttpGet("Count")]
        public async Task<ActionResult<int>> GetSearchPostCount()
        {
            var searchParameters = new List<SearchParameter>();
            var query = HttpContext.Request.Query;

            ParseSearchParameters(query, searchParameters, HttpContext.RequestServices);

            return await _dbHelper.GetSearchPostCount(searchParameters);
        }
        
        private static void ParseSearchParameters(IQueryCollection query, List<SearchParameter> searchParameters, IServiceProvider provider)
        {
            var dbHelper = provider.GetRequiredService<DatabaseHelper>();
            var tagStrings = query["tag"];
            if (tagStrings.Any())
            {
                var includedTags = tagStrings.Where(a => !a.StartsWith("!"));
                var excludedTags = tagStrings.Where(a => a.StartsWith("!")).Select(a => a[1..]);
                var includedSet = includedTags.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
                var includedTagIds = dbHelper.GetTagIds(includedSet).Result;
                var excludedSet = excludedTags.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
                var excludedTagIds = dbHelper.GetTagIds(excludedSet).Result;
                var any = false;
                var mode = query["mode"].FirstOrDefault();
                if (mode != null && mode.Equals("any", StringComparison.OrdinalIgnoreCase)) any = true;
                searchParameters.Add(new TagIdSearchParameter(includedTagIds, excludedTagIds, any));
            }

            tagStrings = query["tagid"];
            if (tagStrings.Any())
            {
                var includedTags = tagStrings.Where(a => !a.StartsWith("!")).Select(int.Parse);
                var excludedTags = tagStrings.Where(a => a.StartsWith("!")).Select(a => int.Parse(a[1..]));
                var any = false;
                var mode = query["mode"].FirstOrDefault();
                if (mode != null && mode.Equals("any", StringComparison.OrdinalIgnoreCase)) any = true;
                searchParameters.Add(new TagIdSearchParameter(includedTags, excludedTags, any));
            }

            tagStrings = query["aspect"];
            if (tagStrings.Any())
            {
                foreach (var s in tagStrings)
                {
                    NumberComparator op;
                    if (s.StartsWith("<=")) op = NumberComparator.LessThan | NumberComparator.Equal;
                    else if (s.StartsWith("<")) op = NumberComparator.LessThan;
                    else if (s.StartsWith(">=")) op = NumberComparator.GreaterThan | NumberComparator.Equal;
                    else if (s.StartsWith(">")) op = NumberComparator.GreaterThan;
                    else if (s.StartsWith("!=")) op = NumberComparator.NotEqual;
                    else op = NumberComparator.Equal;

                    var aspect = new string(s.SkipWhile(a => !char.IsDigit(a)).ToArray());
                    searchParameters.Add(new AspectRatioSearchParameter(op, double.Parse(aspect)));
                }
            }

            if (query.ContainsKey("sfw"))
            {
                searchParameters.Add(new SfwSearchParameter(provider));
            }
        }
    }
}
