using BeachesScraper.Models;
using BeachesScraper.Contracts;

namespace BeachesScraper
{
    internal class Program
    {
        private const int ApiWaitInMilliseconds = 100;
        private const int ApiDelayInMilliseconds = 500;
        private const int MaxApiErrorCount = 5;

        static async Task Main(string[] args)
        {
            var input = "";

            var validInputs = new string[] { "p", "P", "q", "Q", "r", "R" };

            while (input != "Q" && input != "q")
            {
                input = "";
                Console.WriteLine("(P)rint");
                Console.WriteLine("(R)erun");
                Console.WriteLine("(Q)uit");

                while (!validInputs.Contains(input))
                {
                    input = Console.ReadLine();
                }

                switch (input)
                {
                    case "p":
                    case "P":
                        await PerformPrintAsync();
                        break;

                    case "r":
                    case "R":
                        await PerformScrapeAsync();
                        break;
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private static string GetScrapeRequestLabel(ScrapeRequest request)
        {
            return $"{request.StayDuration}n {request.Adults}a {request.Children}k between {request.SearchFrom:MM-dd} and {request.SearchTo:MM-dd}";
        }

        private static async Task<DailyScrapeResult> GetResultFromUser(CancellationToken cancellationToken = default)
        {
            var dataRepository = new DataRepository();

            var results = await dataRepository.LoadResultsAsync(cancellationToken);

            var requestGroups = results.GroupBy(r => r.Request).ToList();

            Console.WriteLine("Which results do you want to see?");

            var index = 0;

            foreach (var requestGroup in requestGroups)
            {
                var key = requestGroup.Key;
                var mostRecentRun = requestGroup.OrderByDescending(r => r.Date).First().Date;

                Console.WriteLine($"{index + 1:00}: {GetScrapeRequestLabel(key)} [last run {mostRecentRun:G}]");

                index++;
            }

            string input = "";
            int inputIndex = -1;

            while (!int.TryParse(input, out inputIndex) || inputIndex < 1 || inputIndex > requestGroups.Count)
            {
                input = Console.ReadLine();
            }

            inputIndex = int.Parse(input) - 1;

            return results.ElementAt(inputIndex);
        }

        private static async Task PerformPrintAsync(CancellationToken cancellationToken = default)
        {
            var result = await GetResultFromUser(cancellationToken);

            if (result == null)
            {
                Console.WriteLine("No results found!");
                return;
            }

            var groups = result.Results.GroupBy(r => RoundedBestPrice(r.BestResult));

            var count = Math.Min(groups.Count(), 10);

            foreach (var group in groups.OrderBy(g => g.Key).Take(count))
            {
                Console.WriteLine($"{group.Key:C0}");

                var maxPrice = group.Key * 1.1;

                foreach (var item in group)
                {
                    var date = FormatResultDate(item.BestResult);
                    var numRooms = item.Results.Count(r => RoundedBestPrice(r) < maxPrice);

                    Console.WriteLine($"\t{date} ({numRooms} rooms under {maxPrice:C0})");
                }
            }
        }

        private static int RoundedBestPrice(ResortAvailabilityResponse? response)
        {
            if (response == null)
            {
                return int.MaxValue;
            }

            var value = (response?.TotalPriceForEntireLengthOfStay).Value;

            var remainder = value % 250;

            return value - remainder;
        }

        private static ScrapeRequest? GetScrapeRequest()
        {
            Console.WriteLine("Use default criteria? y/n");

            var input = "";

            var allowedInputs = new string[] { "y", "Y", "n", "N", "q", "Q" };

            while (!allowedInputs.Contains(input))
            {
                input = Console.ReadLine();
            }

            if (input == "y" || input == "Y")
            {
                return new ScrapeRequest
                {
                    SearchFrom = new DateTime(2025, 2, 17),
                    SearchTo = new DateTime(2025, 6, 1),
                    StayDuration = 7,
                    Adults = 3,
                    Children = 2
                };
            }

            if (input == "q" || input == "Q")
            {
                return null;
            }

            input = "";
            var numKids = -1;
            Console.WriteLine("Number of kids");
            while (!int.TryParse(input, out numKids) || numKids < 0 || numKids > 10)
            {
                input = Console.ReadLine();
                numKids = -1;
            }

            input = "";
            var numAdults = -1;
            Console.WriteLine("Number of adults");
            while (!int.TryParse(input, out numAdults) || numAdults < 0 || numAdults > 10)
            {
                input = Console.ReadLine();
                numAdults = -1;
            }

            input = "";
            var duration = -1;
            Console.WriteLine("Duration");
            while (!int.TryParse(input, out duration) || duration < 3 || duration > 14)
            {
                input = Console.ReadLine();
                duration = -1;
            }

            var minSearchFromDate = DateTime.Today;
            var maxDate = DateTime.Today.AddYears(1);

            input = "";
            var searchFrom = minSearchFromDate.AddDays(-1);
            Console.WriteLine("Search start date");
            while (!DateTime.TryParse(input, out searchFrom) || searchFrom < minSearchFromDate || searchFrom > maxDate)
            {
                input = Console.ReadLine();
                searchFrom = minSearchFromDate.AddDays(-1);
            }

            input = "";
            var minSearchToDate = minSearchFromDate.AddDays(1);
            var searchTo = minSearchToDate.AddDays(-1);
            Console.WriteLine("Search end date");
            while (!DateTime.TryParse(input, out searchTo) || searchTo < minSearchToDate || searchTo > maxDate)
            {
                input = Console.ReadLine();
                searchTo = minSearchToDate.AddDays(-1);
            }

            var request = new ScrapeRequest
            {
                Adults = numAdults,
                Children = numKids,
                StayDuration = duration,
                SearchFrom = searchFrom,
                SearchTo = searchTo
            };

            Console.WriteLine($"Continue with search (y/n)?\n\t{GetScrapeRequestLabel(request)}");

            input = "";
            while (!allowedInputs.Contains(input))
            {
                input = Console.ReadLine();
            }

            if (input == "Y" || input == "y")
            {
                return request;
            }

            return null;
        }

        private static async Task PerformScrapeAsync(CancellationToken cancellationToken = default)
        {
            var dataRepository = new DataRepository();

            var scrapeRequest = GetScrapeRequest();

            if (scrapeRequest == null)
            {
                Console.WriteLine("Aborting scrape");
                return;
            }

            Console.WriteLine("Beginning scrape");

            var data = await GetDailyScrapeResult(scrapeRequest);

            var savedData = await dataRepository.LoadResultsAsync(cancellationToken);

            var updatedData = new List<DailyScrapeResult>(savedData ?? [])
            {
                data
            };

            await dataRepository.SaveResultsAsync(updatedData.OrderByDescending(d => d.Date), cancellationToken);

            Console.WriteLine("Scrape complete");
        }

        private static async Task<DailyScrapeResult> GetDailyScrapeResult(ScrapeRequest scrapeRequest)
        {
            var client = new BeachesApiClient();

            var checkIn = scrapeRequest.SearchFrom;

            var errorCount = 0;

            var results = new List<ResortAvailabilityResults>();
            ResortAvailabilityResponse? bestResult = null;

            var nextApiCallTime = DateTime.MinValue;

            while (checkIn < scrapeRequest.SearchTo)
            {
                var checkOut = checkIn.AddDays(scrapeRequest.StayDuration);

                var request = new ResortAvailabilityRequest
                {
                    Brand = "B",
                    ResortCode = "BTC",
                    Adults = scrapeRequest.Adults,
                    Children = scrapeRequest.Children,
                    CheckIn = checkIn,
                    CheckOut = checkOut
                };

                Console.WriteLine($"Checking {checkIn:MM-dd} - {checkOut:MM-dd}");

                while (DateTime.Now < nextApiCallTime)
                {
                    Thread.Sleep(ApiWaitInMilliseconds);
                }

                var response = await client.GetAvailability(request);

                nextApiCallTime = DateTime.Now.AddMilliseconds(ApiDelayInMilliseconds);

                if (response == null)
                {
                    errorCount++;

                    if (errorCount == MaxApiErrorCount)
                    {
                        Console.WriteLine($"Exiting after {MaxApiErrorCount} API errors");

                        return new DailyScrapeResult
                        {
                            Date = DateTime.Now,
                            Request = scrapeRequest,
                            Results = results,
                            BestResponse = bestResult,
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

                ResortAvailabilityResponse? nextBest = null;

                if (count == 0)
                {
                    Console.WriteLine("\tNo rooms found!");
                }
                else
                {
                    var max = availableResults.Max(r => r.TotalPriceForEntireLengthOfStay);
                    var min = availableResults.Min(r => r.TotalPriceForEntireLengthOfStay);

                    Console.WriteLine($"\tFound {count} rooms from {min:C0} to {max:C0}");

                    nextBest = availableResults.FirstOrDefault();

                    if (bestResult == null)
                    {
                        bestResult = nextBest;

                    }
                    else if (nextBest != null && nextBest.TotalPriceForEntireLengthOfStay < bestResult.TotalPriceForEntireLengthOfStay)
                    {
                        var price = nextBest.TotalPriceForEntireLengthOfStay;

                        Console.WriteLine($"\t$$$ new best rate $$$");

                        bestResult = nextBest;
                    }

                    Console.WriteLine($"\t{FormatResult(bestResult)}");
                }

                results.Add(new ResortAvailabilityResults
                {
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    Results = availableResults.Take(5),
                    BestResult = nextBest
                });

                checkIn = checkIn.AddDays(1);
            }

            var topCount = (int)(results.Count * .1);

            var topResults = results
                .OrderBy(r => r.BestResult?.TotalPrice ?? int.MaxValue)
                .Take(topCount)
                .Select(r => r.BestResult);

            Console.WriteLine($"Top {topCount} results:");

            foreach (var result in topResults)
            {
                Console.WriteLine($"\t{FormatResult(result)}");
            }

            return new DailyScrapeResult
            {
                Date = DateTime.Now,
                Request = scrapeRequest,
                Results = results.OrderBy(r => r.BestResult?.TotalPrice ?? int.MaxValue).ToList(),
                BestResponse = bestResult,
                ErrorCount = errorCount,
                DidErrorOut = false,
                BestResults = topResults ?? []
            };
        }

        private static string FormatResultDate(ResortAvailabilityResponse response)
        {
            var ci = response?.Date;
            var co = ci?.AddDays(response?.Length ?? 0);

            return $"{ci:MM-dd} - {co:MM-dd}";
        }

        private static string FormatResult(ResortAvailabilityResponse response)
        {
            var p = response?.TotalPriceForEntireLengthOfStay ?? 0;

            return $"[{FormatResultDate(response)}] {p:C0}";
        }
    }
}
