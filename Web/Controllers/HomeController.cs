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
            var imageController = HttpContext.RequestServices.GetService<ImageController>();
            if (imageController == null) return NotFound("Unable to find ImageController");
            return View(new IndexViewModel()
            {
                Images = (await imageController.GetLatest(ImagesPerPage)).Value.AsReadOnly(),
                ImagesPerPage = ImagesPerPage,
                Page = 1
            });
        }

        [Route("Image/{id:int}")]
        public async Task<ActionResult> ImageDetail(int id)
        {
            var imageController = HttpContext.RequestServices.GetService<ImageController>();
            if (imageController == null) return NotFound("Unable to find ImageController");
            return View(new IndexViewModel()
            {
                Images = (await imageController.GetLatest(ImagesPerPage)).Value.AsReadOnly(),
                ImagesPerPage = ImagesPerPage,
                Page = 1
            });
        }
    }
}