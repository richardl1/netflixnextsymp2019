using System.Web;

namespace NetflixSitecore.Models
{
    public class Movie
    {
        public string netflixid { get; set; }
        public string image { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public HtmlString synopsis { get; set; }
    }
}