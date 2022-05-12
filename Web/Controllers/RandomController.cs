using Microsoft.AspNetCore.Mvc;

namespace unbooru.Web.Controllers
{
    [Route("[controller]")]
    public class RandomController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}