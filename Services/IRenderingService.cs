using BeachesScraper.Models;

namespace BeachesScraper.Services
{
    public interface IRenderingService
    {
        string GetScrapeRequestLabel(ScrapeParameters request);
        int RoundedBestPrice(Room? response);
        string FormatResultDate(Room response);
        string FormatResult(Room response);
        Task PrintResultAsync(ScrapeResult? result, bool includeDetails, CancellationToken cancellationToken = default);
        string FormatScrapeGroupLabel(IGrouping<ScrapeParameters, ScrapeResult> scrapeGroup);
        void Wait();
        void Print(string message);
    }
}
