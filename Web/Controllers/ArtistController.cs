using System.Collections.Generic;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using unbooru.Abstractions.Poco;

namespace unbooru.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtistController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public ArtistController(DatabaseHelper helper)
        {
            _dbHelper = helper;
        }

        public async Task<ActionResult<IEnumerable<ArtistAccount>>> Index(int limit = 0, int offset = 0)
        {
            return await _dbHelper.GetAllArtistAccounts(limit, offset);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ArtistAccount>> GetById(int id)
        {
            var account = await _dbHelper.GetArtistAccountById(id);
            if (account == null) return new NotFoundResult();
            return account;
        }

        [HttpGet("{id:int}/Avatar")]
        public async Task<ActionResult<byte[]>> GetAvatarById(int id)
        {
            var blob = await _dbHelper.GetArtistAvatarById(id);
            if (blob == default) return new NotFoundResult();
            var image = new MagickImage(blob);
            return File(blob, image.FormatInfo?.MimeType);
        }

        [HttpGet("ExternalId/{id}")]
        public async Task<ActionResult<ArtistAccount>> GetArtistByExternalId(string id)
        {
            var account = await _dbHelper.GetArtistAccountByExternalId(id);
            if (account == null) return new NotFoundResult();
            return account;
        }

        [HttpGet("ByName/{name}")]
        public async Task<ActionResult<List<ArtistAccount>>> FindArtists(string name)
        {
            return await _dbHelper.GetArtistAccounts(name);
        }
    }
}
