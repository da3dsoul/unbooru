using Microsoft.AspNetCore.Mvc;

namespace unbooru.Web.Controllers
{
    [Route("[controller]")]
    public class ImageController : Controller
    {
        [HttpGet("{id:int}")]
        public ActionResult Index(int id)
        {
            return View();
        }
    }
}