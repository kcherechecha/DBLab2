using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LabProject.Models;
using Microsoft.Data.SqlClient;

namespace LabProject.Controllers
{
    public class CastMembersController : Controller
    {
        private readonly CinemaContext _context;

        public CastMembersController(CinemaContext context)
        {
            _context = context;
        }

        // GET: CastMembers
        public async Task<IActionResult> Index()
        {
              return View(await _context.CastMembers.ToListAsync());
        }

        //Знайти акторів які знімалися у тих самих фільмах, що і заданий актор
        public async Task<IActionResult> ActorComapre(string actorName, int hidden)
        {


            string query = @"
                SELECT DISTINCT cm.CastMemberFullName
                FROM MovieCast mc
                INNER JOIN CastMember cm ON cm.CastMemberId = mc.CastMemberId
                WHERE mc.MovieId IN (
                SELECT mc2.MovieId
                FROM MovieCast mc2
                INNER JOIN CastMember cm2 ON cm2.CastMemberId = mc2.CastMemberId
                WHERE cm2.CastMemberFullName = @ActorName
                )
                AND cm.CastMemberFullName <> @ActorName";

            List<CastMember> actors = new List<CastMember>();

            using (SqlConnection connection = new SqlConnection(@"Server=DESKTOP-9O78KC4\SQLEXPRESS; Database=Cinema; Trusted_Connection=True; MultipleActiveResultSets=true; TrustServerCertificate = true"))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ActorName", actorName);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string actor = reader.GetString(0);
                            actors.Add(_context.CastMembers.FirstOrDefault(c => c.CastMemberFullName == actor));   
                        }
                    }
                }
            }
            ViewBag.hidden = hidden;
            return View("Index", actors);
        }

        // GET: CastMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.CastMembers == null)
            {
                return NotFound();
            }

            var castMember = await _context.CastMembers
                .FirstOrDefaultAsync(m => m.CastMemberId == id);
            if (castMember == null)
            {
                return NotFound();
            }

            return View(castMember);
        }

        // GET: CastMembers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CastMembers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CastMemberId,CastMemberFullName")] CastMember castMember)
        {
            if (ModelState.IsValid)
            {
                var existMemberName = await _context.CastMembers.FirstOrDefaultAsync(c => c.CastMemberFullName == castMember.CastMemberFullName);
                if (existMemberName != null)
                {
                    ModelState.AddModelError("CastMemberFullName", "Ця людина вже існує");
                    return View(existMemberName);
                }
                _context.Add(castMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(castMember);
        }

        // GET: CastMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.CastMembers == null)
            {
                return NotFound();
            }

            var castMember = await _context.CastMembers.FindAsync(id);
            if (castMember == null)
            {
                return NotFound();
            }
            return View(castMember);
        }

        // POST: CastMembers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CastMemberId,CastMemberFullName")] CastMember castMember)
        {
            if (id != castMember.CastMemberId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existMemberName = await _context.CastMembers.FirstOrDefaultAsync(c => c.CastMemberId != castMember.CastMemberId && c.CastMemberFullName == castMember.CastMemberFullName);
                if (existMemberName != null)
                {
                    ModelState.AddModelError("CastMemberFullName", "Ця людина вже існує");
                    return View(existMemberName);
                }
                try
                {
                    _context.Update(castMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CastMemberExists(castMember.CastMemberId))
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
            return View(castMember);
        }

        // GET: CastMembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.CastMembers == null)
            {
                return NotFound();
            }

            var castMember = await _context.CastMembers
                .FirstOrDefaultAsync(m => m.CastMemberId == id);
            if (castMember == null)
            {
                return NotFound();
            }

            return View(castMember);
        }

        // POST: CastMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CastMembers == null)
            {
                return Problem("Entity set 'CinemaContext.CastMembers'  is null.");
            }
            var castMember = await _context.CastMembers
                .Include(m => m.MovieCasts)
                .FirstOrDefaultAsync(m => m.CastMemberId == id);

            var movieCast = await _context.MovieCasts.FirstOrDefaultAsync(m => m.CastMemberId == id);
            int movieId = movieCast.MovieId;

            if (castMember != null)
            {
                foreach (var c in castMember.MovieCasts)
                    _context.Remove(c);
                _context.CastMembers.Remove(castMember);
            }
            
            await _context.SaveChangesAsync();

            var movieCastExist = await _context.MovieCasts
                .FirstOrDefaultAsync(m => m.MovieId == movieId);
            if (movieCastExist == null)
            {
                var movie = await _context.Movies
                .Include(m => m.MovieGenres)
                .Include(m => m.MovieCasts)
                .Include(m => m.Sessions)
                .FirstOrDefaultAsync(m => m.MovieId == movieId);

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
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CastMemberExists(int id)
        {
          return _context.CastMembers.Any(e => e.CastMemberId == id);
        }
    }
}
