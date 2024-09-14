using BeachesScraper.Models;
using BeachesScraper.Contracts;

namespace BeachesScraper.Services
{
    public class RenderingService : IRenderingService
    {
        public string GetScrapeRequestLabel(ScrapeRequest request)
        {
            return $"{request.StayDuration}n {request.Adults}a {request.Children}k between {request.SearchFrom:MM-dd} and {request.SearchTo:MM-dd}";
        }

        public int RoundedBestPrice(ResortAvailabilityResponse? response)
        {
            if (response == null)
            {
                return int.MaxValue;
            }

            var value = (response?.TotalPriceForEntireLengthOfStay).Value;

            var remainder = value % 250;

            return value - remainder;
        }


        public string FormatResultDate(ResortAvailabilityResponse response)
        {
            var ci = response?.Date;
            var co = ci?.AddDays(response?.Length ?? 0);

            return $"{ci:MM-dd} - {co:MM-dd}";
        }

        public string FormatResult(ResortAvailabilityResponse response)
        {
            var p = response?.TotalPriceForEntireLengthOfStay ?? 0;

            return $"[{FormatResultDate(response)}] {p:C0}";
        }

        public async Task PrintResultAsync(DailyScrapeResult? result, CancellationToken cancellationToken = default)
        {
            if (result == null)
            {
                Console.WriteLine("No results found!");
                return;
            }

            var groups = result.Results.GroupBy(r => RoundedBestPrice(r.BestResult));

            var count = Math.Min(groups.Count(), 10);

            foreach (var group in groups.OrderBy(g => g.Key).Take(count))
            {
                Console.WriteLine($"{group.Key:C0}");

                var maxPrice = group.Key * 1.1;

                foreach (var item in group)
                {
                    var date = FormatResultDate(item.BestResult);
                    var numRooms = item.Results.Count(r => RoundedBestPrice(r) < maxPrice);

                    Console.WriteLine($"\t{date} ({numRooms} rooms under {maxPrice:C0})");
                }
            }
        }
    }
}
