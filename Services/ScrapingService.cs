using BeachesScraper.Contracts.Beaches;
using BeachesScraper.Contracts.GreatWolfLodge;
using BeachesScraper.Models;
using System.Diagnostics;

namespace BeachesScraper.Services
{

    public class ScrapingService(
        IResortApiAdapter<RoomAvailabilityRequest> beachesApiAdapter,
        IResortApiAdapter<AvailabilityRequest> greatWolfLodgeApiAdapter,
        IRenderingService renderingService
        ) : IScrapingService
    {
        public async Task<ScrapeResult> RunScrapeAsync(ScrapeParameters scrapeParameters, CancellationToken cancellationToken = default)
        {
            switch (scrapeParameters.Resort)
            {
                case Resort.Beaches:
                    return await RunScrapeAsync(beachesApiAdapter, scrapeParameters, cancellationToken);

                case Resort.GreatWolfLodge:
                    return await RunScrapeAsync(greatWolfLodgeApiAdapter, scrapeParameters, cancellationToken);
            }

            return null;
        }

        private async Task<ScrapeResult> RunScrapeAsync<TRequest>(IResortApiAdapter<TRequest> apiAdapter, ScrapeParameters scrapeParameters, CancellationToken cancellationToken = default)
        {
            var checkIn = scrapeParameters.SearchFrom;

            var errorCount = 0;

            var results = new List<ResortStayPossibility>();

            var nextApiCallTime = DateTime.MinValue;

            while (checkIn < scrapeParameters.SearchTo)
            {
                var checkOut = checkIn.AddDays(scrapeParameters.StayDuration);

                var request = apiAdapter.GetRequest(checkIn, checkOut, scrapeParameters);

                renderingService.Print($"Checking {checkIn:MM-dd} - {checkOut:MM-dd}");

                while (DateTime.Now < nextApiCallTime) { await Task.Yield(); }

                nextApiCallTime = DateTime.Now.AddMilliseconds(ApiDelayInMilliseconds);

                var stopwatch = Stopwatch.StartNew();

                var rooms = await apiAdapter.GetRoomsAsync(request, cancellationToken);

                Console.WriteLine($"\t\tTook {stopwatch.Elapsed.TotalSeconds:0.000} seconds");

                if (rooms == null)
                {
                    errorCount++;

                    if (errorCount == MaxApiErrorCount)
                    {
                        renderingService.Print($"Exiting after {MaxApiErrorCount} API errors");

                        return GetScrapeResult(results, scrapeParameters, errorCount);
                    }

                    continue;
                }

                var availableResults = rooms
                    .OrderBy(r => r.Cost)
                    .ToList();
                var count = availableResults.Count;

                if (count == 0)
                {
                    renderingService.Print("\tNo rooms found!");
                }
                else
                {
                    var max = availableResults.Max(r => r.Cost);
                    var min = availableResults.Min(r => r.Cost);

                    renderingService.Print($"\tFound {count} rooms from {min:C0} to {max:C0}");
                }

                results.Add(new ResortStayPossibility
                {
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    Rooms = availableResults,
                    BestRoom = availableResults.FirstOrDefault()
                });

                checkIn = checkIn.AddDays(1);
            }

            return GetScrapeResult(results, scrapeParameters, errorCount);
        }

        private ScrapeResult GetScrapeResult(IEnumerable<ResortStayPossibility> results, ScrapeParameters scrapeParameters, int errorCount)
        {
            var orderedResults = results.OrderBy(r => r.BestRoom?.Cost ?? int.MaxValue);

            var topCount = (int)(results.Count() * .1);
            var topRooms = orderedResults.Take(topCount).Select(r => r.BestRoom) ?? [];

            renderingService.Print($"Top {topCount} results:");

            foreach (var room in topRooms)
            {
                renderingService.Print($"\t{renderingService.FormatResult(room)}");
            }

            return new ScrapeResult
            {
                Date = DateTime.Now,
                Parameters = scrapeParameters,
                Possibilities = orderedResults,
                BestRoom = topRooms.FirstOrDefault(),
                ErrorCount = errorCount,
                DidErrorOut = errorCount == MaxApiErrorCount,
                BestRooms = topRooms
            };
        }

        private const int ApiDelayInMilliseconds = 100;
        private const int MaxApiErrorCount = 5;
    }
}
