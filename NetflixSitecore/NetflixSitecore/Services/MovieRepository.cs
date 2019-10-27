using NetflixNext.Common.Models;
using NetflixNext.xConnect.Extensions;
using Newtonsoft.Json;
using Sitecore.Analytics;
using Sitecore.Diagnostics;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using System;
using System.Linq;
using System.Net.Http;

namespace NetflixSitecore.Services
{
    public class MovieRepository : IMovieRepository
    {
        private GenreList ConvertUnogGenres(string jsonData)
        {
            var list = JsonConvert.DeserializeObject<GenreList>(jsonData);
            return list;
        }

        private MovieList ConvertUnogMovies(string jsonData)
        {
            var list = JsonConvert.DeserializeObject<MovieList>(jsonData);
            return list;
        }

        private MovieDetails ConvertUnogMovieDetails(string jsonData)
        {
            try
            {
                var list = JsonConvert.DeserializeObject<MovieDetails>(jsonData);
                return list;
            }
            //Exception seems to occur especially if there is an issue with IMDB Rating data on Unogs
            catch(Exception ex)
            {
                return new MovieDetails();
            }            
        }

        private string GetDataFromUnogs(string resource)
        {
            var content = string.Empty;

            using (var httpClient = new HttpClient())
            {
                    var url = $"https://unogs-unogs-v1.p.rapidapi.com/aaapi.cgi?{resource}";

                    httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "unogs-unogs-v1.p.rapidapi.com");
                    httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", "");

                    var response = httpClient.GetAsync(url).Result;
                    response.EnsureSuccessStatusCode();
                    content = response.Content.ReadAsStringAsync().Result;
            }

            return content;
        }

        public MovieList GetPopularMoviesListing()
        {

            var jsonData = GetDataFromUnogs("q=popularindex&st=bs");

            return ConvertUnogMovies(jsonData);
        }

        public MovieList WhatIsNew()
        {
            var jsonData = GetDataFromUnogs("q=what%27s+new+last+5+days&st=bs");

            return ConvertUnogMovies(jsonData);
        }

        public MovieList GetHighestRated()
        {
            var jsonData = GetDataFromUnogs("q=top250&st=bs");

            return ConvertUnogMovies(jsonData);
        }

        public MovieDetails GetMovieDetails(string videoid)
        {
            var jsonData = GetDataFromUnogs($"t=loadvideo&q={videoid}");

            return ConvertUnogMovieDetails(jsonData);
        }

        public GenreList GetGenres()
        {
            var jsonData = GetDataFromUnogs("t=genres");

            return ConvertUnogGenres(jsonData);
        }

        public MovieList Search(SearchCriteria searchCriteria)
        {
            var jsonData = GetDataFromUnogs($"q={searchCriteria.Query}-!{searchCriteria.StartYear}%2C{searchCriteria.EndYear}-!{searchCriteria.BottomNetflixRating}%2C{searchCriteria.UpperNetflixRating}-!{searchCriteria.BottomImdbRating}%2C{searchCriteria.UpperImdbRating}-!{searchCriteria.GenreId}-!Any-!Any-!Any-!gt100-!%7Bdownloadable%7D&t=ns&cl=78&st=adv&ob=Relevance");

            return ConvertUnogMovies(jsonData);
        }

        public MovieList GetRecommendedMoviesFromFacet()
        {
            Assert.IsNotNull(Tracker.Current, "Tracker.Current");
            Assert.IsNotNull(Tracker.Current.Session, "Tracker.Current.Session");
            using (XConnectClient client = Sitecore.XConnect.Client.Configuration.SitecoreXConnectClientConfiguration.GetClient())
            {
                var contactReference = new IdentifiedContactReference("xDB.Tracker",
                    Tracker.Current.Session.Contact.ContactId.ToString("N"));
                var contact = client.Get(contactReference,
                    new ContactExpandOptions(MovieRecommendationFacet.DefaultFacetKey));
                var facet = contact?.GetFacet<MovieRecommendationFacet>(MovieRecommendationFacet.DefaultFacetKey);

                if (facet == null) return new MovieList();

                var movieList = new MovieList()
                {
                    Count = 50,
                    Items = facet.MovieRecommendations.Skip(Math.Max(0,facet.MovieRecommendations.Count - 50)).ToList()
                };
                return movieList;
            }
        }
    }
}
