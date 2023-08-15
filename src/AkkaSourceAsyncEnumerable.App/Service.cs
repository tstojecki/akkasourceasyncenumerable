using Microsoft.Extensions.Logging;

public class Service
{    
    public Service(ILoggerFactory loggerFactory)
    {        
        Log = loggerFactory.CreateLogger(GetType());
    }

    public ILogger Log { get; }

    public IDisposable? WithLoggingScope(List<KeyValuePair<string, object>> state)
    {
        return Log.BeginScope(state);
    }

    public async IAsyncEnumerable<int> AsyncEnumerable(int limit, Func<IDisposable?>? loggingScopeFactory = null, ILogger? logger = null)
    {
        var log = logger ?? Log;

        using var ctx = loggingScopeFactory?.Invoke();
        log.LogDebug("In {method}", nameof(AsyncEnumerable));

        for (int i = 0; i < limit; i++)
        {
            log.LogDebug("{i}", i);

            await Task.Delay(1000);
            yield return i;
        }
    }
}