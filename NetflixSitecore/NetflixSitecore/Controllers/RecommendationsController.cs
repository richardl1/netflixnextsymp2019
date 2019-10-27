using System.Web.Mvc;
using IMovieRepository = NetflixSitecore.Services.IMovieRepository;
using MovieRepository = NetflixSitecore.Services.MovieRepository;

namespace NetflixSitecore.Controllers
{
    public class RecommendationsController : Controller
    {
        protected IMovieRepository MovieRepository { get; }

        public RecommendationsController()
        {
            MovieRepository = new MovieRepository();
        }
        
        public ActionResult Index()
        {
            var recommendations = MovieRepository.GetRecommendedMoviesFromFacet();

            return View(Views.Recommendations, recommendations);
        }
        

        protected static class Views
        {
            public const string Recommendations = "/Views/MovieListing/Recommendations.cshtml";
        }

    }
}