using SiteParser.Utils;
using SiteParserService.Interfaces;
using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;

namespace SiteParserService
{
    public class Scheduler : IScheduler
    {
        public void Schedule(Timer scheduleTimer)
        {
            try
            {
                string mode = "Interval";

                try
                {
                    mode = ConfigurationManager.AppSettings["Mode"].ToString();
                    Log.Info($"Service Mode: {mode}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }

                DateTime scheduledTime = GetScheduledTime(mode);
                TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
                string schedule = string.Format($"{timeSpan.Days} day(s) {timeSpan.Hours} hour(s) {timeSpan.Minutes} minute(s) {timeSpan.Seconds} seconds(s)");
                Log.Info($"Service scheduled to run after: {schedule}");
                int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);
                scheduleTimer.Change(dueTime, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Log.Error($"Service Error on: {ex.Message}");

                using (ServiceController serviceController = new ServiceController("SiteParserService"))
                {
                    serviceController.Stop();
                }
            }
        }

        private DateTime GetScheduledTime(string mode)
        {
            DateTime scheduledTime = DateTime.MinValue;

            if (mode.ToUpper() == "DAILY")
            {
                scheduledTime = DateTime.Parse(ConfigurationManager.AppSettings["ScheduledTime"]);

                if (DateTime.Now > scheduledTime)
                {
                    scheduledTime = scheduledTime.AddDays(1);
                }
            }

            if (mode.ToUpper() == "INTERVAL")
            {
                int intervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalMinutes"]);
                scheduledTime = DateTime.Now.AddMinutes(intervalMinutes);

                if (DateTime.Now > scheduledTime)
                {
                    scheduledTime = scheduledTime.AddMinutes(intervalMinutes);
                }
            }

            return scheduledTime;
        }
    }
}
