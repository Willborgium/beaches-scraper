using BeachesScraper.Contracts;
using Newtonsoft.Json;

namespace BeachesScraper.Models
{
    public class ResortStayPossibility
    {
        [JsonConverter(typeof(StandardDateConverter))]
        public DateTime CheckIn { get; set; }
        [JsonConverter(typeof(StandardDateConverter))]
        public DateTime CheckOut { get; set; }
        public IEnumerable<RoomAvailabilityResponse> Rooms { get; set; }
        public RoomAvailabilityResponse? BestRoom { get; set; }
    }
}
