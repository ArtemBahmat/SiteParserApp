using System.Threading;

namespace SiteParserService.Interfaces
{
    public interface IScheduler
    {
        void Schedule(Timer scheduleTimer);
    }
}