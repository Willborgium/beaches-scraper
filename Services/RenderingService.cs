using BeachesScraper.Models;

namespace BeachesScraper.Services
{
    public class RenderingService : IRenderingService
    {
        public void Print(string message)
        {
            Console.WriteLine(message);
        }

        private const int ResortLength = 14;
        private const int ResortCodeLength = 6;

        public string GetScrapeRequestLabel(ScrapeParameters request)
        {
            return $"{request.Resort,-ResortLength}  {request.ResortCode,-ResortCodeLength}  {request.StayDuration,-2}n  {request.Adults,-2}a  {request.Children,-2}k  {request.SearchFrom:MM-dd} to {request.SearchTo:MM-dd}";
        }

        public int RoundedBestPrice(Room? response)
        {
            if (response == null)
            {
                return int.MaxValue;
            }

            var value = (response?.Cost).Value;

            var remainder = value % 250;

            return value - remainder;
        }

        public string FormatResultDate(Room response)
        {
            var ci = response?.CheckIn;
            var co = response?.CheckOut;

            return $"{ci:MM-dd} - {co:MM-dd}";
        }

        public string FormatResult(Room response)
        {
            var p = response?.Cost ?? 0;

            return $"[{FormatResultDate(response)}] {p:C0}";
        }

        public string FormatScrapeGroupLabel(IGrouping<ScrapeParameters, ScrapeResult> scrapeGroup)
        {
            var key = scrapeGroup.Key;
            var mostRecentRun = scrapeGroup.OrderByDescending(r => r.Date).First().Date;

            return $"{GetScrapeRequestLabel(key)} [last run {mostRecentRun:G}]";
        }

        public void Wait()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }



        private static int GetRoomCodeLength(ResortStayPossibility? possibility) => possibility.Rooms.MaxOr(0, GetRoomCodeLength);
        private static int GetRoomCodeLength(Room r) => r.RoomCode.Length;

        private static int GetDescriptionLength(ResortStayPossibility? possibility) => possibility.Rooms.MaxOr(0, GetDescriptionLength);
        private static int GetDescriptionLength(Room r) => r.Description.Length;

        public async Task PrintResultAsync(ScrapeResult? result, bool includeDetails, CancellationToken cancellationToken = default)
        {
            if (result == null)
            {
                Console.WriteLine("No results found!");
                return;
            }

            var groups = result.Possibilities.GroupBy(r => RoundedBestPrice(r.BestRoom));

            var count = Math.Min(groups.Count(), 10);

            var maxRcLength = groups.Max(g => g.Max(GetRoomCodeLength));
            var maxDescriptionLength = groups.Max(g => g.Max(GetDescriptionLength));

            foreach (var group in groups.OrderBy(g => g.Key).Take(count).OrderByDescending(g => g.Key))
            {
                Console.WriteLine($"{group.Key:C0}");

                var maxPrice = group.Key * 1.1;

                foreach (var item in group.OrderByDescending(g => g.BestRoom.Cost))
                {
                    var date = FormatResultDate(item.BestRoom);
                    var qualifiedRooms = item.Rooms.Where(r => RoundedBestPrice(r) < maxPrice);
                    var numRooms = qualifiedRooms.Sum(r => r.AvailableRooms);
                    var maxQualifiedPrice = qualifiedRooms.Max(r => r.Cost);

                    Console.WriteLine($"\t{date} ({numRooms} rooms under {maxQualifiedPrice:C0})");

                    if (includeDetails)
                    {
                        foreach (var detailGroup in item.Rooms.GroupBy(r => (r.RoomCode, r.Description)))
                        {
                            var rc = $"{detailGroup.Key.RoomCode}".PadRight(maxRcLength);
                            var d = detailGroup.Key.Description.PadRight(maxDescriptionLength);
                            var availableRooms = $"{detailGroup.Sum(r => r.AvailableRooms)}".PadRight(3);
                            var averageCost = detailGroup.Average(r => r.Cost);
                            Console.WriteLine($"\t\t[{rc}]\t{d}\t{availableRooms:00} rooms\t{averageCost:C0}");
                        }
                        
                        Console.WriteLine();
                    }
                }

                Console.WriteLine();
            }
        }
    }
}
