using Newtonsoft.Json;

namespace BeachesScraper.Models
{
    public class ResortStayPossibility
    {
        [JsonConverter(typeof(StandardDateConverter))]
        public DateTime CheckIn { get; set; }
        [JsonConverter(typeof(StandardDateConverter))]
        public DateTime CheckOut { get; set; }
        public IEnumerable<Room> Rooms { get; set; }
        public Room? BestRoom { get; set; }
    }
}
