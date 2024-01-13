using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FirePatrol
{
    public static class UnityLogRedirector
    {
        private static bool _isStarted;

        public static bool IsStarted
        {
            get { return _isStarted; }
        }

        public static void Start()
        {
            if (_isStarted)
            {
                return;
            }

            _isStarted = true;
            Application.logMessageReceived += OnUnityLog;
        }

        public static void Stop()
        {
            if (!_isStarted)
            {
                Log.Warn(
                    "[UnityLogRedirector] Attempted to stop UnityLogRedirector when it is not started"
                );
                return;
            }

            Assert.That(!Log.Streams.OfType<UnityLogStream>().Any());

            _isStarted = false;
            Application.logMessageReceived -= OnUnityLog;
        }

        private static void OnUnityLog(
            string message,
            string stackTrace,
            LogType logType
        )
        {
            var customLogType = ConvertUnityLogType(logType);
            var fullMessage = string.Format("[Unity] {0}", message);

            if (customLogType == LogLevel.Error && stackTrace != null && stackTrace.Length > 0)
            {
                fullMessage += "\n" + stackTrace;
            }

            Log.RecordLog(customLogType, fullMessage);
        }

        private static LogLevel ConvertUnityLogType(LogType logType)
        {
            switch (logType)
            {
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:
                    return LogLevel.Error;

                case LogType.Log:
                    return LogLevel.Info;

                case LogType.Warning:
                    return LogLevel.Warning;
            }

            return LogLevel.Error;
        }
    }
}
