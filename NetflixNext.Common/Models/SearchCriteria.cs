namespace NetflixNext.Common.Models
{
    public class SearchCriteria
    {
        public string Query { get; set; }
        public string StartYear { get; set; } = "1900";
        public string EndYear { get; set; } = "2019";
        public string BottomNetflixRating { get; set; } = "0";
        public string UpperNetflixRating { get; set; } = "5";
        public string BottomImdbRating { get; set; } = "0";
        public string UpperImdbRating { get; set; } = "10";
        public string GenreId { get; set; } = "0";

    }
}
