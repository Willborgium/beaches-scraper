using Newtonsoft.Json.Converters;

namespace BeachesScraper
{
    public class StandardDateConverter : IsoDateTimeConverter
    {
        public StandardDateConverter() => DateTimeFormat = "yyyy-MM-dd";
    }
}
