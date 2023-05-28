using LabProject.Models;

namespace LabProject.Controllers
{
    public static class MovieStatic
    {
        public static List<Movie> movies = new List<Movie>();
        public static List<Session> sessions = new List<Session>();
        public static List<Hall> halls = new List<Hall>();      
        public static List<Cinema> cinemas= new List<Cinema>();

        public static void movieSet(List<Movie> getmovies) 
        {
            movies.Clear();
            foreach (var movie in getmovies)
            {
                movies.Add(movie);
            }
        }

        public static void hallSet(List<Hall> getmovies)
        {
            halls.Clear();
            foreach (var movie in getmovies)
            {
                halls.Add(movie);
            }
        }

        public static void sessionSet(List<Session> getmovies)
        {
            sessions.Clear();
            foreach (var movie in getmovies)
            {
                sessions.Add(movie);
            }
        }

        public static void cinemaSet(List<Cinema> getmovies)
        {
            cinemas.Clear();
            foreach (var movie in getmovies)
            {
                cinemas.Add(movie);
            }
        }
    }
}
