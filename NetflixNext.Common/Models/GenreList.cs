using System.Collections.Generic;
using Newtonsoft.Json;

namespace NetflixNext.Common.Models
{
    public class GenreList
    {
        [JsonProperty("COUNT")]
        public int Count { get; set; }
        [JsonProperty("ITEMS")]
        public List<Genre> Items { get; set; }
    }


}
