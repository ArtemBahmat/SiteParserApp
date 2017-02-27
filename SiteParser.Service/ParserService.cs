using SiteParser.Utils;
using SiteParserService.Interfaces;
using System.Threading;

namespace SiteParserService
{
    public class ParserService
    {
        private readonly IWalker _walker = ConfigureService.Container.GetInstance<IWalker>(); 
        private readonly IScheduler _scheduler = ConfigureService.Container.GetInstance<IScheduler>(); 
        private Thread _walkerThread;
        private Timer _scheduleTimer;

        public void Start()
        {
            Log.Info("Service SiteParserService started");
            _scheduleTimer = new Timer(SchedularCallback);
            _walkerThread = new Thread(DoWork);   
            _walkerThread.Start();
        }
        public void Stop()
        {
            _walker.Stop();
        }

        private void DoWork()
        {
            _walker.Start();
            _scheduler.Schedule(_scheduleTimer);
        }

        private void SchedularCallback(object e)
        {
            DoWork();
        }
    }
}
