using NUnit.Framework;
using SiteParserCore.Helpers;
using SiteParserCore.Models;

namespace ParserTests.HelpersTests
{
    public class UrlValidatorTests
    {
        private const string BASE_URL = "http://www.deezer.com/";

        [TestCase("http://www.deezer.com/article/show/", BASE_URL, false, ExpectedResult = true)]
        [TestCase("http://www.google.com/article/show/", BASE_URL, false, ExpectedResult = false)]
        public bool IsValidUrl_Test(string urlName, string baseUrl, bool parseExternal)
        {
            UrlValidator validator = new UrlValidator(baseUrl, parseExternal);
            Url url = new Url {Name = urlName, IsExternal = false};
            return validator.IsValid(url);
        }
    }
}