using System.Runtime.CompilerServices;

namespace BeachesScraper.Contracts
{
    public static class ResortCodes
    {
        public const string BeachesTurksAndCaicos = "BTC";
        public const string BeachesNegril = "BNG";
        public const string BeachesOchoRios = "BBO";

        public static IEnumerable<string> All => _all;

        private static readonly IEnumerable<string> _all = [BeachesTurksAndCaicos, BeachesNegril, BeachesOchoRios];
    }
}
