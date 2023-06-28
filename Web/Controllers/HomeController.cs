using Microsoft.AspNetCore.Mvc;

namespace unbooru.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}
