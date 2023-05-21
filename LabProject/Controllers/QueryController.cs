using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using LabProject.Models;


namespace LabProject.Controllers
{
    public class QueryController : Controller
    {
        private readonly CinemaContext _context;

        public QueryController(CinemaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieName");
            return View();
        }
    }
}
