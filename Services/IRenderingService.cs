using BeachesScraper.Models;
using BeachesScraper.Contracts;

namespace BeachesScraper.Services
{
    public interface IRenderingService
    {
        string GetScrapeRequestLabel(ScrapeParameters request);
        int RoundedBestPrice(RoomAvailabilityResponse? response);
        string FormatResultDate(RoomAvailabilityResponse response);
        string FormatResult(RoomAvailabilityResponse response);
        Task PrintResultAsync(ScrapeResult? result, CancellationToken cancellationToken = default);
        string FormatScrapeGroupLabel(IGrouping<ScrapeParameters, ScrapeResult> scrapeGroup);
        void Wait();
        void Print(string message);
    }
}
