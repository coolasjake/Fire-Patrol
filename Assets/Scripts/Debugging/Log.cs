using System.Collections.Generic;
using System.Diagnostics;

namespace FirePatrol
{
    public static class Log
    {
        private static readonly List<ILogStream> _streams = new List<ILogStream>();
        private static LogLevel _maxLogLevel = LogLevel.Debug;

        public static IReadOnlyList<ILogStream> Streams
        {
            get { return _streams; }
        }

        public static LogLevel MaxLogLevel
        {
            get => _maxLogLevel;
            set
            {
                if (_maxLogLevel != value)
                {
                    var oldValue = _maxLogLevel;
                    _maxLogLevel = value;
                    RecordLog(
                        LogLevel.Debug,
                        "[Log] Changed max log level from '{0}' to '{1}'",
                        oldValue,
                        _maxLogLevel
                    );
                }
            }
        }

        public static bool IsTraceEnabled()
        {
            return IsLevelEnabled(LogLevel.Trace);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Trace(string message)
        {
            RecordLog(LogLevel.Trace, message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Trace(string message, object arg1)
        {
            RecordLog(LogLevel.Trace, message, arg1);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Trace(string message, object arg1, object arg2)
        {
            RecordLog(LogLevel.Trace, message, arg1, arg2);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Trace(string message, object arg1, object arg2, object arg3)
        {
            RecordLog(LogLevel.Trace, message, arg1, arg2, arg3);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Trace(string message, object arg1, object arg2, object arg3, object arg4)
        {
            RecordLog(LogLevel.Trace, message, arg1, arg2, arg3, arg4);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Trace(
            string message,
            object arg1,
            object arg2,
            object arg3,
            object arg4,
            object arg5
        )
        {
            RecordLog(LogLevel.Trace, message, arg1, arg2, arg3, arg4, arg5);
        }

        public static bool IsDebugEnabled()
        {
            return IsLevelEnabled(LogLevel.Debug);
        }

        public static void Debug(string message)
        {
            RecordLog(LogLevel.Debug, message);
        }

        public static void Debug(string message, object arg1)
        {
            RecordLog(LogLevel.Debug, message, arg1);
        }

        public static void Debug(string message, object arg1, object arg2)
        {
            RecordLog(LogLevel.Debug, message, arg1, arg2);
        }

        public static void Debug(string message, object arg1, object arg2, object arg3)
        {
            RecordLog(LogLevel.Debug, message, arg1, arg2, arg3);
        }

        public static void Debug(string message, object arg1, object arg2, object arg3, object arg4)
        {
            RecordLog(LogLevel.Debug, message, arg1, arg2, arg3, arg4);
        }

        public static void Debug(
            string message,
            object arg1,
            object arg2,
            object arg3,
            object arg4,
            object arg5
        )
        {
            RecordLog(LogLevel.Debug, message, arg1, arg2, arg3, arg4, arg5);
        }

        public static bool IsInfoEnabled()
        {
            return IsLevelEnabled(LogLevel.Info);
        }

        public static void Info(string message)
        {
            RecordLog(LogLevel.Info, message);
        }

        public static void Info(string message, object arg1)
        {
            RecordLog(LogLevel.Info, message, arg1);
        }

        public static void Info(string message, object arg1, object arg2)
        {
            RecordLog(LogLevel.Info, message, arg1, arg2);
        }

        public static void Info(string message, object arg1, object arg2, object arg3)
        {
            RecordLog(LogLevel.Info, message, arg1, arg2, arg3);
        }

        public static void Info(string message, object arg1, object arg2, object arg3, object arg4)
        {
            RecordLog(LogLevel.Info, message, arg1, arg2, arg3, arg4);
        }

        public static void Info(
            string message,
            object arg1,
            object arg2,
            object arg3,
            object arg4,
            object arg5
        )
        {
            RecordLog(LogLevel.Info, message, arg1, arg2, arg3, arg4, arg5);
        }

        public static bool IsWarnEnabled()
        {
            return IsLevelEnabled(LogLevel.Warning);
        }

        public static void Warn(string message)
        {
            RecordLog(LogLevel.Warning, message);
        }

        public static void Warn(string message, object arg1)
        {
            RecordLog(LogLevel.Warning, message, arg1);
        }

        public static void Warn(string message, object arg1, object arg2)
        {
            RecordLog(LogLevel.Warning, message, arg1, arg2);
        }

        public static void Warn(string message, object arg1, object arg2, object arg3)
        {
            RecordLog(LogLevel.Warning, message, arg1, arg2, arg3);
        }

        public static void Warn(string message, object arg1, object arg2, object arg3, object arg4)
        {
            RecordLog(LogLevel.Warning, message, arg1, arg2, arg3, arg4);
        }

        public static void Warn(
            string message,
            object arg1,
            object arg2,
            object arg3,
            object arg4,
            object arg5
        )
        {
            RecordLog(LogLevel.Warning, message, arg1, arg2, arg3, arg4, arg5);
        }

        public static bool IsErrorEnabled()
        {
            return IsLevelEnabled(LogLevel.Error);
        }

        public static void Error(string message)
        {
            RecordLog(LogLevel.Error, message);
        }

        public static void Error(string message, object arg1)
        {
            RecordLog(LogLevel.Error, message, arg1);
        }

        public static void Error(string message, object arg1, object arg2)
        {
            RecordLog(LogLevel.Error, message, arg1, arg2);
        }

        public static void Error(string message, object arg1, object arg2, object arg3)
        {
            RecordLog(LogLevel.Error, message, arg1, arg2, arg3);
        }

        public static void Error(string message, object arg1, object arg2, object arg3, object arg4)
        {
            RecordLog(LogLevel.Error, message, arg1, arg2, arg3, arg4);
        }

        public static void Error(
            string message,
            object arg1,
            object arg2,
            object arg3,
            object arg4,
            object arg5
        )
        {
            RecordLog(LogLevel.Error, message, arg1, arg2, arg3, arg4, arg5);
        }

        public static void AddStream(ILogStream logger)
        {
            Assert.That(!_streams.Contains(logger));
            RecordLog(LogLevel.Debug, "[Log] Registered stream '{0}'", logger.GetType());
            _streams.Add(logger);
        }

        public static void RemoveStream(ILogStream logger)
        {
            _streams.Remove(logger);
            RecordLog(LogLevel.Debug, "[Log] Unregistered stream '{0}'", logger.GetType());
        }

        public static void ClearLogStreams()
        {
            _streams.Clear();
        }

        public static void RecordLog(LogLevel logLevel, string message)
        {
            if (IsLevelEnabled(logLevel))
            {
                RecordLogToStreams(logLevel, message);
            }
        }

        public static void RecordLog(LogLevel logLevel, string message, object arg1)
        {
            if (IsLevelEnabled(logLevel))
            {
                RecordLogToStreams(logLevel, string.Format(message, arg1));
            }
        }

        public static void RecordLog(LogLevel logLevel, string message, object arg1, object arg2)
        {
            if (IsLevelEnabled(logLevel))
            {
                RecordLogToStreams(logLevel, string.Format(message, arg1, arg2));
            }
        }

        public static void RecordLog(
            LogLevel logLevel,
            string message,
            object arg1,
            object arg2,
            object arg3
        )
        {
            if (IsLevelEnabled(logLevel))
            {
                RecordLogToStreams(logLevel, string.Format(message, arg1, arg2, arg3));
            }
        }

        public static void RecordLog(
            LogLevel logLevel,
            string message,
            object arg1,
            object arg2,
            object arg3,
            object arg4
        )
        {
            if (IsLevelEnabled(logLevel))
            {
                RecordLogToStreams(logLevel, string.Format(message, arg1, arg2, arg3, arg4));
            }
        }

        public static void RecordLog(
            LogLevel logLevel,
            string message,
            object arg1,
            object arg2,
            object arg3,
            object arg4,
            object arg5
        )
        {
            if (IsLevelEnabled(logLevel))
            {
                RecordLogToStreams(logLevel, string.Format(message, arg1, arg2, arg3, arg4, arg5));
            }
        }

        private static void RecordLogToStreams(LogLevel logLevel, string message)
        {
            foreach (var logger in _streams)
            {
                logger.RecordLog(logLevel, message);
            }
        }

        public static bool IsLevelEnabled(LogLevel logLevel)
        {
            if (_streams.Count == 0)
            {
                return false;
            }

            return (int)logLevel <= (int)MaxLogLevel;
        }
    }
}
