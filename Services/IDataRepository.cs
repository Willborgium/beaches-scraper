using BeachesScraper.Models;

namespace BeachesScraper
{
    public interface IDataRepository
    {
        Task SaveResultsAsync(IEnumerable<DailyScrapeResult> dailyScrapeResults, CancellationToken cancellationToken = default);
        Task<IEnumerable<DailyScrapeResult>> LoadResultsAsync(CancellationToken cancellationToken = default);
    }
}
