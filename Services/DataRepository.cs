using BeachesScraper.Models;
using Newtonsoft.Json;
using System.Text;

namespace BeachesScraper.Services
{
    public class DataRepository : IDataRepository
    {
        public async Task AppendResultAsync(ScrapeResult scrapeResult, CancellationToken cancellationToken = default)
        {
            var results = await LoadResultsInternalAsync(cancellationToken);

            results.Add(scrapeResult);

            using var writer = new StreamWriter(SaveResultsFilePath);

            var data = JsonConvert.SerializeObject(results.OrderByDescending(d => d.Date), Formatting.Indented);

            var builder = new StringBuilder(data);

            await writer.WriteAsync(builder, cancellationToken);
        }

        public async Task<IEnumerable<ScrapeResult>> LoadResultsAsync(CancellationToken cancellationToken = default) =>
            await LoadResultsInternalAsync(cancellationToken);

        private static async Task<List<ScrapeResult>> LoadResultsInternalAsync(CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(SaveResultsFilePath);

            var data = await reader.ReadToEndAsync(cancellationToken);

            return JsonConvert.DeserializeObject<List<ScrapeResult>>(data) ?? [];
        }

        private const string SaveResultsFilePath = "c:/source/beaches-scrapes.json";
    }
}
