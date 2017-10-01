using Zeta.Common;

namespace EXPCount
{
    internal static class Logger
    {
        private static readonly log4net.ILog DbLog = Zeta.Common.Logger.GetLoggerInstanceForType();

        private static void error(string message, params object[] args)
        {
            message = "[" + EXPCountPugin.NAME + " V" + EXPCountPugin.VERSION + "]" + message;
            DbLog.ErrorFormat(message, args);
        }

        private static void info(string message, params object[] args)
        {
            message = "[" + EXPCountPugin.NAME + " V" + EXPCountPugin.VERSION + "]" + message;
            DbLog.InfoFormat(message, args);
        }

        private static void debug(string message, params object[] args)
        {
            message = "[" + EXPCountPugin.NAME + " V" + EXPCountPugin.VERSION + "]" + message;
            DbLog.DebugFormat(message, args);
        }

        private static void verbase(string message)
        {
            message = "[" + EXPCountPugin.NAME + " V" + EXPCountPugin.VERSION + "]" + message;
            DbLog.Verbose(message);
        }

        private static void warn(string message)
        {
            message = "[" + EXPCountPugin.NAME + " V" + EXPCountPugin.VERSION + "]" + message;
            DbLog.WarnFormat(message);
        }

        internal static void system(string message)
        {
            warn("[Plugin information]" + message);
        }

        internal static void count(string message)
        {
            info("[Experience statistics]" + message);
        }
    }
}
