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
            
            return View("Index", movies.Distinct());
        }

        public async Task<IActionResult> MoviesByGenre(string movieId, int hidden)
        {
            //Знайти фільми, які містять такі самі жанри, що і заданий фільм

            string query = @"
                SELECT DISTINCT m.MovieId
                FROM Movie m
                WHERE m.MovieId <> @MovieId
                    AND NOT EXISTS (
                    SELECT g.GenreId
                    FROM MovieGenre mg
                    INNER JOIN Genre g ON g.GenreId = mg.GenreId
                    WHERE mg.MovieId = @MovieId
                    AND g.GenreId NOT IN (
                    SELECT mg2.GenreId
                    FROM MovieGenre mg2
                    WHERE mg2.MovieId = m.MovieId
                    )
                )";

            List<Movie> movies = new List<Movie>();

            using (SqlConnection connection = new SqlConnection(@"Server=DESKTOP-9O78KC4\SQLEXPRESS; Database=Cinema; Trusted_Connection=True; MultipleActiveResultSets=true; TrustServerCertificate = true"))
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
                            movies.Add(_context.Movies.FirstOrDefault(m => m.MovieId == moviemdbId));
                        }
                    }
                }
            }
            ViewBag.hidden = hidden;
            return View("Index", movies);
        }

        public async Task<IActionResult> MoviesNoGenres(string movieId, int hidden)
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
            return View("Index", movies);
        }

        public async Task<IActionResult> MovieAllGenre(string movieId, int hidden)
        {
            List<Movie> movies = new List<Movie>();
            using (SqlConnection connection = new SqlConnection(@"Server=DESKTOP-9O78KC4\SQLEXPRESS; Database=Cinema; Trusted_Connection=True; MultipleActiveResultSets=true; TrustServerCertificate = true"))
            {
                string query = @"
                    SELECT DISTINCT m.MovieId
                    FROM Movie m
                    WHERE m.MovieId <> @MovieId
                    AND EXISTS (
                    SELECT mg.GenreId
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
                    command.Parameters.AddWithValue("@MovieId", movieId); // Замініть movieId на ідентифікатор заданого фільму

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int moviedbId = reader.GetInt32(0);
                            movies.Add(_context.Movies.FirstOrDefault(m => m.MovieId == moviedbId));
                            // Виконання необхідної логіки з результатами запиту
                        }
                    }
                }
            }
            ViewBag.hidden = hidden;
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

        public async Task<IActionResult> ImportFromExcel(IFormFile fileExcel)
        {
            string importSuccess = "Файл завнтажено успішно. ";
            if (fileExcel != null && fileExcel.Length > 0)
            {
                using (var stream = fileExcel.OpenReadStream())
                {
                    try
                    {
                        XLWorkbook workbook = new XLWorkbook(stream);
                    }
                    catch
                    {
                        return RedirectToAction("Index", new { importSuccess = "Формат файлу невірний" });
                    }
                    using (XLWorkbook workbook = new XLWorkbook(stream))
                    {
                        int worksheetPos = 1;
                        var worksheet = workbook.Worksheet(worksheetPos);
                        var row = 2;
                        var failedAdd = new List<int>();
                        var movies = new List<Movie>();
                        while (true)
                        {
                            if (worksheet.Cell(row, 1).IsEmpty())
                            {
                                row = 2;
                                worksheetPos++;
                                try
                                {
                                    worksheet = workbook.Worksheet(worksheetPos);
                                }
                                catch
                                {
                                    break;
                                }
                            }

                            var movie = new Movie();

                            movie.MovieName = worksheet.Cell(row, 1).GetValue<string>();
                            movie.MovieDuration = worksheet.Cell(row, 2).GetValue<int>();
                            movie.MovieRating = worksheet.Cell(row, 3).GetValue<int>();
                            movie.MovieReleaseDate = worksheet.Cell(row, 4).GetValue<DateTime>();

                            if(movie.MovieRating > 5 || movie.MovieRating < 1)
                            {
                                importSuccess += $"{movie.MovieName} не був доданий, оскільки рейтинг не відповідає вимогам. ";
                                failedAdd.Add(worksheetPos);
                                row++;
                                continue;
                            }

                            DateTime importDate = movie.MovieReleaseDate;

                            DateTime startDate = new DateTime(1985, 1, 1);
                            DateTime endDate = new DateTime(2027, 12, 31);

                            if (importDate <= startDate || importDate >= endDate)
                            {
                                importSuccess += $"{movie.MovieName} не був доданий, оскільки дата не віповідає вимогам. ";
                                failedAdd.Add(worksheetPos);
                                row++;
                                continue;
                            }

                            var movieCheck = _context.Movies.FirstOrDefault(m => m.MovieName == movie.MovieName);

                            if (movieCheck != null)
                            {
                                importSuccess += $"{movie.MovieName} не був доданий, оскільки вже існує в таблиці. ";
                                failedAdd.Add(worksheetPos);
                                row++;
                                continue;
                            }

                            movies.Add(movie);
                            row++;
                        }

                        if(movies.Count == 0) return RedirectToAction("Index", new { importSuccess = "Жоден фільм не було додано. " });
                        await _context.Movies.AddRangeAsync(movies);
                        await _context.SaveChangesAsync();
                        row = 2;
                        worksheetPos = 1;
                        worksheet = workbook.Worksheet(worksheetPos);
                        int movieCount = 0;
                        var movieCasts = new List<MovieCast>();

                        while (true)
                        {
                            if(failedAdd.Contains(worksheetPos))
                            {
                                row = 2;
                                worksheetPos++;
                                try
                                {
                                    worksheet = workbook.Worksheet(worksheetPos);
                                }
                                catch
                                {
                                    break;
                                }
                                continue;
                            }

                            if (worksheet.Cell(row, 5).IsEmpty())
                            {
                                row = 2;
                                worksheetPos++;
                                try
                                {
                                    worksheet = workbook.Worksheet(worksheetPos);
                                }
                                catch
                                {
                                    break;
                                }
                                movieCount++;
                                continue;
                            }

                            var castMember = new CastMember();
                            var position = new Position();
                            var movieCast = new MovieCast();

                            castMember.CastMemberFullName = worksheet.Cell(row,5).GetValue<string>();

                            var castMemberCheck = _context.CastMembers.FirstOrDefault(c => c.CastMemberFullName == castMember.CastMemberFullName);

                            if(castMemberCheck == null)
                            {
                                await _context.CastMembers.AddAsync(castMember);
                                await _context.SaveChangesAsync();
                                movieCast.CastMemberId = castMember.CastMemberId;
                            }
                            else
                                movieCast.CastMemberId = castMemberCheck.CastMemberId;

                            position.PositionName = worksheet.Cell(row,6).GetValue<string>();

                            var positionCheck = _context.Positions.FirstOrDefault(c => c.PositionName == position.PositionName);

                            if (positionCheck == null)
                            {
                                await _context.Positions.AddAsync(position);
                                await _context.SaveChangesAsync();
                                movieCast.PositionId = position.PositionId;
                            }
                            else
                            { 
                                movieCast.PositionId = positionCheck.PositionId;
                            }

                            movieCast.MovieId = movies[movieCount].MovieId;

                            movieCasts.Add(movieCast);
                            row++;

                        }
                        await _context.MovieCasts.AddRangeAsync(movieCasts);
                        await _context.SaveChangesAsync();

                        row = 2;
                        worksheetPos = 1;
                        worksheet = workbook.Worksheet(worksheetPos);
                        movieCount = 0;
                        var movieGenres = new List<MovieGenre>();

                        while (true)
                        {
                            if (failedAdd.Contains(worksheetPos))
                            {
                                row = 2;
                                worksheetPos++;
                                try
                                {
                                    worksheet = workbook.Worksheet(worksheetPos);
                                }
                                catch
                                {
                                    break;
                                }
                                continue;
                            }

                            if (worksheet.Cell(row, 7).IsEmpty())
                            {
                                row = 2;
                                worksheetPos++;
                                try
                                {
                                    worksheet = workbook.Worksheet(worksheetPos);
                                }
                                catch
                                {
                                    break;
                                }
                                movieCount++;
                                continue;
                            }

                            var genre = new Genre();
                            var movieGenre = new MovieGenre();

                            genre.GenreName = worksheet.Cell(row, 7).GetValue<string>();

                            var genreCheck = _context.Genres.FirstOrDefault(g => g.GenreName == genre.GenreName);

                            if (genreCheck == null)
                            {
                                await _context.Genres.AddAsync(genre);
                                await _context.SaveChangesAsync();
                                movieGenre.GenreId = genre.GenreId;
                            }
                            else
                                movieGenre.GenreId = genreCheck.GenreId;

                            movieGenre.MovieId = movies[movieCount].MovieId;

                            movieGenres.Add(movieGenre);
                            row++;
                        }
                        await _context.MovieGenres.AddRangeAsync(movieGenres);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            if (fileExcel == null) return RedirectToAction("Index", new { importSuccess = "Ви не вибрали файл для завантаження" });
            if (fileExcel.Length < 0) return RedirectToAction("Index", new { importSuccess = "Вибраний файл пустий" });
            return RedirectToAction("Index", new { importSuccess = importSuccess });
        }

        public ActionResult Export()
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {
                var movies = _context.Movies
               .Include(m => m.MovieCasts)
                   .ThenInclude(mc => mc.CastMember)
               .Include(m => m.MovieCasts)
                   .ThenInclude(mc => mc.Position)
               .Include(m => m.MovieGenres)
                   .ThenInclude(g => g.Genre)
               .ToList();


                foreach (var movie in movies)
                {
                    var worksheet = workbook.Worksheets.Add(movie.MovieName);
                    worksheet.Cell("A1").Value = "Назва";
                    worksheet.Cell("B1").Value = "Тривалість";
                    worksheet.Cell("C1").Value = "Рейтинг";
                    worksheet.Cell("D1").Value = "Дата виходу";
                    worksheet.Cell("E1").Value = "Член команди";
                    worksheet.Cell("F1").Value = "Позиція";
                    worksheet.Cell("G1").Value = "Жанр";
                    worksheet.Row(1).Style.Font.Bold = true;

                    worksheet.Cell(2, 1).Value = movie.MovieName;
                    worksheet.Cell(2, 2).Value = movie.MovieDuration;
                    worksheet.Cell(2, 3).Value = movie.MovieRating;
                    worksheet.Cell(2, 4).Value = movie.MovieReleaseDate;

                    int row = 2;
                    foreach (var movieCast in movie.MovieCasts)
                    {
                        worksheet.Cell(row, 5).Value = movieCast.CastMember.CastMemberFullName;
                        worksheet.Cell(row, 6).Value = movieCast.Position.PositionName;
                        row++;
                    }
                    row = 2;
                    foreach(var genre in movie.MovieGenres)
                    {
                        worksheet.Cell(row,7).Value = genre.Genre.GenreName;
                        row++;
                    }
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