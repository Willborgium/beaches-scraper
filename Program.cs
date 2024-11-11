using BeachesScraper.Services;

namespace BeachesScraper
{
    public static class Extensions
    {
        public static int MaxOr<T>(this IEnumerable<T> collection, int max, Func<T, int> selector)
        {
            if (collection == null || !collection.Any())
            {
                return max;
            }

            return collection.Max(selector);
        }
    }

    internal class Program
    {
        static async Task Main()
        {
            var dataRepository = new DataRepository();
            var renderingService = new RenderingService();
            var userInputService = new UserInputService(renderingService);
            var beachesApiAdapter = new BeachesApiAdapter(renderingService);
            var greatWolfLodgeApiAdapter = new GreatWolfLodgeApiAdapter(renderingService);
            var scrapingService = new ScrapingService(beachesApiAdapter, greatWolfLodgeApiAdapter, renderingService);

            var scraperProgram = new ScraperProgram(
                userInputService,
                renderingService,
                dataRepository,
                scrapingService
                );

            await scraperProgram
                .RunAsync();
        }
    }
}
