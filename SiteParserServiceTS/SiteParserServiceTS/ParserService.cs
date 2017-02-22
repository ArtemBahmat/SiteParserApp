using SiteParserCore.Helpers;
using System.Threading;

namespace SiteParserServiceTS
{
    public class ParserService
    {
        SitesWalker _walker = new SitesWalker();
        Scheduler _scheduler = new Scheduler();
        Thread walkerThread;
        private Timer _scheduleTimer;

        public void Start()
        {
            Log.Info("Service SiteParserServiceTS started");
            _scheduleTimer = new Timer(new TimerCallback(SchedularCallback));
            walkerThread = new Thread(new ThreadStart(DoWork));   
            walkerThread.Start();
        }
        public void Stop()
        {
            _walker.Stop();
        }

        private void DoWork()
        {
            _walker.Start();
            _scheduler.ScheduleService(ref _scheduleTimer);
        }

        private void SchedularCallback(object e)
        {
            DoWork();
        }

    }
}
