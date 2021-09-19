using System.Collections.Generic;
using System.Threading.Tasks;
using unbooru.Abstractions.Poco;
using Microsoft.AspNetCore.Mvc;

namespace unbooru.Web.Controllers
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
