using System.Web.Mvc;
using NetflixSitecore.Services;

namespace NetflixSitecore.Controllers
{
    public class MovieDetailController : Controller
    {
        private IMovieRepository MovieRepository { get; }
        protected IGoalsRepository GoalsRepository { get; }
        public MovieDetailController()
        {
            MovieRepository = new MovieRepository();
            GoalsRepository = new GoalsRepository();
        }

        public ActionResult MovieDetail(string videoId)
        {
            var movie = MovieRepository.GetMovieDetails(videoId);

            GoalsRepository.TriggerGoal(Services.GoalsRepository.GoalIds.MovieWatchedId, videoId, string.Join(",", movie.RESULT.Genreid), movie.RESULT.imdbinfo.rating);

            return View(Views.MovieDetailView, movie);
        }

        protected static class Views
        {
            public const string MovieDetailView = "/Views/MovieDetail/MovieDetail.cshtml";
        }
    }
}