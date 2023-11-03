using BepInEx.Logging;

namespace TunicStrategyTester
{
    internal class Logger
    {
        private static ManualLogSource Log = null;

        public static void SetLogger(ManualLogSource log)
        {
            Log = log;
        }

        public static void LogDebug(object data)
        {
            Log?.LogDebug(data);
        }

        public static void LogInfo(object data)
        {
            Log?.LogInfo(data);
        }

        public static void LogWarning(object data)
        {
            Log?.LogWarning(data);
        }

        public static void LogError(object data)
        {
            Log?.LogError(data);
        }

        public static void LogFatal(object data)
        {
            Log?.LogFatal(data);
        }
    }
}
