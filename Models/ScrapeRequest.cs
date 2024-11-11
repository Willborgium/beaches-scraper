using Newtonsoft.Json;

namespace BeachesScraper.Models
{
    public enum Resort
    {
        Beaches,
        GreatWolfLodge
    }

    public record ScrapeParameters
    {
        [JsonConverter(typeof(StandardDateConverter))]
        public DateTime SearchFrom;
        [JsonConverter(typeof(StandardDateConverter))]
        public DateTime SearchTo;
        public int StayDuration;
        public int Adults;
        public int Children;
        public IEnumerable<int> ChildrenAges;
        public Resort Resort;
        public string ResortCode;
    }
}
