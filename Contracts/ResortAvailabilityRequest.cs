using Newtonsoft.Json;

namespace BeachesScraper.Contracts
{
    public class RoomAvailabilityRequest
    {
        public string Brand { get; set; }
        public string ResortCode { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        [JsonConverter(typeof(StandardDateConverter))]
        public DateTime CheckIn { get; set; }
        [JsonConverter(typeof(StandardDateConverter))]
        public DateTime CheckOut { get; set; }
    }
}
