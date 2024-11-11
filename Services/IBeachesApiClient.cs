using BeachesScraper.Contracts.Beaches;

namespace BeachesScraper.Services
{
    public interface IBeachesApiClient
    {
        Task<IEnumerable<RoomAvailabilityResponse>?> GetAvailability(RoomAvailabilityRequest request, CancellationToken cancellationToken = default);
    }
}
