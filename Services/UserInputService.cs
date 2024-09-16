using BeachesScraper.Models;

namespace BeachesScraper.Services
{
    public class UserInputService(IRenderingService renderingService) : IUserInputService
    {
        public ScrapeParameters? GetScrapeRequest()
        {
            var input = GetYesNoQuit("Use default criteria?");

            if (Yes(input))
            {
                return new ScrapeParameters
                {
                    SearchFrom = new DateTime(2025, 2, 17),
                    SearchTo = new DateTime(2025, 6, 1),
                    StayDuration = 7,
                    Adults = 3,
                    Children = 2
                };
            }
            else if (Quit(input))
            {
                return null;
            }

            var numKids = GetInt("Number of kids", 0, 10);
            var numAdults = GetInt("Number of adults", 0, 10);
            var duration = GetInt("Duration", 3, 14);
            var searchFrom = GetDateTime("Search start date", DateTime.Today, DateTime.Today.AddYears(1));
            var searchTo = GetDateTime("Search end date", searchFrom.AddDays(1), DateTime.Today.AddYears(1));

            var request = new ScrapeParameters
            {
                Adults = numAdults,
                Children = numKids,
                StayDuration = duration,
                SearchFrom = searchFrom,
                SearchTo = searchTo
            };

            var renderingService = new RenderingService();

            renderingService.Print(renderingService.GetScrapeRequestLabel(request));

            input = GetYesNoQuit("Continue with search");

            if (Yes(input))
            {
                return request;
            }

            return null;
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

        private static readonly string[] YesNoQuitValues = { "y", "Y", "n", "N", "q", "Q" };

        private string GetYesNoQuit(string prompt)
        {
            renderingService.Print($"{prompt} (y/n/q)");

            var value = Console.ReadLine();

            while (!YesNoQuitValues.Contains(value))
            {
                value = Console.ReadLine();
            }

            return value;
        }

        private static bool Yes(string value) => value == "Y" || value == "y";
        private static bool No(string value) => value == "N" || value == "n";
        private static bool Quit(string value) => value == "Q" || value == "q";
    }
}
