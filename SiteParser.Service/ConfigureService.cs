using SiteParser.Core.BusinessLogic;
using SiteParser.Core.Interfaces;
using SiteParser.Core.Repository;
using SiteParser.DAL.Interfaces;
using SiteParser.DAL.Models;
using SiteParser.DAL.Repositories;
using SiteParserService.Interfaces;
using StructureMap;
using Topshelf;

namespace SiteParserService
{
    internal static class ConfigureService
    {
        public static readonly IContainer Container = new Container(x =>
        {
            x.For<IParser>().Use<Parser>();
            x.For<IParserManager>().Use<ParserManager>();
            x.For<IRepository<Site>>().Use<Repository<Site>>();
            x.For<IRepository<Url>>().Use<UrlsRepository>();
            x.For<IRepository<Resource>>().Use<ResourcesRepository>();
            x.For<IWalker>().Use<SitesWalker>();
            x.For<IScheduler>().Use<Scheduler>();
        });

        //public static readonly IContainer Container = new Container(x =>
        //{
        //    x.Scan(s =>
        //    {
        //        s.WithDefaultConventions();
        //        s.AssembliesFromApplicationBaseDirectory();
        //    });
        //});

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
                configure.SetServiceName("SiteParserService");
                configure.SetDisplayName("SiteParserService");
                configure.SetDescription("SiteParser service (TopShelf)");
            });
        }
    }
}
