using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ByteSizeLib;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Web.SearchParameters;
using ImageInfrastructure.Web.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable StringLiteralTypo

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

            var sort = ParseSortParameters(query);

            var images = await _dbHelper.Search(searchParameters, sort, limit, offset);
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
            AddTagQueries(query, searchParameters, dbHelper);
            AddTagIdQueries(query, searchParameters);
            AddAspectRatioQueries(query, searchParameters);
            AddWidthQueries(query, searchParameters);
            AddHeightQueries(query, searchParameters);
            AddFileSizeQueries(query, searchParameters);
            AddPostDateQueries(query, searchParameters);
            AddImportDateQueries(query, searchParameters);
            AddPixivIdQueries(query, searchParameters);
            AddSfwQuery(query, searchParameters, provider);
        }

        private static Func<IQueryable<SearchViewModel>, IQueryable<SearchViewModel>> ParseSortParameters(IQueryCollection query)
        {
            IQueryable<SearchViewModel> Function(IQueryable<SearchViewModel> a) => a.OrderByDescending(b => b.Image.ImageId);
            var queryStrings = query["Sort"];
            if (!queryStrings.Any()) return Function;

            var newFunction = (Func<IQueryable<SearchViewModel>, IQueryable<SearchViewModel>>)Function;

            foreach (var s in queryStrings)
            {
                var q = s;
                var desc = false;
                if (q.StartsWith("!"))
                {
                    q = new string(q.SkipWhile(a => !char.IsLetter(a)).ToArray());
                    desc = true;
                }

                q = q.ToLower();
                var first = newFunction == Function;
                newFunction = GetParsedSortFunction(q, first, desc) ?? Function;
            }

            return newFunction;
        }

        private static Func<IQueryable<SearchViewModel>, IQueryable<SearchViewModel>> GetParsedSortFunction(string q, bool first, bool desc)
        {
            Expression<Func<SearchViewModel, object>> selector;
            switch (q)
            {
                case "imageid":
                    selector = b => b.Image.ImageId;
                    break;
                case "aspect":
                    selector = b => (double)b.Image.Width / b.Image.Height;
                    break;
                case "width":
                    selector = b => b.Image.Width;
                    break;
                case "height":
                    selector = b => b.Image.Height;
                    break;
                case "size":
                    selector = b => b.Blob.Size;
                    break;
                case "postdate":
                    selector = b => b.Sources.FirstOrDefault(a => a.Source == "Pixiv").PostDate;
                    break;
                case "importdate":
                    selector = b => b.Image.ImportDate;
                    break;
                case "pixivid":
                    selector = b => b.Sources.FirstOrDefault(a => a.Source == "Pixiv").PostId;
                    break;
                default:
                    return null;
            }

            if (first)
            {
                return desc ? a => a.OrderByDescending(selector) : a => a.OrderBy(selector);
            }

            return desc
                ? a => ((IOrderedQueryable<SearchViewModel>)a).ThenByDescending(selector)
                : a => ((IOrderedQueryable<SearchViewModel>)a).ThenBy(selector);
        }

        #region Query Parsing
        private static void AddTagQueries(IQueryCollection query, List<SearchParameter> searchParameters, DatabaseHelper dbHelper)
        {
            var queryStrings = query["Tag"];
            if (!queryStrings.Any()) return;
            var includedTags = queryStrings.Where(a => !a.StartsWith("!"));
            var excludedTags = queryStrings.Where(a => a.StartsWith("!")).Select(a => a[1..]);
            var includedSet = includedTags.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var includedTagIds = dbHelper.GetTagIds(includedSet).Result;
            var excludedSet = excludedTags.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var excludedTagIds = dbHelper.GetTagIds(excludedSet).Result;
            var any = false;
            var mode = query["mode"].FirstOrDefault();
            if (mode != null && mode.Equals("any", StringComparison.OrdinalIgnoreCase)) any = true;
            searchParameters.Add(new TagIdSearchParameter(includedTagIds, excludedTagIds, any));
        }

        private static void AddTagIdQueries(IQueryCollection query, List<SearchParameter> searchParameters)
        {
            var queryStrings = query["TagID"];
            if (!queryStrings.Any()) return;
            var includedTags = queryStrings.Where(a => !a.StartsWith("!")).Select(int.Parse);
            var excludedTags = queryStrings.Where(a => a.StartsWith("!")).Select(a => int.Parse(a[1..]));
            var any = false;
            var mode = query["mode"].FirstOrDefault();
            if (mode != null && mode.Equals("any", StringComparison.OrdinalIgnoreCase)) any = true;
            searchParameters.Add(new TagIdSearchParameter(includedTags, excludedTags, any));
        }

        private static void AddAspectRatioQueries(IQueryCollection query, List<SearchParameter> searchParameters)
        {
            var queryStrings = query["Aspect"];
            if (!queryStrings.Any()) return;
            foreach (var s in queryStrings)
            {
                var op = NumberComparatorEnum.Parse(s);
                var aspect = new string(s.SkipWhile(a => !char.IsDigit(a)).ToArray());
                searchParameters.Add(new AspectRatioSearchParameter(op, double.Parse(aspect)));
            }
        }

        private static void AddWidthQueries(IQueryCollection query, List<SearchParameter> searchParameters)
        {
            var queryStrings = query["Width"];
            if (!queryStrings.Any()) return;
            foreach (var s in queryStrings)
            {
                var op = NumberComparatorEnum.Parse(s);
                var width = new string(s.SkipWhile(a => !char.IsDigit(a)).ToArray());
                searchParameters.Add(new WidthSearchParameter(op, int.Parse(width)));
            }
        }

        private static void AddHeightQueries(IQueryCollection query, List<SearchParameter> searchParameters)
        {
            var queryStrings = query["Height"];
            if (!queryStrings.Any()) return;
            foreach (var s in queryStrings)
            {
                var op = NumberComparatorEnum.Parse(s);
                var height = new string(s.SkipWhile(a => !char.IsDigit(a)).ToArray());
                searchParameters.Add(new HeightSearchParameter(op, int.Parse(height)));
            }
        }

        private static void AddFileSizeQueries(IQueryCollection query, List<SearchParameter> searchParameters)
        {
            var queryStrings = query["Size"];
            if (!queryStrings.Any()) return;
            foreach (var s in queryStrings)
            {
                var op = NumberComparatorEnum.Parse(s);
                var size = new string(s.SkipWhile(a => !char.IsDigit(a)).ToArray());
                var byteSize = ByteSize.Parse(size);
                searchParameters.Add(new FileSizeSearchParameter(op, Convert.ToInt64(byteSize.Bytes)));
            }
        }

        private static void AddPostDateQueries(IQueryCollection query, List<SearchParameter> searchParameters)
        {
            var queryStrings = query["PostDate"];
            if (!queryStrings.Any()) return;
            foreach (var s in queryStrings)
            {
                var op = NumberComparatorEnum.Parse(s);
                var time = new string(s.SkipWhile(a => !char.IsDigit(a)).ToArray());
                searchParameters.Add(new PostDateSearchParameter(op, DateTime.Parse(time)));
            }
        }

        private static void AddImportDateQueries(IQueryCollection query, List<SearchParameter> searchParameters)
        {
            var queryStrings = query["ImportDate"];
            if (!queryStrings.Any()) return;
            foreach (var s in queryStrings)
            {
                var op = NumberComparatorEnum.Parse(s);
                var time = new string(s.SkipWhile(a => !char.IsDigit(a)).ToArray());
                searchParameters.Add(new ImportDateSearchParameter(op, DateTime.Parse(time)));
            }
        }

        private static void AddPixivIdQueries(IQueryCollection query, List<SearchParameter> searchParameters)
        {
            var queryStrings = query["PixivID"];
            if (!queryStrings.Any()) return;
            foreach (var s in queryStrings)
            {
                searchParameters.Add(new PixivIDSearchParameter(s));
            }
        }

        private static void AddSfwQuery(IQueryCollection query, List<SearchParameter> searchParameters, IServiceProvider provider)
        {
            if (query.ContainsKey("SFW"))
            {
                searchParameters.Add(new SfwSearchParameter(provider));
            }
        }
#endregion
    }
}
