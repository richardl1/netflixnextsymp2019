using NetflixNext.Common.Models;

namespace NetflixSitecore.Services
{
    public interface IMovieRepository
    {
        MovieList GetPopularMoviesListing();
        MovieList Search(SearchCriteria searchCriteria);
        GenreList GetGenres();
        MovieDetails GetMovieDetails(string videoid);
        MovieList GetHighestRated();
        MovieList WhatIsNew();
        MovieList GetRecommendedMoviesFromFacet();
    }
}
