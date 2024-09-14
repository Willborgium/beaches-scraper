using Newtonsoft.Json;

namespace BeachesScraper.Contracts
{
    public class ResortAvailabilityResponse
    {
        public int? AdultRate { get; set; }
        public int? ChildRate { get; set; }
        public string ResortCode { get; set; }
        public string RoomCategoryCode { get; set; }
        public string CountryCode { get; set; }
        public bool? Available { get; set; }
        [JsonConverter(typeof(StandardDateConverter))]
        public DateTime? Date { get; set; }
        public int? Length { get; set; }
        public int? TotalPrice { get; set; }
        public int? DaysOfBestPrice { get; set; }
        public int? Pppn { get; set; }
        public int? TotalPriceForEntireLengthOfStay { get; set; }
        public int? AvgPriceAdultsAndKids { get; set; }
        public int? AvailableRooms { get; set; }
        public int? UnavailableDays { get; set; }
    }
}
