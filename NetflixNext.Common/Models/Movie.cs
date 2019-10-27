using Newtonsoft.Json;

namespace NetflixNext.Common.Models
{
    public class Movie
    {
        public string netflixid { get; set; }
        public string title { get; set; }
        public string image { get; set; }
        public string synopsis { get; set; }
        public string rating { get; set; }
        public string type { get; set; }
        public string released { get; set; }
        public string runtime { get; set; }
        public string largeimage { get; set; }
        public string unogsdate { get; set; }
        public string imdbid { get; set; }
        public string download { get; set; }
    }
}
