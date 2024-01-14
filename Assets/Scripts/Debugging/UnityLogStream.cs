using UnityEngine;

namespace FirePatrol
{
    public class UnityLogStream : ILogStream
    {
        private static UnityLogStream _instance;

        private UnityLogStream() { }

        public static UnityLogStream Instance
        {
            get
            {
                Assert.That(!UnityLogRedirector.IsStarted);

                if (_instance == null)
                {
                    _instance = new UnityLogStream();
                }

                return _instance;
            }
        }

        public void RecordLog(LogLevel logLevel, string message)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                case LogLevel.Trace:
                    Debug.Log(message);
                    break;

                case LogLevel.Error:
                    Debug.LogError(message);
                    break;

                case LogLevel.Warning:
                    Debug.LogWarning(message);
                    break;

                default:
                    Debug.LogError(message);
                    break;
            }
        }
    }
}
