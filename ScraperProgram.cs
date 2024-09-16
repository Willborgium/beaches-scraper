using BeachesScraper.Services;

namespace BeachesScraper
{
    public class ScraperProgram(
        IUserInputService userInputService,
        IRenderingService renderingService,
        IDataRepository dataRepository,
        IScrapingService scrapingService
            )
    {
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var option = "";

            while (option != "Quit")
            {
                option = userInputService.GetOption(MenuOptions);

                if (option == "Print")
                {
                    await PerformPrintAsync(cancellationToken);
                }
                else if (option == "Rerun")
                {
                    await PerformScrapeAsync(cancellationToken);
                }

                renderingService.Wait();
            }
        }

        private async Task PerformPrintAsync(CancellationToken cancellationToken = default)
        {
            var scrapeResults = await dataRepository.LoadResultsAsync(cancellationToken);
            var scrapeResult = await userInputService.GetResultAsync(scrapeResults, cancellationToken);

            await renderingService.PrintResultAsync(scrapeResult, cancellationToken);
        }

        private async Task PerformScrapeAsync(CancellationToken cancellationToken = default)
        {
            var scrapeRequest = userInputService.GetScrapeRequest();

            if (scrapeRequest == null)
            {
                renderingService.Print("Aborting scrape");
                return;
            }

            renderingService.Print("Beginning scrape");

            var result = await scrapingService.RunScrapeAsync(scrapeRequest, cancellationToken);

            await dataRepository.AppendResultAsync(result, cancellationToken);

            renderingService.Print("Scrape complete");
        }

        private const string Print = "Print";
        private const string Rerun = "Rerun";
        private const string Quit = "Quit";
        private static readonly string[] MenuOptions = { Print, Rerun, Quit };
    }
}
