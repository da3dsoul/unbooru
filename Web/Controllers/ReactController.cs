using Microsoft.AspNetCore.Mvc;

namespace ImageInfrastructure.Web.Controllers
{
    [Route("/")]
    public class ReactController : Controller
    {
        [HttpGet("{**url}")]
        public ActionResult Index()
        {
            return View();
        }
    }
}