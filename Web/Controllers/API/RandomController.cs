using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Runtime.InteropServices;
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
        public async Task<ActionResult<List<Image>>> Random(int limit = 0, int offset = 0, int currentID = 0, int seed = -1)
        {
            var query = HttpContext.Request.Query;
            var searchParameters = SearchHelper.ParseSearchParameters(query, HttpContext.RequestServices);
            IEnumerable<int> filteredIds = await _dbHelper.Search(searchParameters).Select(a => a.ImageId).ToArrayAsync();
            Span<int> shuffledIds = Shuffle(filteredIds, seed).ToArray();
            if (currentID <= 0) offset = shuffledIds.IndexOf(currentID) + 1; 
            if (offset > 0 && limit <= 0)
                shuffledIds = shuffledIds[offset..];
            else if (limit > 0 && offset <= 0)
                shuffledIds = shuffledIds[..limit];
            else if (limit > 0 && offset > 0)
                shuffledIds = shuffledIds.Slice(offset, Math.Min(limit, shuffledIds.Length - offset));

            var shuffledArray = shuffledIds.ToArray();

            var images = await _dbHelper.ExecuteExpression(c =>
                c.Images.AsNoTracking().Include(a => a.Sources)
                    .Where(i => ((IEnumerable<int>)shuffledArray).Contains(i.ImageId)).ToDictionaryAsync(a => a.ImageId));
            var results = shuffledArray.Select(a => images[a]).ToList();
            if (!results.Any()) return new NotFoundResult();
            return results;
        }

        [HttpGet("Image")]
        public async Task<ActionResult> RandomImage(int seed = -1)
        {
            var query = HttpContext.Request.Query;
            var searchParameters = SearchHelper.ParseSearchParameters(query, HttpContext.RequestServices);
            Span<int> filteredIds = await _dbHelper.Search(searchParameters).Select(a => a.ImageId).ToArrayAsync();
            var rand = seed == -1 ? new Random() : new Random(seed);
            var id = filteredIds[rand.Next(0, filteredIds.Length)];

            var result = await _dbHelper.GetImageById(id);
            if (result == null) return new NotFoundResult();
            var blob = await _dbHelper.GetImageBlobById(result.ImageId);
            if (blob == null) return new NotFoundResult();
            var name = result.Sources.FirstOrDefault(a => Path.HasExtension(a.OriginalFilename))?.OriginalFilename ?? "file.jpg";
            return File(blob, MimeTypes.GetMimeType(name), name);
        }

        [HttpGet("Redirect")]
        public async Task<ActionResult> RandomRedirect(int seed = -1)
        {
            var query = HttpContext.Request.Query;
            var searchParameters = SearchHelper.ParseSearchParameters(query, HttpContext.RequestServices);
            Span<int> filteredIds = await _dbHelper.Search(searchParameters).Select(a => a.ImageId).ToArrayAsync();
            var rand = seed == -1 ? new Random() : new Random(seed);
            var id = filteredIds[rand.Next(0, filteredIds.Length)];

            var result = await _dbHelper.GetImageById(id);
            if (result == null) return new NotFoundResult();
            var name = result.Sources.FirstOrDefault(a => Path.HasExtension(a.OriginalFilename))?.OriginalFilename ?? "file.jpg";
            return RedirectToAction("GetImageById", "Image", new { id = result.ImageId, filename = name });
        }

        /// <summary>
        /// Fisher-Yates Shuffle a Span
        /// </summary>
        /// <param name="items"></param>
        /// <param name="seed"></param>
        /// <typeparam name="T"></typeparam>
        private static void FisherYatesShuffle<T>(Span<T> items, int seed)
        {
            var rand = seed == -1 ? new Random() : new Random(seed);
            for (var i = items.Length - 1; i > 0; i--)
            {
                var j = rand.Next(0, i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }
        private static long GetStableHash(string s, int seed)
        {
            var bytes = MemoryMarshal.AsBytes(s.AsSpan());
            return (long)XxHash64.HashToUInt64(bytes, seed: seed);
        }

        private static IEnumerable<T> Shuffle<T>(IEnumerable<T> items, int seed) =>
            items.OrderBy(item => GetStableHash(item?.ToString() ?? string.Empty, seed == -1 ? System.Random.Shared.Next() : seed));
    }
}
