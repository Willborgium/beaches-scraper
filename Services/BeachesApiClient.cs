using BeachesScraper.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Text;

namespace BeachesScraper.Services
{
    public class BeachesApiClient : IBeachesApiClient
    {
        public async Task<IEnumerable<ResortAvailabilityResponse>?> GetAvailability(ResortAvailabilityRequest request, CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient();

            var requestString = JsonConvert.SerializeObject(request, Settings);

            var content = new StringContent(requestString, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(BeachesAvailabilityUrl, content);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"[{response.StatusCode}]: {response.RequestMessage}");

                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<IEnumerable<ResortAvailabilityResponse>>(responseBody);
        }

        private static readonly JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private const string BeachesAvailabilityUrl = "https://www.beaches.com/api/route/resort/rate/price/availability/";
    }
}
