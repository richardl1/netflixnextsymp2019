using System.Collections.Generic;
using Newtonsoft.Json;

namespace NetflixNext.Common.Models
{
    public class MovieDetails
    {
            public Result RESULT { get; set; }
     
    }
    public class Result
    {
        public Nfinfo nfinfo { get; set; }
        public Imdbinfo imdbinfo { get; set; }
        public string[] mgname { get; set; }
        public string[] Genreid { get; set; }
        public People[] people { get; set; }
        public object[] country { get; set; }
    }

    public class Nfinfo
    {
        public string image1 { get; set; }
        public string title { get; set; }
        public string synopsis { get; set; }
        public string matlevel { get; set; }
        public string matlabel { get; set; }
        public string avgrating { get; set; }
        public string type { get; set; }
        public string updated { get; set; }
        public string unogsdate { get; set; }
        public string released { get; set; }
        public string netflixid { get; set; }
        public string runtime { get; set; }
        public string image2 { get; set; }
        public string download { get; set; }
    }

    public class Imdbinfo
    {
        public string rating { get; set; }
        public string votes { get; set; }
        public string metascore { get; set; }
        public string genre { get; set; }
        public string awards { get; set; }
        public string runtime { get; set; }
        public string plot { get; set; }
        public string country { get; set; }
        public string language { get; set; }
        public string imdbid { get; set; }
    }

    public class People
    {
        public string[] actor { get; set; }
        public string[] creator { get; set; }
        public string[] director { get; set; }
    }
}
