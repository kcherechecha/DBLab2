using Microsoft.AspNetCore.Mvc;

namespace LabProject.Controllers
{
    public class QueryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
