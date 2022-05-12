using Microsoft.AspNetCore.Mvc;

namespace unbooru.Web.Controllers
{
    [Route("[controller]")]
    public class ReactController : Controller
    {
        [HttpGet("{**url}")]
        public ActionResult Index()
        {
            return View();
        }
    }
}