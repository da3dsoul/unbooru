using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using unbooru.Abstractions.Poco;

namespace unbooru.Web.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public TagController(DatabaseHelper helper)
        {
            _dbHelper = helper;
        }

        [ItemCanBeNull]
        public async Task<ActionResult<IEnumerable<ImageTag>>> Index(int limit = 0, int offset = 0)
        {
            return await _dbHelper.GetAllTags(limit, offset);
        }

        [HttpGet("ByName/{id:int}")]
        public async Task<ActionResult<ImageTag>> GetTag(int id)
        {
            var tag = await _dbHelper.GetTag(id);
            if (tag == null) return NotFound($"No tag found with ID: {id}");
            return tag;
        }

        [HttpGet("ByName/{query}")]
        public async Task<ActionResult<List<ImageTag>>> GetTags(string query)
        {
            var tags = await _dbHelper.GetTags(query);
            return tags;
        }
    }
}
