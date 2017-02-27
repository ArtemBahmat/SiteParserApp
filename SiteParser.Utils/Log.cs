using NLog;
using System;

namespace SiteParser.Utils
{
    public static class Log
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public static void Info(string message)
        {
            Logger.Info(message);
        }

        public static void Error(string message)
        {
            Logger.Error(message);
        }

        public static void Error(Exception ex)
        {
            Logger.Error(ex);
        }
    }
}
