using NUnit.Framework;
using SiteParser.Core.Helpers;

namespace ParserTests.HelpersTests
{
    [TestFixture]
    public class UrlHelperTests
    {
        //Assert.That(UrlHelper.GetCorrectUrl(url), Is.EqualTo("http://www.deezer.com/"));
        // TestFixtureSetup - отмечает метод, который запускается 1 раз перед всеми тестами в классе

        private const string EXPECTED_RESULT_SITE_NAME = "http://www.deezer.com/";

        private const string EXPECTED_RESULT_ABSOLUTE_URL = "http://www.deezer.com/en/article";

        [TestCase("/en/article", EXPECTED_RESULT_SITE_NAME, ExpectedResult = EXPECTED_RESULT_ABSOLUTE_URL)]
        [TestCase("en/article", EXPECTED_RESULT_SITE_NAME, ExpectedResult = EXPECTED_RESULT_ABSOLUTE_URL)]
        public string GetAbsoluteUrl_Test(string href, string baseUrl)
        {
            return UrlHelper.GetAbsoluteUrl(href, baseUrl);
        }

        [TestCase(EXPECTED_RESULT_SITE_NAME, ExpectedResult = "deezer.com")]
        public string GetDomainFromUrl_Test(string url)
        {
            return UrlHelper.GetDomainFromUrl(url);
        }

        [Test]
        [TestCase("www.deezer.com", ExpectedResult = EXPECTED_RESULT_SITE_NAME)]
        [TestCase("http://www.deezer.com", ExpectedResult = EXPECTED_RESULT_SITE_NAME)]
        [TestCase("www.deezer.com/", ExpectedResult = EXPECTED_RESULT_SITE_NAME)]
        public string GetCorrectUrl_Test(string url)
        {
            return UrlHelper.GetCorrectUrl(url);
        }
    }
}