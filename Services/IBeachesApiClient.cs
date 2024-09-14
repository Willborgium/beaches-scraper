using BeachesScraper.Contracts;

namespace BeachesScraper
{
    public interface IBeachesApiClient
    {
        Task<IEnumerable<ResortAvailabilityResponse>?> GetAvailability(ResortAvailabilityRequest request, CancellationToken cancellationToken = default);
    }
}
