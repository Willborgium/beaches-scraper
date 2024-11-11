using Beaches = BeachesScraper.Contracts.Beaches;
using GWL = BeachesScraper.Contracts.GreatWolfLodge;
using BeachesScraper.Models;

namespace BeachesScraper.Services
{
    public class UserInputService(IRenderingService renderingService) : IUserInputService
    {
        private static readonly IReadOnlyDictionary<Resort, ScrapeParameters> DefaultParameters = new Dictionary<Resort, ScrapeParameters>()
        {
            {
                Resort.Beaches,
                new()
                {
                    SearchFrom = new DateTime(2025, 2, 17),
                    SearchTo = new DateTime(2025, 6, 1),
                    StayDuration = 7,
                    Adults = 3,
                    Children = 2,
                    Resort = Resort.Beaches,
                    ResortCode = Beaches.ResortCodes.BeachesTurksAndCaicos
                }
            },
            {
                Resort.GreatWolfLodge,
                new()
                {
                    SearchFrom = new DateTime(2025, 7, 1),
                    SearchTo = new DateTime(2025, 9, 1),
                    StayDuration = 3,
                    Adults = 2,
                    Children = 2,
                    ChildrenAges = [4, 7],
                    Resort = Resort.GreatWolfLodge,
                    ResortCode = GWL.ResortCodes.Mashantucket
                }
            },
        };

        private ScrapeParameters? GetBeachesScrapeRequest()
        {
            var numKids = GetInt("Number of kids", 0, 10);
            var numAdults = GetInt("Number of adults", 0, 10);
            var duration = GetInt("Duration", 3, 14);
            var searchFrom = GetDateTime("Search start date", DateTime.Today, DateTime.Today.AddYears(1));
            var searchTo = GetDateTime("Search end date", searchFrom.AddDays(1), DateTime.Today.AddYears(1));
            var resortCode = GetOption(Beaches.ResortCodes.All);

            var request = new ScrapeParameters
            {
                Adults = numAdults,
                Children = numKids,
                StayDuration = duration,
                SearchFrom = searchFrom,
                SearchTo = searchTo,
                Resort = Resort.Beaches,
                ResortCode = resortCode
            };

            var renderingService = new RenderingService();

            renderingService.Print(renderingService.GetScrapeRequestLabel(request));

            var doContinue = GetYesNoQuit("Continue with search");

            if (Yes(doContinue))
            {
                return request;
            }

            return null;
        }

        private ScrapeParameters? GetGreatWolfLodgeScrapeRequest()
        {
            var numKids = GetInt("Number of kids", 0, 5);
            var kidsAges = new List<int>();

            while (kidsAges.Count < numKids)
            {
                var age = GetInt($"Kid {kidsAges.Count + 1} age", 1, 18);
                kidsAges.Add(age);
            }
            
            var numAdults = GetInt("Number of adults", 0, 10);

            var duration = GetInt("Duration", 1, 14);
            var searchFrom = GetDateTime("Search start date", DateTime.Today, DateTime.Today.AddYears(1));
            var searchTo = GetDateTime("Search end date", searchFrom.AddDays(1), DateTime.Today.AddYears(1));
            var resortCode = GetOption(GWL.ResortCodes.All);

            var request = new ScrapeParameters
            {
                Adults = numAdults,
                Children = numKids,
                ChildrenAges = kidsAges,
                StayDuration = duration,
                SearchFrom = searchFrom,
                SearchTo = searchTo,
                Resort = Resort.GreatWolfLodge,
                ResortCode = resortCode
            };

            var renderingService = new RenderingService();

            renderingService.Print(renderingService.GetScrapeRequestLabel(request));

            var doContinue = GetYesNoQuit("Continue with search");

            if (Yes(doContinue))
            {
                return request;
            }

            return null;
        }

        public ScrapeParameters? GetScrapeRequest()
        {
            var resort = GetOption(All<Resort>());

            var useDefault = GetYesNoQuit("Use default criteria?");

            if (Yes(useDefault))
            {
                return DefaultParameters[resort];
            }
            else if (Quit(useDefault))
            {
                return null;
            }

            return resort switch
            {
                Resort.Beaches => GetBeachesScrapeRequest(),
                Resort.GreatWolfLodge => GetGreatWolfLodgeScrapeRequest(),
                _ => null,
            };
        }

        public static IEnumerable<T> All<T>()
            where T : struct, Enum
        {
            return Enum.GetValues<T>();
        }

        public async Task<ScrapeResult> GetResultAsync(IEnumerable<ScrapeResult> scrapeResults, CancellationToken cancellationToken = default)
        {
            var renderingService = new RenderingService();

            var requestGroups = scrapeResults.GroupBy(r => r.Parameters).ToList();

            return GetOption(requestGroups, renderingService.FormatScrapeGroupLabel)
                .OrderByDescending(r => r.Date)
                .First();
        }

        public T GetOption<T>(IEnumerable<T> options, Func<T, string>? formatter = null)
        {
            var index = 1;

            foreach (var option in options)
            {
                var label = formatter?.Invoke(option) ?? $"{option}";

                renderingService.Print($"{index++:00}: {label}");
            }

            var selectedIndex = GetInt("Select an option", 1, options.Count()) - 1;

            return options.ElementAt(selectedIndex);
        }

        private int GetInt(string prompt, int min, int max)
        {
            var input = "";
            var value = -1;

            renderingService.Print(prompt);

            while (!int.TryParse(input, out value) || value < min || value > max)
            {
                input = Console.ReadLine();
            }

            return value;
        }

        private DateTime GetDateTime(string prompt, DateTime min, DateTime max)
        {
            var input = "";
            var value = DateTime.MinValue;

            renderingService.Print(prompt);

            while (!DateTime.TryParse(input, out value) || value < min || value > max)
            {
                input = Console.ReadLine();
            }

            return value;
        }

        private static readonly string[] YesNoQuitValues = ["y", "Y", "n", "N", "q", "Q"];

        public string GetYesNoQuit(string prompt)
        {
            renderingService.Print($"{prompt} (y/n/q)");

            var value = Console.ReadLine();

            while (!YesNoQuitValues.Contains(value))
            {
                value = Console.ReadLine();
            }

            return value;
        }

        public bool Yes(string value) => value == "Y" || value == "y";
        public bool No(string value) => value == "N" || value == "n";
        public bool Quit(string value) => value == "Q" || value == "q";
    }
}
