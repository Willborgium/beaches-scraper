using BeachesScraper.Models;
using BeachesScraper.Contracts;

namespace BeachesScraper.Services
{
    public interface IRenderingService
    {
        string GetScrapeRequestLabel(ScrapeRequest request);
        int RoundedBestPrice(ResortAvailabilityResponse? response);
        string FormatResultDate(ResortAvailabilityResponse response);
        string FormatResult(ResortAvailabilityResponse response);
        Task PrintResultAsync(DailyScrapeResult? result, CancellationToken cancellationToken = default);
    }
}
