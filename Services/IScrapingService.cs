using BeachesScraper.Models;

namespace BeachesScraper.Services
{
    public interface IScrapingService
    {
        Task<ScrapeResult> RunScrapeAsync(ScrapeParameters scrapeParameters, CancellationToken cancellationToken = default);
    }
}
