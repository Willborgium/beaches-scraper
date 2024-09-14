using Newtonsoft.Json;

namespace BeachesScraper.Models
{
    public class ResortAvailabilityResults
    {
        [JsonConverter(typeof(StandardDateConverter))]
        public DateTime CheckIn { get; set; }
        [JsonConverter(typeof(StandardDateConverter))]
        public DateTime CheckOut { get; set; }
        public IEnumerable<ResortAvailabilityResponse> Results { get; set; }
        public ResortAvailabilityResponse? BestResult { get; set; }
    }
}
