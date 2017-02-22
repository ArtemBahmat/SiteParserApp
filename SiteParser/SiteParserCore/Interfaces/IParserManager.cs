namespace SiteParserCore.Interfaces
{
    public interface IParserManager
    {
        IParser ParserInstance { get; set; }
        bool IsAlive { get; set; }

        void Execute(string url, int maxThreadsCount, int nestingLevel, bool parseExternal);
    }
}
