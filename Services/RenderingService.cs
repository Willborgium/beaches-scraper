using BeachesScraper.Models;
using BeachesScraper.Contracts;

namespace BeachesScraper.Services
{
    public class RenderingService : IRenderingService
    {
        public void Print(string message)
        {
            Console.WriteLine(message);
        }

        public string GetScrapeRequestLabel(ScrapeParameters request)
        {
            return $"{request.StayDuration}n {request.Adults}a {request.Children}k between {request.SearchFrom:MM-dd} and {request.SearchTo:MM-dd}";
        }

        public int RoundedBestPrice(RoomAvailabilityResponse? response)
        {
            if (response == null)
            {
                return int.MaxValue;
            }

            var value = (response?.TotalPriceForEntireLengthOfStay).Value;

            var remainder = value % 250;

            return value - remainder;
        }

        public string FormatResultDate(RoomAvailabilityResponse response)
        {
            var ci = response?.Date;
            var co = ci?.AddDays(response?.Length ?? 0);

            return $"{ci:MM-dd} - {co:MM-dd}";
        }

        public string FormatResult(RoomAvailabilityResponse response)
        {
            var p = response?.TotalPriceForEntireLengthOfStay ?? 0;

            return $"[{FormatResultDate(response)}] {p:C0}";
        }

        public string FormatScrapeGroupLabel(IGrouping<ScrapeParameters, ScrapeResult> scrapeGroup)
        {
            var key = scrapeGroup.Key;
            var mostRecentRun = scrapeGroup.OrderByDescending(r => r.Date).First().Date;

            return $"{GetScrapeRequestLabel(key)} [last run {mostRecentRun:G}]";
        }

        public void Wait()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }

        public async Task PrintResultAsync(ScrapeResult? result, CancellationToken cancellationToken = default)
        {
            if (result == null)
            {
                Console.WriteLine("No results found!");
                return;
            }

            var groups = result.Possibilities.GroupBy(r => RoundedBestPrice(r.BestRoom));

            var count = Math.Min(groups.Count(), 10);

            foreach (var group in groups.OrderBy(g => g.Key).Take(count))
            {
                Console.WriteLine($"{group.Key:C0}");

                var maxPrice = group.Key * 1.1;

                foreach (var item in group)
                {
                    var date = FormatResultDate(item.BestRoom);
                    var qualifiedRooms = item.Rooms.Where(r => RoundedBestPrice(r) < maxPrice);
                    var numRooms = qualifiedRooms.Count();
                    var maxQualifiedPrice = qualifiedRooms.Max(r => r.TotalPriceForEntireLengthOfStay);

                    Console.WriteLine($"\t{date} ({numRooms} rooms under {maxQualifiedPrice:C0})");
                }
            }
        }
    }
}
