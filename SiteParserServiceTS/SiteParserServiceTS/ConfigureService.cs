using SiteParserCore.BusinessLogic;
using SiteParserCore.Interfaces;
using StructureMap;
using Topshelf;

namespace SiteParserServiceTS
{
    internal static class ConfigureService
    {
        public static IContainer container = new Container(x => {
            x.For<IParserManager>().Use<ParserManager>();
            x.For<IRepository>().Use<Repository>();
        });

        internal static void Configure()
        {
            HostFactory.Run(configure =>
            {
                configure.Service<ParserService>(service =>
                {
                    service.ConstructUsing(s => new ParserService());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });

                configure.RunAsLocalSystem(); 
                configure.SetServiceName("SiteParserServiceTS");
                configure.SetDisplayName("SiteParserServiceTS");
                configure.SetDescription("SiteParser service (TopShelf)");
            });
        }
    }
}
