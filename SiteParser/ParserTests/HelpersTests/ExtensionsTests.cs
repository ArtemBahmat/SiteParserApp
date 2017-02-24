using HtmlAgilityPack;
using NUnit.Framework;
using SiteParserCore.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserTests.HelpersTests
{
    [TestFixture]
    public class ExtensionsTests
    {
        //  private const string EXPECTED_RESULT_SITE_NAME = "http://www.deezer.com/";
        [TestCase(ExpectedResult = 189)]
        public int GetHtmlSize_Test()
        {
            string path = Path.GetTempPath() + "testFile.html";
            int size = 0;

            if (!CreateHtmlFile(path)) return size;
            HtmlDocument doc = new HtmlDocument();
            doc.Load(path);
            size = doc.GetHtmlSize();

            return size;
        }

        private bool CreateHtmlFile(string path)
        {
            using (var sWriter = new StreamWriter(path))
            {
                sWriter.WriteLine("<html>");
                sWriter.WriteLine("<head><title>This is test html file</title></head>");
                sWriter.WriteLine("<body>");
                sWriter.WriteLine("Some interesting text <table><tr><td>cell number 1</td><td>cell number 2</td></tr> and some more text");
                sWriter.WriteLine("</body>");
                sWriter.WriteLine("</html>");
            }
            return File.Exists(path);
        }
    }
}
