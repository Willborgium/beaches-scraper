using BeachesScraper.Contracts;

namespace BeachesScraper.Services
{
    public interface IBeachesApiClient
    {
        Task<IEnumerable<ResortAvailabilityResponse>?> GetAvailability(ResortAvailabilityRequest request, CancellationToken cancellationToken = default);
    }
}
