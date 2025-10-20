using System.Collections.Concurrent;

namespace RoomEnglish.Web.Services;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _path;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

    public FileLoggerProvider(string path)
    {
        _path = path;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _path));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

public class FileLogger : ILogger
{
    private readonly string _name;
    private readonly string _path;
    private static readonly object _lock = new();

    public FileLogger(string name, string path)
    {
        _name = name;
        _path = path;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var logFile = Path.Combine(_path, $"RoomEnglish-{DateTime.Now:yyyy-MM-dd}.log");
        var logRecord = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] {_name}: {formatter(state, exception)}";
        
        if (exception != null)
            logRecord += Environment.NewLine + exception;

        lock (_lock)
        {
            File.AppendAllText(logFile, logRecord + Environment.NewLine);
        }
    }
}