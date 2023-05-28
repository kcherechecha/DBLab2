using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LabProject.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ClosedXML.Excel;
using static System.Reflection.Metadata.BlobBuilder;
using DocumentFormat.OpenXml.InkML;
using System.Numerics;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
using Microsoft.Data.SqlClient;

namespace LabProject.Controllers
{
    public class MoviesController : Controller
    {
        private readonly CinemaContext _context;

        public MoviesController(CinemaContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string importSuccess)
        {
            ViewBag.ImportSuccess = importSuccess;
            var movieList = _context.Movies;
            return View(await movieList.ToListAsync());

            //if (id == null) return View(await _context.Movies.ToListAsync()); 
            //ViewBag.SessionId = id;
            //ViewBag.SessionName = name;
            //var moviesBySessions = _context.Movies.Where(b => b. == id).Include(b => b.Session);
            //return View(await moviesBySessions.ToListAsync());
        }

        public async Task<IActionResult> ShowMovie(int? id)
        {
            if (id == null) return RedirectToAction("Session", "Index");
            ViewBag.SessionId = id;
            var movieBySession = _context.Movies.Where(b => b.MovieId == id);
            return View(await movieBySession.ToListAsync());
        }

        //Знайти список фільмів які показують за певною локацією
        public async Task<IActionResult> MoviesByLocation(string location, int hidden)
        {
            var query = _context.Sessions
                .Include(h => h.Hall)
                    .ThenInclude(c => c.Cinema)
                .Include(m => m.Movie)
                .Where(c => c.Hall.Cinema.CinemaAddress == location).ToList();

            List<Movie> movies = new List<Movie>();

            foreach (var movie in query)
                movies.Add(movie.Movie);
                    
            ViewBag.hidden = hidden;
            MovieStatic.movieSet(movies.Distinct().ToList());
            return View("Index", movies.Distinct());
        }

