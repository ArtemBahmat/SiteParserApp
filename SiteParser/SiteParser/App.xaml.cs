using SiteParserCore.BusinessLogic;
using SiteParserCore.Interfaces;
using StructureMap;
using System.Windows;

namespace SiteParser
{
    public partial class App : Application
    {
        public static IContainer container = new Container(x => {
            x.For<IParserManager>().Use<ParserManager>();
            x.For<IRepository>().Use<Repository>();
        });
    }
}
