using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LabProject.Models;
using System.Security.Cryptography;
using ClosedXML.Excel;

namespace LabProject.Controllers
{
    public class HallsController : Controller
    {
        private readonly CinemaContext _context;

        public HallsController(CinemaContext context)
        {
            _context = context;
        }

        // GET: Halls
        public async Task<IActionResult> Index(int? id, string? name)
        {
            if (id == null) return RedirectToAction("Cinemas", "Index");
            ViewBag.CinemaId = id;
            ViewBag.CinemaName = name;
            var hallsByCinema = _context.Halls.Where(b => b.CinemaId == id).Include(b => b.Cinema);
            return View(await hallsByCinema.ToListAsync());
        }

        //Знайти зали в яких кількість сеансів більше заданого параметру
        public async Task<IActionResult> SessionQuantityByHalls(int sessionsQuantity, int hidden)
        {
            var query = _context.Halls
                .Include(s => s.Sessions)
                .Where(s => s.Sessions.Count > sessionsQuantity).ToList();

            ViewBag.hidden = hidden;
            MovieStatic.hallSet(query);
            return View("Index", query);
        }

        //Знайти список залів у заданому за локацією кінотеатрі, де транслюють фільми, де грає заданий актор
        public async Task<IActionResult> HallsByActor(string cinemaName, string castMemberName, int hidden)
        {
            var sessions = _context.Sessions
                .Include(h => h.Hall)
                    .ThenInclude(c => c.Cinema)
                .Include(m => m.Movie)
                .Where(c => c.Hall.Cinema.CinemaAddress == cinemaName).ToList();

            var castMember = _context.MovieCasts
                .Include(c => c.CastMember)
                .Include(m => m.Movie)
                .Where(c => c.CastMember.CastMemberFullName == castMemberName).ToList();

            List<Hall> query = new List<Hall>();

            foreach(var s in sessions)
            {
                foreach(var c in castMember)
                {
                    if (s.MovieId == c.MovieId) query.Add(s.Hall);
                }
            }

            ViewBag.hidden = hidden;
            MovieStatic.hallSet(query);
            return View("Index", query.Distinct());
        }


        // GET: Halls/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Halls == null)
            {
                return NotFound();
            }

            var hall = await _context.Halls
                .Include(h => h.Cinema)
                .FirstOrDefaultAsync(m => m.HallId == id);
            if (hall == null)
            {
                return NotFound();
            }

            //return View(hall);
            return RedirectToAction("Index", "Sessions", new { id = hall.HallId, name = hall.HallName });
        }

        // GET: Halls/Create
        public IActionResult Create(int cinemaId)
        {
            //ViewData["CinemaId"] = new SelectList(_context.Cinemas, "CinemaId", "CinemaName");

            ViewBag.CinemaId = cinemaId;
            ViewBag.CinemaName = _context.Cinemas.Where(c => c.CinemaId == cinemaId).FirstOrDefault().CinemaName;

            return View();
        }

        // POST: Halls/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int cinemaId, [Bind("HallId,HallName,HallCapacity,CinemaId")] Hall hall)
        {
            hall.CinemaId = cinemaId;
            if (ModelState.IsValid)
            {
                var existHallName = await _context.Halls.FirstOrDefaultAsync(c => c.HallName == hall.HallName && c.CinemaId == cinemaId);

                if (existHallName != null)
                {
                    ViewBag.CinemaId = cinemaId;
                    ModelState.AddModelError("HallName", "Зала з такою назвою вже існує в цьому кінотеатрі");
                    return View(existHallName);
                }

                _context.Add(hall);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Index", "Halls", new { id = cinemaId, name = _context.Cinemas.Where(c => c.CinemaId == cinemaId).FirstOrDefault().CinemaName});
            }
            //ViewData["CinemaId"] = new SelectList(_context.Cinemas, "CinemaId", "CinemaName", hall.CinemaId);
            //return View(hall);
            return RedirectToAction("Index", "Halls", new { id = cinemaId, name = _context.Cinemas.Where(c => c.CinemaId == cinemaId).FirstOrDefault().CinemaName });
        }

        // GET: Halls/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Halls == null)
            {
                return NotFound();
            }

