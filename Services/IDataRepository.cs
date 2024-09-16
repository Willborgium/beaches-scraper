using BeachesScraper.Models;

namespace BeachesScraper.Services
{
    public interface IDataRepository
    {
        Task AppendResultAsync(ScrapeResult scrapeResult, CancellationToken cancellationToken = default);
        Task<IEnumerable<ScrapeResult>> LoadResultsAsync(CancellationToken cancellationToken = default);
    }
}
