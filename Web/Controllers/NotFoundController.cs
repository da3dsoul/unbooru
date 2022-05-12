using Microsoft.AspNetCore.Mvc;

namespace unbooru.Web.Controllers
{
    [Route("[controller]")]
    public class NotFoundController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}