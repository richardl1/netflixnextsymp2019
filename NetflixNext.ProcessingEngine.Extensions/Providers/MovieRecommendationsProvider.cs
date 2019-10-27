using Microsoft.Extensions.Logging;
using NetflixNext.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace NetflixNext.ProcessingEngine.Extensions.Providers
{
    public class MovieRecommendationsProvider : IMovieRecommendationsProvider
    {
        private ILogger _logger;
        public MovieRecommendationsProvider(ILogger logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Accepts List of Movie ID's and calls Unogs to get movie details and computes parameters which will then be passed as search criteria to Unogs
        /// </summary>
        /// <param name="movieIds"></param>
        /// <returns></returns>
        public List<Movie> GetRecommendations(IEnumerable<string> movieIds)
        {
            var movies = new List<Movie>();
            var ratings = new List<Decimal>();
            StringBuilder genres = new StringBuilder();
            foreach (var movid in movieIds)
            {
                var movieDetails = GetMovieDetails(movid);               
                ratings.Add(decimal.Parse(movieDetails.RESULT.imdbinfo.rating));
                string movieGenres = string.Join(",", movieDetails.RESULT.Genreid);
                if (!string.IsNullOrEmpty(movieGenres))
                {
                    genres.Append(movieGenres);
                    // "," should not be added if it is last movie ID in IEnumerable to avoid UNogs to crash
                    if (movieIds.LastOrDefault() != movid)
                    {
                        genres.Append(",");
                    }
                }
            }
            var bRating = ratings.Sum(rating => rating) / ratings.Count();
                var response = Search(new SearchCriteria()
                {
                    BottomImdbRating = bRating.ToString(),
                    Query = string.Empty,
                    GenreId = genres.ToString()
                });

                movies.AddRange(response.Items);      


            return movies;
        }
/// <summary>
/// Gets the data needed from Unogs given the resource passed
/// Note that resource passed should respect format from UNogs expectation
/// </summary>
/// <param name="resource"></param>
/// <returns></returns>
        private string GetDataFromUnogs(string resource)
        {
            var content = string.Empty;

            using (var httpClient = new HttpClient())
            {
                try
                {                    
                    var url = $"https://unogs-unogs-v1.p.rapidapi.com/aaapi.cgi?{resource}";

                    httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "unogs-unogs-v1.p.rapidapi.com");
                    httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", "");

                    var response = httpClient.GetAsync(url).Result;
                    response.EnsureSuccessStatusCode();
                    content = response.Content.ReadAsStringAsync().Result;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogInformation("error on the movie recommendation provider", ex.Message);
                    throw;
                }
            }

            return content;
        }
        /// <summary>
        /// Search Criteria based search 
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        private MovieList Search(SearchCriteria searchCriteria)
        {
            var jsonData = GetDataFromUnogs($"q={searchCriteria.Query}-!{searchCriteria.StartYear}%2C{searchCriteria.EndYear}-!{searchCriteria.BottomNetflixRating}%2C{searchCriteria.UpperNetflixRating}-!{searchCriteria.BottomImdbRating}%2C{searchCriteria.UpperImdbRating}-!{searchCriteria.GenreId}-!Any-!Any-!Any-!gt100-!%7Bdownloadable%7D&t=ns&cl=78&st=adv&ob=Relevance");

            return JsonConvert.DeserializeObject<MovieList>(jsonData);
        }
        /// <summary>
        /// Gets the movie details from UNogs
        /// </summary>
        /// <param name="videoid"></param>
        /// <returns></returns>
        private MovieDetails GetMovieDetails(string videoid)
        {
            var jsonData = GetDataFromUnogs($"t=loadvideo&q={videoid}");

            return JsonConvert.DeserializeObject<MovieDetails>(jsonData);
        }
    }
}
