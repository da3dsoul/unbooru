using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using unbooru.Abstractions.Enums;

namespace unbooru.Web.OpenGraph
{
    public class OpenGraphMiddleware
    {
        private readonly RequestDelegate _next;

        public OpenGraphMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider, DatabaseHelper dbHelper)
        {
            var originalBody = context.Response.Body;
            var newBody = new MemoryStream();
            context.Response.Body = newBody;
            await _next(context);
            newBody.Position = 0;
            if (context.Response.StatusCode is < 200 or > 299 || !context.Response.Headers["Content-Type"]
                .Any(a => a.Contains("html", StringComparison.InvariantCultureIgnoreCase)))
            {
                await newBody.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
                return;
            }

            const string template = @"<meta property=""og:type"" content=""website"">
<title>{0}</title>
<meta name=""twitter:card"" content=""summary_large_image"" />
<meta property=""og:title"" content=""{0}"" />
<meta name=""description"" content=""{1}"" />
<meta property=""og:description"" content=""{1}"" />
<meta property=""og:image"" content=""{2}"" />
<meta property=""og:url"" content=""{3}"" />";

            if (context.Request.Path.HasValue && context.Request.Path.Value.StartsWith("/Image", StringComparison.InvariantCultureIgnoreCase))
            {
                var (title, desc, url) = await GetDataForRequest(context, dbHelper);
                if (title != null)
                {
                    var response = string.Format(template, title, desc, url,
                        "https://da3dsoul.dev" + context.Request.Path);
                    var original = await new StreamReader(newBody).ReadToEndAsync();
                    var replaced = original.Replace("<head>", "<head>" + response);
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(replaced));
                    await stream.CopyToAsync(originalBody);
                    context.Response.Body = originalBody;
                    return;
                }
            }
            if (context.Request.Path.HasValue && context.Request.Path.Value.StartsWith("/Search", StringComparison.InvariantCultureIgnoreCase))
            {
                var query = context.Request.Query;
                var searchParams = SearchHelper.ParseSearchParameters(query, serviceProvider);
                var sortParams = SearchHelper.ParseSortParameters(query);
                var results = await dbHelper.Search(searchParams, sortParams, 1, 0);
                var resultCount = await dbHelper.GetSearchPostCount(searchParams);
                var firstTemp = results.FirstOrDefault();
                var first = firstTemp == null ? null : await dbHelper.GetImageById(firstTemp.ImageId);
                var pixiv = first?.Sources.FirstOrDefault(a => "Pixiv".Equals(a.Source));
                var description = $"{resultCount} results";
                var url = "";
                if (pixiv != null)
                {
                    description +=
                        $" | {first.ArtistAccounts.FirstOrDefault()?.Name} - {pixiv.Title}";
                    url = $"https://da3dsoul.dev/api/Image/{first.ImageId}/{pixiv.OriginalFilename}?size=small";
                }


                var response = string.Format(template, "Search", description, url, "https://da3dsoul.dev" + context.Request.Path + context.Request.QueryString);
                var original = await new StreamReader(newBody).ReadToEndAsync();
                var replaced = original.Replace("<head>", "<head>" + response);
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(replaced));
                await stream.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
                return;
            }
            await newBody.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }

        private static readonly Regex ImageId =
            new("/Image/(\\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private async Task<(string title, string description, string image)> GetDataForRequest(HttpContext context,
            DatabaseHelper dbHelper)
        {
            var url = context.Request.Path;
            var match = ImageId.Match(url.Value);
            var id = match.Groups[1].Value;
            if (int.TryParse(id, out int imageId))
            {
                var image = await dbHelper.GetImageById(imageId);
                if (image == null) return default;
                var pixiv = image.Sources.FirstOrDefault(a => "Pixiv".Equals(a.Source));
                var title = image.ArtistAccounts.FirstOrDefault()?.Name + " - " + pixiv?.Title;
                var desc = string.Join(" ",
                    image.Tags.Where(a => a.Safety == TagSafety.Safe).Take(5).Select(a => a.Name));
                return (title, desc, $"https://da3dsoul.dev/api/Image/{imageId}/{pixiv.OriginalFilename}?size=small");
            }

            return default;
        }
    }
}
