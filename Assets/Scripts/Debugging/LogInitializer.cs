using System.IO;
using UnityEngine;

namespace FirePatrol
{
    public static class LogInitializer
    {
        static bool _hasInitialized = false;

        public static void LazyInitialize()
        {
            if (_hasInitialized)
            {
                return;
            }

            _hasInitialized = true;

            var settings = GameSettings.Instance;

            if (settings == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (System.Environment.GetEnvironmentVariable("COMPUTERNAME") == "STEVE-HOME")
            {
                Log.AddStream(
                    new SimpleFileLog(Path.Combine(Application.dataPath, "../Svkj.json"))
                );
                UnityLogRedirector.Start();
            }
            else
            {
                Log.AddStream(UnityLogStream.Instance);
            }
#else
            Log.AddStream(UnityLogStream.Instance);
#endif
            Log.MaxLogLevel = settings.MaxLogLevel;

            Log.Info("[LogInitializer] Initialized log with max level = {0}", settings.MaxLogLevel);
        }
    }
}
