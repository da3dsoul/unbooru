using System.Collections.Generic;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Poco;
using Microsoft.AspNetCore.Mvc;

namespace ImageInfrastructure.Web.Controllers
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

        [HttpGet("{query}")]
        public async Task<ActionResult<List<ImageTag>>> GetTags(string query)
        {
            var tags = await _dbHelper.GetTags(query);
            return tags;
        }
    }
}