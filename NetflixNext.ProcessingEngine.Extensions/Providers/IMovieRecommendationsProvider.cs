using NetflixNext.Common.Models;
using System.Collections.Generic;

namespace NetflixNext.ProcessingEngine.Extensions.Providers
{
    public interface IMovieRecommendationsProvider
    {
        List<Movie> GetRecommendations(IEnumerable<string> movieids);
    }
}
