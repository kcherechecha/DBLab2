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
    public class MovieGenresController : Controller
    {
        private readonly CinemaContext _context;

        public MovieGenresController(CinemaContext context)
        {
            _context = context;
        }

        // GET: MovieGenres
        public async Task<IActionResult> Index()
        {
            var cinemaContext = _context.MovieGenres.Include(m => m.Genre).Include(m => m.Movie);
            return View(await cinemaContext.ToListAsync());
        }

        // GET: MovieGenres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MovieGenres == null)
            {
                return NotFound();
            }

            var movieGenre = await _context.MovieGenres
                .Include(m => m.Genre)
                .Include(m => m.Movie)
                .FirstOrDefaultAsync(m => m.MovieGenreId == id);
            if (movieGenre == null)
            {
                return NotFound();
            }

            return View(movieGenre);
        }

        // GET: MovieGenres/Create
        public IActionResult Create()
        {
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "GenreId");
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId");
            return View();
        }

        // POST: MovieGenres/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MovieGenreId,MovieId,GenreId")] MovieGenre movieGenre)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movieGenre);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "GenreId", movieGenre.GenreId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId", movieGenre.MovieId);
            return View(movieGenre);
        }

        // GET: MovieGenres/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MovieGenres == null)
            {
                return NotFound();
            }

            var movieGenre = await _context.MovieGenres.FindAsync(id);
            if (movieGenre == null)
            {
                return NotFound();
            }
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "GenreId", movieGenre.GenreId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId", movieGenre.MovieId);
            return View(movieGenre);
        }

        // POST: MovieGenres/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MovieGenreId,MovieId,GenreId")] MovieGenre movieGenre)
        {
            if (id != movieGenre.MovieGenreId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movieGenre);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieGenreExists(movieGenre.MovieGenreId))
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
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "GenreId", movieGenre.GenreId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId", movieGenre.MovieId);
            return View(movieGenre);
        }

        // GET: MovieGenres/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MovieGenres == null)
            {
                return NotFound();
            }

            var movieGenre = await _context.MovieGenres
                .Include(m => m.Genre)
                .Include(m => m.Movie)
                .FirstOrDefaultAsync(m => m.MovieGenreId == id);
            if (movieGenre == null)
            {
                return NotFound();
            }

            return View(movieGenre);
        }

        // POST: MovieGenres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MovieGenres == null)
            {
                return Problem("Entity set 'CinemaContext.MovieGenres'  is null.");
            }
            var movieGenre = await _context.MovieGenres.FindAsync(id);
            if (movieGenre != null)
            {
                _context.MovieGenres.Remove(movieGenre);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieGenreExists(int id)
        {
          return _context.MovieGenres.Any(e => e.MovieGenreId == id);
        }

     
    }
}
