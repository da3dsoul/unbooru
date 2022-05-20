using Microsoft.AspNetCore.Mvc;

namespace unbooru.Web.Controllers
{
    [Route("[controller]")]
    public class ArtistController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}