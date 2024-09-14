namespace BeachesScraper.Models
{
    public class DailyScrapeResult
    {
        public DateTime Date { get; set; }
        public ScrapeRequest Request { get; set; }
        public IEnumerable<ResortAvailabilityResults> Results { get; set; }
        public ResortAvailabilityResponse? BestResponse { get; set; }
        public int ErrorCount { get; set; }
        public bool DidErrorOut { get; set; }
        public IEnumerable<ResortAvailabilityResponse?> BestResults { get; set; }
    }
}
