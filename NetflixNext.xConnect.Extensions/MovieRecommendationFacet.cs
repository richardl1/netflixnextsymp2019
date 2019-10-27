using NetflixNext.Common.Models;
using Sitecore.XConnect;
using System;
using System.Collections.Generic;

namespace NetflixNext.xConnect.Extensions
{
    /// <summary>
    /// A <see cref="Facet"/> which stores movie recommendations for the contact.
    /// </summary>
    [Serializable]
    [FacetKey(DefaultFacetKey)]
    public class MovieRecommendationFacet : Facet
    {
        public MovieRecommendationFacet()
        {
            MovieRecommendations = new List<Movie>();
        }

        public const string DefaultFacetKey = "MovieRecommendationFacet";
        public List<Movie> MovieRecommendations { get; set; }
    }
}
