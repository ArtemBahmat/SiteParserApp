using SiteParser.Core.BusinessLogic;
using SiteParser.Core.Interfaces;
using SiteParser.DAL.Interfaces;
using SiteParser.DAL.Models;
using SiteParser.DAL.Repositories;
using SiteParser.Core.Repository;
using StructureMap;
using System.Windows;

namespace SiteParser
{
    public partial class App : Application
    {
        public static readonly IContainer Container = new Container(x =>
        {
            x.For<IParser>().Use<Parser>();
            x.For<IParserManager>().Use<ParserManager>();
            x.For<IRepository<Site>>().Use<Repository<Site>>();
            x.For<IRepository<Url>>().Use<UrlsRepository>();
            x.For<IRepository<Resource>>().Use<ResourcesRepository>();
            x.For<IRepository<ImportUrl>>().Use<ImportUrlsRepository>();
        });

        //public static readonly IContainer Container = new Container(x =>
        //{
        //    x.Scan(s =>
        //    {
        //        s.WithDefaultConventions();
        //        s.AssembliesFromApplicationBaseDirectory();
        //    });
        //});
    }
}