        public async Task<IActionResult> MoviesByGenre(int movieId, int hidden)
        {
            
            string query = @"
                    SELECT m.MovieId
                    FROM Movie m
                    WHERE NOT EXISTS (
                        SELECT mg1.GenreId
                        FROM MovieGenre mg1
                        WHERE mg1.MovieId = @MovieId
                        EXCEPT
                        SELECT mg2.GenreId
                        FROM MovieGenre mg2
                        WHERE mg2.MovieId = m.MovieId
                        )
                    AND NOT EXISTS (
                    SELECT mg3.GenreId
                    FROM MovieGenre mg3
                    WHERE mg3.MovieId = m.MovieId
                    EXCEPT
                    SELECT mg4.GenreId
                    FROM MovieGenre mg4
                    WHERE mg4.MovieId = @MovieId
                    )";


            List<Movie> movies = new List<Movie>();

            using (SqlConnection connection = new SqlConnection(@"Server =DESKTOP-9O78KC4\SQLEXPRESS; Database=Cinema; Trusted_Connection=True; MultipleActiveResultSets=true; TrustServerCertificate = true"))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MovieId", movieId);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int moviemdbId = reader.GetInt32(0);
                            var obj = _context.Movies.FirstOrDefault(m => m.MovieId == moviemdbId && m.MovieId != movieId);
                            if(obj != null)
                                movies.Add(obj);
                        }
                    }
                }
            }
            ViewBag.hidden = hidden;
            MovieStatic.movieSet(movies);
            return View("Index", movies);
        }



        public async Task<IActionResult> MoviesNoGenres(int movieId, int hidden)
        {
            List<Movie> movies = new List<Movie>();

            using (SqlConnection connection = new SqlConnection(@"Server=DESKTOP-9O78KC4\SQLEXPRESS; Database=Cinema; Trusted_Connection=True; MultipleActiveResultSets=true; TrustServerCertificate = true"))
            {
                string query = @"
                    SELECT DISTINCT m.MovieId
                    FROM Movie m
                        WHERE NOT EXISTS (
                        SELECT *
                        FROM MovieGenre mg
                        WHERE mg.MovieId = @MovieId
                            AND mg.GenreId IN (
                        SELECT mg2.GenreId
                        FROM MovieGenre mg2
                        WHERE mg2.MovieId = m.MovieId
                        )
                    )";

                

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MovieId", movieId); 

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int moviemdbId = reader.GetInt32(0);
                            movies.Add(_context.Movies.FirstOrDefault(m => m.MovieId == moviemdbId));
                        }
                    }
                }
            }
            ViewBag.hidden = hidden;
            MovieStatic.movieSet(movies);
            return View("Index", movies);
        }

        public async Task<IActionResult> MovieAllGenre(int movieId, int hidden)
        {
            List<Movie> movies = new List<Movie>();
            using (SqlConnection connection = new SqlConnection(@"Server=DESKTOP-9O78KC4\SQLEXPRESS; Database=Cinema; Trusted_Connection=True; MultipleActiveResultSets=true; TrustServerCertificate = true"))
            {
                string query = @"
                    SELECT m.MovieId
                    FROM Movie m
                    WHERE NOT EXISTS (
                        SELECT mg1.GenreId
                        FROM MovieGenre mg1
                        WHERE mg1.MovieId = @MovieId
                        EXCEPT
                        SELECT mg2.GenreId
                        FROM MovieGenre mg2
                        WHERE mg2.MovieId = m.MovieId
                    )";
                   

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MovieId", movieId); // Замініть movieId на ідентифікатор заданого фільму

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int moviedbId = reader.GetInt32(0);
                            var obj = _context.Movies.FirstOrDefault(m => m.MovieId == moviedbId && m.MovieId != movieId);
                            if (obj != null)
                                movies.Add(obj);
                            // Виконання необхідної логіки з результатами запиту
                        }
                    }
                }
            }
            ViewBag.hidden = hidden;
            MovieStatic.movieSet(movies);
            return View("Index", movies);
        }

            // GET: Movies/Details/5
            public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Movies == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null)
            {
                return NotFound();
            }

            return RedirectToAction("Index", "MovieCasts", new {movieId = movie.MovieId, movieName = movie.MovieName});
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MovieId,MovieName,MovieDuration,MovieRating,MovieReleaseDate")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                var existMovieName = await _context.Movies.FirstOrDefaultAsync(c => c.MovieName == movie.MovieName);
                if (existMovieName != null)
                {
                    ModelState.AddModelError("MovieName", "Вже існує фільм з такою назвою");
                    return View(existMovieName);
                }

                DateTime enteredDate = movie.MovieReleaseDate;

                DateTime startDate = new DateTime(1985, 1, 1);
                DateTime endDate = new DateTime(2027, 12, 31);

                if (enteredDate <= startDate || enteredDate >= endDate)
                {
                    ModelState.AddModelError("MovieReleaseDate", "Неможливо призначити таку дату (01.01.1985-31.12.2027");
                    return View(movie);
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Index", "Genres", new { movieId = movie.MovieId });
            }
             return View(movie);
            //return RedirectToAction("Index", "Genres", new { Movieid = movie.MovieId });
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Movies == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MovieId,MovieName,MovieDuration,MovieRating,MovieReleaseDate")] Movie movie)
        {
            if (id != movie.MovieId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existMovieName = await _context.Movies.FirstOrDefaultAsync(c => c.MovieId != movie.MovieId && c.MovieName == movie.MovieName);

                if (existMovieName != null)
                {
                    ModelState.AddModelError("MovieName", "Вже існує фільм з такою назвою");
                    return View(existMovieName);
                }
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.MovieId))
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
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Movies == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Movies == null)
            {
                return Problem("Entity set 'CinemaContext.Movies'  is null.");
            }
            var movie = await _context.Movies
                .Include(m => m.MovieGenres)
                .Include(m => m.MovieCasts)
                .Include(m => m.Sessions)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie != null)
            {
                foreach (var m in movie.Sessions)
                    _context.Remove(m);

                foreach (var m in movie.MovieGenres)
                    _context.Remove(m);

                foreach (var m in movie.MovieCasts)
                    _context.Remove(m);

                _context.Movies.Remove(movie);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
          return _context.Movies.Any(e => e.MovieId == id);
        }

        // 1.6

       
        public ActionResult Export()
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {

                var movies = MovieStatic.movies;

                ViewBag.hidden = 1;
                if (movies.Count == 0) return View("Index", movies);

                foreach (var movie in movies)
                {
                    var worksheet = workbook.Worksheets.Add(movie.MovieName);
                    worksheet.Cell("A1").Value = "Назва";
                    worksheet.Cell("B1").Value = "Тривалість";
                    worksheet.Cell("C1").Value = "Рейтинг";
                    worksheet.Cell("D1").Value = "Дата виходу";
                    worksheet.Row(1).Style.Font.Bold = true;

                    worksheet.Cell(2, 1).Value = movie.MovieName;
                    worksheet.Cell(2, 2).Value = movie.MovieDuration;
                    worksheet.Cell(2, 3).Value = movie.MovieRating;
                    worksheet.Cell(2, 4).Value = movie.MovieReleaseDate;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();
                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"movies.xlsx"
                    };
                }
            }
        }
    }
   
}