            var hall = await _context.Halls.FindAsync(id);
            if (hall == null)
            {
                return NotFound();
            }
            //ViewData["CinemaId"] = new SelectList(_context.Cinemas, "CinemaId", "CinemaName", hall.CinemaId);
            return View(hall);
        }

        // POST: Halls/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,[Bind("HallId,HallName,HallCapacity,CinemaId")] Hall hall)
        {
            
            if (id != hall.HallId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existHallName = await _context.Halls.FirstOrDefaultAsync(c => c.HallId != hall.HallId && c.HallName == hall.HallName && c.CinemaId == hall.CinemaId);

                if (existHallName != null)
                {
                    ModelState.AddModelError("HallName", "Зала з такою назвою вже існує в цьому кінотеатрі");
                    return View(existHallName);
                }
                try
                {
                    _context.Update(hall);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HallExists(hall.HallId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Halls", new { id = hall.CinemaId, name = _context.Cinemas.Where(c => c.CinemaId == hall.CinemaId).FirstOrDefault().CinemaName });
            }
            //ViewData["CinemaId"] = new SelectList(_context.Cinemas, "CinemaId", "CinemaName", hall.CinemaId);
            // return RedirectToAction("Index", "Halls", new { hall.CinemaId, hall.Cinema.CinemaName });
            return RedirectToAction("Index", "Halls", new { id = id, name = _context.Cinemas.Where(c => c.CinemaId == id).FirstOrDefault().CinemaName });
        }

        // GET: Halls/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Halls == null)
            {
                return NotFound();
            }

            var hall = await _context.Halls
                .Include(h => h.Cinema)
                .FirstOrDefaultAsync(m => m.HallId == id);
            if (hall == null)
            {
                return NotFound();
            }

            return View(hall);
        }

        // POST: Halls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Halls == null)
            {
                return Problem("Entity set 'CinemaContext.Halls'  is null.");
            }
            var hall = await _context.Halls
                .Include(h => h.Sessions)
                .Include(h => h.Cinema)
                .FirstOrDefaultAsync(h => h.HallId == id);

            int cinemaId = hall.CinemaId;
            string cinemaName = hall.Cinema.CinemaName;
            if (hall != null)
            {
                foreach (var h in hall.Sessions)
                    _context.Remove(h);

                _context.Halls.Remove(hall);
            }
            
            await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(Index));
            return RedirectToAction("Index", new {id = cinemaId, name = cinemaName});
        }

        private bool HallExists(int id)
        {
          return (_context.Halls?.Any(e => e.HallId == id)).GetValueOrDefault();
        }

        //check if there is a cinema with the same address
       /* [HttpPost]
        public ActionResult HallNameExists(string HallName, int? id)
        {
            bool hallNameExists = false;
            ViewBag.CinemaId = id;
            var hall = _context.Halls.Where(b => b.CinemaId == id).Include(b => b.Cinema);

            //var hall = _context.Cinemas.Where(c => c.CinemaAddress == HallName).FirstOrDefault();

            //var hall = _context.Halls.Where()

            if (hall != null) hallNameExists = true;

            if (hallNameExists == true)
            {
                return Content("false");
            }
            else
            {
                return Content("true");
            }
        }*/

        //return to all halls
        public IActionResult ReturnToHalls(int cinemaId, string name)
        {
            ViewBag.CinemaId = cinemaId;
            ViewBag.CinemaName = name;
            var hallsByCinema = _context.Halls.Where(b => b.CinemaId ==cinemaId).Include(b => b.Cinema);
            return View(hallsByCinema.ToListAsync());
            //var halls = _context.Halls.Where(h => h.CinemaId == cinemaId).FirstOrDefault();
            //return View(halls);
            //return RedirectToAction("Index", "Hall", new { HallId = hallId });
        }

        public ActionResult Export()
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {

                var halls = MovieStatic.halls;

                ViewBag.hidden = 1;
                if (halls.Count == 0) return View("Index", halls);

                foreach (var hall in halls)
                {
                    var worksheet = workbook.Worksheets.Add(hall.HallName);
                    worksheet.Cell("A1").Value = "Назва";
                    worksheet.Cell("B1").Value = "Місткість";
                    worksheet.Row(1).Style.Font.Bold = true;

                    worksheet.Cell(2, 1).Value = hall.HallName;
                    worksheet.Cell(2, 2).Value = hall.HallCapacity;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();
                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"halls.xlsx"
                    };
                }
            }
        }
    }
}
