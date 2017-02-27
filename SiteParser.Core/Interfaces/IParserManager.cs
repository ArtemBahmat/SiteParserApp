namespace SiteParser.Core.Interfaces
{
    public interface IParserManager
    {
        IParser ParserInstance { get; }

        bool IsAlive { get; set; }

        void Execute(string url, int maxThreadsCount, int nestingLevel, bool parseExternal);
    }
}
