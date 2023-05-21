using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LabProject.Models;

namespace LabProject.Controllers
{
    public class MovieCastsController : Controller
    {
        private readonly CinemaContext _context;

        public MovieCastsController(CinemaContext context)
        {
            _context = context;
        }

        // GET: MovieCasts
        public async Task<IActionResult> Index(int movieId, string movieName)
        {
            var cinemaContext = _context.MovieCasts.Where(c => c.MovieId == movieId).Include(m => m.CastMember).Include(m => m.Movie).Include(m => m.Position);
            ViewBag.MovieId = movieId;
            ViewBag.MovieName = movieName;
            return View(await cinemaContext.ToListAsync());
        }

        public async Task<IActionResult> AddedMovieCastList(int movieId)
        {
            bool buttonCheck = true;

            var cinemaContext = await _context.MovieCasts.Where(c => c.MovieId == movieId).Include(m => m.CastMember).Include(m => m.Movie).Include(m => m.Position).ToListAsync();

            if (cinemaContext.Count == 0)
                buttonCheck = false;

            ViewBag.buttonCheck = buttonCheck;
            ViewBag.MovieId = movieId;
            return View(cinemaContext);
        }

        // GET: MovieCasts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MovieCasts == null)
            {
                return NotFound();
            }

            var movieCast = await _context.MovieCasts
                .Include(m => m.CastMember)
                .Include(m => m.Movie)
                .Include(m => m.Position)
                .FirstOrDefaultAsync(m => m.MovieCastId == id);
            if (movieCast == null)
            {
                return NotFound();
            }

            return View(movieCast);
        }

        // GET: MovieCasts/Create
        public IActionResult Create(int movieId)
        {
            ViewData["CastMemberId"] = new SelectList(_context.CastMembers, "CastMemberId", "CastMemberFullName");
            //ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId");
            ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionName");
            ViewBag.MovieId = movieId;
            return View();
        }

        // POST: MovieCasts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int movieId,[Bind("MovieCastId,CastMemberId,PositionId,MovieId")] MovieCast movieCast)
        {
            var preMovieCast = _context.MovieCasts.Where(m => m.MovieId == movieId).FirstOrDefault();
            if(preMovieCast != null && preMovieCast.CastMemberId == movieCast.CastMemberId)
            {
                // get message error "This actor already exist in this film" try to add again
                return RedirectToAction("Create", new { movieId = movieId });
            }
            movieCast.MovieId = movieId;
            if (ModelState.IsValid)
            {
                _context.Add(movieCast);
                await _context.SaveChangesAsync();
                return RedirectToAction("AddedMovieCastList", new {movieId});
            }
            ViewData["CastMemberId"] = new SelectList(_context.CastMembers, "CastMemberId", "CastMemberFullName", movieCast.CastMemberId);
            //ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId", movieCast.MovieId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionName", movieCast.PositionId);
            return View(movieCast);
        }

        public async Task<IActionResult> CreateToTable(int movieId, [Bind("MovieCastId,CastMemberId,PositionId,MovieId")] MovieCast movieCast)
        {
            var preMovieCast = _context.MovieCasts.Where(m => m.MovieId == movieId).FirstOrDefault();
            if (preMovieCast != null && preMovieCast.CastMemberId == movieCast.CastMemberId)
            {
                // get message error "This actor already exist in this film" try to add again
                return RedirectToAction("Create", new { movieId = movieId });
            }
            movieCast.MovieId = movieId;
            if (ModelState.IsValid)
            {
                _context.Add(movieCast);
                await _context.SaveChangesAsync();
                return View(nameof(Index));
            }
            ViewData["CastMemberId"] = new SelectList(_context.CastMembers, "CastMemberId", "CastMemberFullName", movieCast.CastMemberId);
            //ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId", movieCast.MovieId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionName", movieCast.PositionId);
            return View(movieCast);
        }
        // GET: MovieCasts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MovieCasts == null)
            {
                return NotFound();
            }

            var movieCast = await _context.MovieCasts.FindAsync(id);
            if (movieCast == null)
            {
                return NotFound();
            }
            ViewData["CastMemberId"] = new SelectList(_context.CastMembers, "CastMemberId", "CastMemberFullName", movieCast.CastMemberId);
            //ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId", movieCast.MovieId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionName", movieCast.PositionId);
            return View(movieCast);
        }

        // POST: MovieCasts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MovieCastId,CastMemberId,PositionId,MovieId")] MovieCast movieCast)
        {
            if (id != movieCast.MovieCastId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movieCast);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieCastExists(movieCast.MovieCastId))
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
            ViewData["CastMemberId"] = new SelectList(_context.CastMembers, "CastMemberId", "CastMemberId", movieCast.CastMemberId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId", movieCast.MovieId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionId", movieCast.PositionId);
            return View(movieCast);
        }

        // GET: MovieCasts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MovieCasts == null)
            {
                return NotFound();
            }

            var movieCast = await _context.MovieCasts
                .Include(m => m.CastMember)
                .Include(m => m.Movie)
                .Include(m => m.Position)
                .FirstOrDefaultAsync(m => m.MovieCastId == id);
            if (movieCast == null)
            {
                return NotFound();
            }

            return View(movieCast);
        }

        // POST: MovieCasts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MovieCasts == null)
            {
                return Problem("Entity set 'CinemaContext.MovieCasts'  is null.");
            }
            var movieCast = await _context.MovieCasts.FindAsync(id);
            int movieId = movieCast.MovieId;
            if (movieCast != null)
            {
                _context.MovieCasts.Remove(movieCast);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AddedMovieCastList), new { movieId });
        }

        private bool MovieCastExists(int id)
        {
          return _context.MovieCasts.Any(e => e.MovieCastId == id);
        }
    }
}
