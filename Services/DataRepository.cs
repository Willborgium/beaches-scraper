using BeachesScraper.Models;
using Newtonsoft.Json;
using System.Text;

namespace BeachesScraper.Services
{
    public class DataRepository : IDataRepository
    {
        public async Task SaveResultsAsync(IEnumerable<DailyScrapeResult> dailyScrapeResults, CancellationToken cancellationToken = default)
        {
            using var writer = new StreamWriter(SaveResultsFilePath);

            var data = JsonConvert.SerializeObject(dailyScrapeResults, Formatting.Indented);

            var builder = new StringBuilder(data);

            await writer.WriteAsync(builder, cancellationToken);
        }

        public async Task<IEnumerable<DailyScrapeResult>> LoadResultsAsync(CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(SaveResultsFilePath);

            var data = await reader.ReadToEndAsync(cancellationToken);

            return JsonConvert.DeserializeObject<IEnumerable<DailyScrapeResult>>(data) ?? [];
        }

        private const string SaveResultsFilePath = "c:/source/beaches-scrapes.json";
    }
}
