using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LabProject.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Data.SqlClient;

namespace LabProject.Controllers
{
    public class SessionsController : Controller
    {
        private readonly CinemaContext _context;

        public SessionsController(CinemaContext context)
        {
            _context = context;
        }

        // GET: Sessions
        public async Task<IActionResult> Index(int? id, string? name)
        {
            if (id == null) return RedirectToAction("Cinemas", "Index");
            ViewBag.HallId = id;
            ViewBag.HallName = name;
            var sessionsByHall = _context.Sessions.Where(b => b.HallId == id).Include(b => b.Hall).Include(b => b.Movie).Include(b => b.Status);
            return View(await sessionsByHall.ToListAsync());
        }

        public async Task<IActionResult> SessionsInCity(string cityName, string movieName, int hidden)
        {

            var cinema = _context.Cinemas.Where(c => c.CinemaName == cityName);

            var query = _context.Sessions
                .Include(h => h.Hall)
                    .ThenInclude(c => c.Cinema)
                .Include(m => m.Movie)
                .Include(s => s.Status)
                .Where(m => m.Movie.MovieName == movieName && m.Hall.Cinema.CinemaAddress == cityName).ToList();

            //string query = @"
            //SELECT s.SessionId
            //FROM Session s
            //JOIN Hall h ON s.HallId = h.HallId
            //JOIN Cinema c ON h.CinemaId = c.CinemaId
            //JOIN Movie m ON s.MovieId = m.MovieId
            //WHERE c.CinemaAddress = @CinemaAddress
            //AND m.MovieName = @MovieName";

            //List<Session> sessions = new List<Session>();

            //using (SqlConnection connection = new SqlConnection(@"Server=DESKTOP-9O78KC4\SQLEXPRESS; Database=Cinema; Trusted_Connection=True; MultipleActiveResultSets=true; TrustServerCertificate = true"))
            //{
            //    using (SqlCommand command = new SqlCommand(query, connection))
            //    {
            //        command.Parameters.AddWithValue("@CinemaAddress", cityName);
            //        command.Parameters.AddWithValue("@MovieName", movieName);

            //        connection.Open();
            //        using (SqlDataReader reader = command.ExecuteReader())
            //        {
            //            while (reader.Read())
            //            {
            //                int sessionDbId = reader.GetInt32(0);
            //                sessions.Add(_context.Sessions.FirstOrDefault(s => s.SessionId == sessionDbId));
            //            }
            //        }
            //    }
            //}

            ViewBag.hidden = hidden;
            return View("Index", query);
        }

        // GET: Sessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Sessions == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(m => m.SessionId == id);

            if (session == null)
            {
                return NotFound();
            }

            var movieId = session.MovieId;

            //return View(session);
            //return RedirectToAction("Index", "Movies", new { id = session.SessionId, name = session.SessionNumber });
            return RedirectToAction("ShowMovie", "Movies", new { id = movieId });
        }

        // GET: Sessions/Create
        public IActionResult Create(int hallId)
        {
            //ViewData["HallId"] = new SelectList(_context.Halls, "HallId", "HallId");
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieName");
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName");

            ViewBag.HallId = hallId;
            return View();
        }

        // POST: Sessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int hallId, [Bind("SessionId,SessionNumber,SessionDateTime,HallId,MovieId,StatusId")] Session session)
        {

            session.HallId = hallId;

            if (ModelState.IsValid)
            {

                DateTime enteredDate = session.SessionDateTime;
                DateTime curDate = DateTime.UtcNow;

                var movie = await _context.Movies.FirstOrDefaultAsync(m => m.MovieId == session.MovieId);
                DateTime movieDate = movie.MovieReleaseDate;

                DateTime startDate = new DateTime(2022, 1, 1);
                DateTime endDate = new DateTime(2024, 12, 31);

                if (enteredDate <= startDate || enteredDate >= endDate)
                {
                    ViewBag.HallId = hallId;
                    ModelState.AddModelError("SessionDateTime", "Неможливо призначити таку дату (01.01.2022-21.12.2024)");
                    ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieName", session.MovieId);
                    ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName");
                    return View(session);
                }

                if(enteredDate > curDate && session.StatusId == 3 || enteredDate < curDate && session.StatusId == 1)
                {
                    ViewBag.HallId = hallId;
                    ModelState.AddModelError("SessionDateTime", "Оберіть відповідний статус");
                    ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieName", session.MovieId);
                    ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName");
                    return View(session);
                }

                if(enteredDate < movieDate)
                {
                    ViewBag.HallId = hallId;
                    ModelState.AddModelError("SessionDateTime", "Неможливо призначити сеанс, фільм ще не вийшов");
                    ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieName", session.MovieId);
                    ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName");
                    return View(session);
                }

                    var existSessionName = await _context.Sessions.FirstOrDefaultAsync(c => c.SessionNumber == session.SessionNumber && c.HallId == hallId);

                if (existSessionName != null)
                {
                    ViewBag.HallId = hallId;
                    ModelState.AddModelError("SessionNumber", "Такий номер сеансу вже існує");
                    return View(existSessionName);
                }
                _context.Add(session);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Index", "Sessions", new { id = hallId, name = _context.Halls.Where(c => c.HallId == hallId).FirstOrDefault().HallName });
            }
            //ViewData["HallId"] = new SelectList(_context.Halls, "HallId", "HallId", session.HallId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId", session.MovieId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName");
            return View(session);
            //RedirectToAction("Index", "Sessions", new { id = hallId, name = _context.Halls.Where(c => c.HallId == hallId).FirstOrDefault().HallName });
        }

        // GET: Sessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Sessions == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            ViewData["HallId"] = new SelectList(_context.Halls, "HallId", "HallId", session.HallId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieName", session.MovieId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName");
            return View(session);
        }

        // POST: Sessions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SessionId,SessionNumber,SessionDateTime,StatusId,HallId,MovieId")] Session session)
        {
            if (id != session.SessionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(session);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.SessionId))
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
            ViewData["HallId"] = new SelectList(_context.Halls, "HallId", "HallId", session.HallId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId", session.MovieId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName");
            return View(session);
        }

        // GET: Sessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Sessions == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .Include(s => s.Status)
                .FirstOrDefaultAsync(m => m.SessionId == id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        // POST: Sessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            
            if (_context.Sessions == null)
            {
                return Problem("Entity set 'CinemaContext.Sessions'  is null.");
            }
            var session = await _context.Sessions.FindAsync(id);

            int hallId = session.HallId;
            var hall = await _context.Halls.FindAsync(hallId);
            string hallName = hall.HallName;
            if (session != null)
            {
                _context.Sessions.Remove(session);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new {id = hallId, name = hallName});
        }

        private bool SessionExists(int id)
        {
          return (_context.Sessions?.Any(e => e.SessionId == id)).GetValueOrDefault();
        }
    }
}
