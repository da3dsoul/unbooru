using Microsoft.AspNetCore.Mvc;

namespace unbooru.Web.Controllers
{
    [Route("[controller]")]
    public class SearchController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}