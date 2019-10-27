using System.Collections.Generic;
using Newtonsoft.Json;

namespace NetflixNext.Common.Models
{
    public class MovieList
    {
        [JsonProperty("COUNT")]
        public int Count { get; set; }
        [JsonProperty("ITEMS")]
        public List<Movie> Items { get; set; }
    }
}
