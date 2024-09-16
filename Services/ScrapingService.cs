using BeachesScraper.Contracts;
using BeachesScraper.Models;

namespace BeachesScraper.Services
{
    public class ScrapingService(
        IBeachesApiClient beachesApiClient,
        IRenderingService renderingService
        ) : IScrapingService
    {
        public async Task<ScrapeResult> RunScrapeAsync(ScrapeParameters scrapeParameters, CancellationToken cancellationToken = default)
        {
            var checkIn = scrapeParameters.SearchFrom;

            var errorCount = 0;

            var results = new List<ResortStayPossibility>();
            RoomAvailabilityResponse? bestResult = null;

            var nextApiCallTime = DateTime.MinValue;

            while (checkIn < scrapeParameters.SearchTo)
            {
                var checkOut = checkIn.AddDays(scrapeParameters.StayDuration);

                var request = new RoomAvailabilityRequest
                {
                    Brand = "B",
                    ResortCode = "BTC",
                    Adults = scrapeParameters.Adults,
                    Children = scrapeParameters.Children,
                    CheckIn = checkIn,
                    CheckOut = checkOut
                };

                renderingService.Print($"Checking {checkIn:MM-dd} - {checkOut:MM-dd}");

                while (DateTime.Now < nextApiCallTime)
                {
                    Thread.Sleep(ApiWaitInMilliseconds);
                }

                var response = await beachesApiClient.GetAvailability(request, cancellationToken);

                nextApiCallTime = DateTime.Now.AddMilliseconds(ApiDelayInMilliseconds);

                if (response == null)
                {
                    errorCount++;

                    if (errorCount == MaxApiErrorCount)
                    {
                        renderingService.Print($"Exiting after {MaxApiErrorCount} API errors");

                        return new ScrapeResult
                        {
                            Date = DateTime.Now,
                            Parameters = scrapeParameters,
                            Possibilities = results,
                            BestRoom = bestResult,
                            ErrorCount = errorCount,
                            DidErrorOut = true
                        };
                    }

                    continue;
                }

                var availableResults = response
                    .Where(r => r.Available == true)
                    .OrderBy(r => r.TotalPriceForEntireLengthOfStay)
                    .ToList();
                var count = availableResults.Count;

                RoomAvailabilityResponse? nextBest = null;

                if (count == 0)
                {
                    renderingService.Print("\tNo rooms found!");
                }
                else
                {
                    var max = availableResults.Max(r => r.TotalPriceForEntireLengthOfStay);
                    var min = availableResults.Min(r => r.TotalPriceForEntireLengthOfStay);

                    renderingService.Print($"\tFound {count} rooms from {min:C0} to {max:C0}");

                    nextBest = availableResults.FirstOrDefault();

                    if (bestResult == null)
                    {
                        bestResult = nextBest;

                    }
                    else if (nextBest != null && nextBest.TotalPriceForEntireLengthOfStay < bestResult.TotalPriceForEntireLengthOfStay)
                    {
                        var price = nextBest.TotalPriceForEntireLengthOfStay;

                        renderingService.Print($"\t$$$ new best rate $$$");

                        bestResult = nextBest;
                    }

                    renderingService.Print($"\t{renderingService.FormatResult(bestResult)}");
                }

                results.Add(new ResortStayPossibility
                {
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    Rooms = availableResults.Take(5),
                    BestRoom = nextBest
                });

                checkIn = checkIn.AddDays(1);
            }

            var topCount = (int)(results.Count * .1);

            var topResults = results
                .OrderBy(r => r.BestRoom?.TotalPrice ?? int.MaxValue)
                .Take(topCount)
                .Select(r => r.BestRoom);

            renderingService.Print($"Top {topCount} results:");

            foreach (var result in topResults)
            {
                renderingService.Print($"\t{renderingService.FormatResult(result)}");
            }

            return new ScrapeResult
            {
                Date = DateTime.Now,
                Parameters = scrapeParameters,
                Possibilities = results.OrderBy(r => r.BestRoom?.TotalPrice ?? int.MaxValue).ToList(),
                BestRoom = bestResult,
                ErrorCount = errorCount,
                DidErrorOut = false,
                BestRooms = topResults ?? []
            };
        }

        private const int ApiWaitInMilliseconds = 100;
        private const int ApiDelayInMilliseconds = 500;
        private const int MaxApiErrorCount = 5;
    }
}
