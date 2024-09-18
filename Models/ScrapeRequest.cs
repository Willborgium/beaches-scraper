using Newtonsoft.Json;

namespace BeachesScraper.Models
{
    public record ScrapeParameters
    {
        [JsonConverter(typeof(StandardDateConverter))]
        public DateTime SearchFrom;
        [JsonConverter(typeof(StandardDateConverter))]
        public DateTime SearchTo;
        public int StayDuration;
        public int Adults;
        public int Children;
        public string ResortCode;
    }
}
