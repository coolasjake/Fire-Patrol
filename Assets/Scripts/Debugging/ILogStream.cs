namespace FirePatrol
{
    public interface ILogStream
    {
        void RecordLog(LogLevel logLevel, string message);
    }
}
