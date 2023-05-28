using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LabProject.Models;
using System.ComponentModel;
using ClosedXML.Excel;

namespace LabProject.Controllers
{
    public class CinemasController : Controller
    {
        private readonly CinemaContext _context;

        public CinemasController(CinemaContext context)
        {
            _context = context;
        }

        // GET: Cinemas
        public async Task<IActionResult> Index()
        {
              return _context.Cinemas != null ? 
                          View(await _context.Cinemas.ToListAsync()) :
                          Problem("Entity set 'CinemaContext.Cinemas'  is null.");
        }

        public async Task<IActionResult> CinemasWithSessions(DateTime sessionDate, int hidden)
        {
            //Знайти усі кінотеатри які мають принаймні один сеанс у заданий день

            var query = _context.Sessions
                .Include(h => h.Hall)
                    .ThenInclude(c => c.Cinema)
                .Where(s => s.SessionDateTime.Date == sessionDate.Date).ToList();

            List<Cinema> cinemas = new List<Cinema>();

            foreach (var q in query)
                cinemas.Add(q.Hall.Cinema);

            ViewBag.hidden = hidden;
            MovieStatic.cinemaSet(cinemas);
            return View("Index", cinemas);
        }

        // GET: Cinemas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Cinemas == null)
            {
                return NotFound();
            }

            var cinema = await _context.Cinemas
                .FirstOrDefaultAsync(m => m.CinemaId == id);
            if (cinema == null)
            {
                return NotFound();
            }
            //return view(cinema)

            return RedirectToAction("Index", "Halls", new {id = cinema.CinemaId, name=cinema.CinemaName});

        }

        // GET: Cinemas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cinemas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CinemaId,CinemaName,CinemaAddress")] Cinema cinema)
        {
            var existAddress = await _context.Cinemas.FirstOrDefaultAsync(c => c.CinemaAddress == cinema.CinemaAddress);

            if(existAddress != null)
            {
                ModelState.AddModelError("CinemaAddress", "Вже є кінотеатр з такою адресою");
                return View(existAddress);
            }

            if (ModelState.IsValid)
            {
                _context.Add(cinema);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cinema);
        }

        // GET: Cinemas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Cinemas == null)
            {
                return NotFound();
            }

            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
            {
                return NotFound();
            }
            return View(cinema);
        }

        // POST: Cinemas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CinemaId,CinemaName,CinemaAddress")] Cinema cinema)
        {
            

            if (id != cinema.CinemaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existAddress = await _context.Cinemas.FirstOrDefaultAsync(c => c.CinemaId != cinema.CinemaId && c.CinemaAddress == cinema.CinemaAddress);

                if (existAddress != null)
                {
                    ModelState.AddModelError("CinemaAddress", "Вже є кінотеатр з такою адресою");
                    return View(existAddress);
                }

                try
                {
                    _context.Update(cinema);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CinemaExists(cinema.CinemaId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cinema);
        }

        // GET: Cinemas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Cinemas == null)
            {
                return NotFound();
            }

            var cinema = await _context.Cinemas
                .FirstOrDefaultAsync(m => m.CinemaId == id);
            if (cinema == null)
            {
                return NotFound();
            }

            return View(cinema);
        }

        // POST: Cinemas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Cinemas == null)
            {
                return Problem("Entity set 'CinemaContext.Cinemas'  is null.");
            }
            var cinema = await _context.Cinemas
                .Include(c => c.Halls)
                .FirstOrDefaultAsync(c => c.CinemaId == id);

            var sessions = await _context.Halls
                .Where(b => b.CinemaId == id)
                .Include(b => b.Sessions)
                .FirstOrDefaultAsync();

            if (cinema != null)
            {
                foreach(var s in sessions.Sessions)
                    _context.Remove(s);

                foreach(var s in cinema.Halls)
                    _context.Halls.Remove(s);

                _context.Cinemas.Remove(cinema);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CinemaExists(int id)
        {
          return (_context.Cinemas?.Any(e => e.CinemaId == id)).GetValueOrDefault();
        }

        public ActionResult Export()
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {

                var cinemas = MovieStatic.cinemas;

                ViewBag.hidden = 1;
                if (cinemas.Count == 0) return View("Index", cinemas);

                foreach (var cinema in cinemas)
                {
                    var worksheet = workbook.Worksheets.Add(cinema.CinemaName);
                    worksheet.Cell("A1").Value = "Назва";
                    worksheet.Cell("B1").Value = "Адреса";
                    worksheet.Row(1).Style.Font.Bold = true;

                    worksheet.Cell(2, 1).Value = cinema.CinemaName;
                    worksheet.Cell(2, 2).Value = cinema.CinemaAddress;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();
                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"cinemas.xlsx"
                    };
                }
            }
        }
    }
}
