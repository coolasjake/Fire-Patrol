using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace FirePatrol
{
    public class SimpleFileLog : ILogStream
    {
        private readonly string _filePath;
        private StreamWriter _streamWriter;
        private readonly Stopwatch _timer;

        public SimpleFileLog(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            _filePath = filePath;

            _streamWriter = new StreamWriter(_filePath, append: false) { AutoFlush = true };

            _timer = Stopwatch.StartNew();
        }

        private string FormatTime()
        {
            return _timer.Elapsed.TotalSeconds.ToString("F2").PadLeft(6, ' ');
        }

        String ConvertToTraceLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Error:
                    return "ERROR";

                case LogLevel.Warning:
                    return "WARN";

                case LogLevel.Info:
                    return "INFO";

                case LogLevel.Debug:
                    return "DEBUG";

                case LogLevel.Trace:
                    return "TRACE";

                default:
                    return "ERROR";
            }
        }

        public static long GetUnixTimestamp()
        {
            var now = DateTime.UtcNow;
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(now - epoch).TotalSeconds;
        }

        public void RecordLog(LogLevel logLevel, string message)
        {
            var entry = new LogEntry()
            {
                level = ConvertToTraceLevel(logLevel),
                message = message,
                timestamp = GetUnixTimestamp(),
                time = (long)(Time.time * 10e9),
            };

            _streamWriter.WriteLine(JsonUtility.ToJson(entry));
        }

        [Serializable]
        class LogEntry
        {
            public String level;
            public String message;
            public long timestamp;
            public long time;
        }
    }
}
