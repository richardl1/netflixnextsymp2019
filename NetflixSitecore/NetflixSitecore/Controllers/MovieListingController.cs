using System.Web.Mvc;
using IMovieRepository = NetflixSitecore.Services.IMovieRepository;
using MovieRepository = NetflixSitecore.Services.MovieRepository;

namespace NetflixSitecore.Controllers
{
    public class MovieListingController : Controller
    {
        private IMovieRepository MovieRespository { get; set; }

        public MovieListingController()
        {
            MovieRespository = new MovieRepository();
        }

        // GET: MovieListing
        public ActionResult PopularListing()
        {
            var list = MovieRespository.GetPopularMoviesListing();
            
            return View(Views.PopularListingView, list);
        }

        public ActionResult NewReleaseListing()
        {
            var list = MovieRespository.WhatIsNew();
            return View(Views.NewReleaseListingView, list);
        }

        public ActionResult HighestRatedListing()
        {
            var list = MovieRespository.GetHighestRated();
            return View(Views.HighestRatedListingView, list);
        }

        protected static class Views
        {
            public const string NewReleaseListingView = "/Views/MovieListing/NewReleaseListing.cshtml";
            public const string HighestRatedListingView = "/Views/MovieListing/HightestRatedListing.cshtml";
            public const string PopularListingView = "/Views/MovieListing/PopularListing.cshtml";
        }

    }
}