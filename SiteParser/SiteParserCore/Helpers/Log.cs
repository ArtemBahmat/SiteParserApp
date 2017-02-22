using NLog;
using System;

namespace SiteParserCore.Helpers
{
    public static class Log
    {
        static ILogger logger = LogManager.GetCurrentClassLogger();

        public static void Info(string message)
        {
            logger.Info(message);
        }

        public static void Error(string message)
        {
            logger.Error(message);
        }

        public static void Error(Exception ex)
        {
            logger.Error(ex);
        }
    }
}
