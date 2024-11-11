namespace BeachesScraper.Models
{
    public class ScrapeResult
    {
        public DateTime Date { get; set; }
        public ScrapeParameters Parameters { get; set; }
        public IEnumerable<ResortStayPossibility> Possibilities { get; set; }
        public Room? BestRoom { get; set; }
        public int ErrorCount { get; set; }
        public bool DidErrorOut { get; set; }
        public IEnumerable<Room?> BestRooms { get; set; }
    }
}
