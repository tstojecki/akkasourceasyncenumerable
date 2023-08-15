using Akka.Streams;

namespace AkkaSourceAsyncEnumerable.App;

public class Actor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    public Actor(Service service)
    {
        ReceiveAsync<RunService>(async msg =>
        {
            _log.Debug("{msg}", msg);

            var path = Self.Path.ToString();
            using var ctx = service.WithLoggingScope(new(new[] { new KeyValuePair<string, object>("ActorPath", path) }));
            await foreach (var item in service.AsyncEnumerable(msg.Limit))
            {
                _ = item;
            }

            Func<IDisposable?>? factory = () => service.Log.BeginScope(new List<KeyValuePair<string, object>>(new[] { new KeyValuePair<string, object>("ActorPath", path) }));
            var source = Source.From(() => service.AsyncEnumerable(msg.Limit, factory));

            // or
            // var source = Source.From(() => service.AsyncEnumerable(msg.Limit));

            // or 
            //var source = Source.From(() =>
            //{
            //    var _ = ctx;
            //    return service.AsyncEnumerable(msg.Limit, () => service.WithLoggingScope(new(new[] { new KeyValuePair<string, object>("ActorPath", path) })));
            //});

            // or             
            //var source = Source.From(() => service.AsyncEnumerable(msg.Limit, logger: service.Log));

            await source.RunWith(Sink.Ignore<int>(), Context.Materializer());

            Sender.Tell(new RunService.Complete());
        });
    }

    public record RunService(int Limit)
    {
        public record Complete();
    }
}