using BeachesScraper.Models;

namespace BeachesScraper.Services
{
    public interface IUserInputService
    {
        ScrapeParameters? GetScrapeRequest();
        Task<ScrapeResult> GetResultAsync(IEnumerable<ScrapeResult> scrapeResults, CancellationToken cancellationToken = default);
        T GetOption<T>(IEnumerable<T> options, Func<T, string>? formatter = null);
        string GetYesNoQuit(string prompt);
        bool Yes(string value);
        bool No(string value);
        bool Quit(string value);
    }
}