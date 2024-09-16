using BeachesScraper.Services;

namespace BeachesScraper
{
    internal class Program
    {
        static async Task Main()
        {
            var dataRepository = new DataRepository();
            var renderingService = new RenderingService();
            var userInputService = new UserInputService(renderingService);
            var beachesApiClient = new BeachesApiClient(renderingService);
            var scrapingService = new ScrapingService(beachesApiClient, renderingService);

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
