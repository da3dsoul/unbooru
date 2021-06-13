using System.Threading.Tasks;
using ImageInfrastructure.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ImageInfrastructure.Web.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        private const int ImagesPerPage = 21;
        
        public async Task<ActionResult> Index()
        {
            var dbHelper = HttpContext.RequestServices.GetService<DatabaseHelper>();
            if (dbHelper == null) return NotFound("Unable to find DatabaseHelper");
            return View(new IndexViewModel
            {
                Images = await dbHelper.GetLatest(ImagesPerPage),
                ImagesPerPage = ImagesPerPage,
                Page = 1
            });
        }

        [Route("Image/{id:int}")]
        public async Task<ActionResult> ImageDetail(int id)
        {
            var dbHelper = HttpContext.RequestServices.GetService<DatabaseHelper>();
            if (dbHelper == null) return NotFound("Unable to find DatabaseHelper");
            var image = await dbHelper.GetById(id);
            return View();
        }
        
        [Route("Safe")]
        public async Task<ActionResult> Safe()
        {
            var dbHelper = HttpContext.RequestServices.GetService<DatabaseHelper>();
            if (dbHelper == null) return NotFound("Unable to find DatabaseHelper");
            return View(new IndexViewModel
            {
                Images = await dbHelper.GetSafe(ImagesPerPage),
                ImagesPerPage = ImagesPerPage,
                Page = 1
            });
        }
    }
}