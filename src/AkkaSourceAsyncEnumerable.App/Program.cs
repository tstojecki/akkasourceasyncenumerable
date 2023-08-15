using Akka.Hosting;
using Akka.Logger.Serilog;
using AkkaSourceAsyncEnumerable.App;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{ActorPath}] {Message:lj}{NewLine}{Exception}")
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .CreateLogger();

Log.Logger = logger;

var hostBuilder = new HostBuilder();
hostBuilder.UseSerilog();

hostBuilder.ConfigureServices((context, services) =>
{
    services.AddAkka("MyActorSystem", (builder, sp) =>
    {
        builder
            .WithActors((system, registry, resolver) =>
            {
                var serviceWithLogger = resolver.GetService<Service>();
                var helloActor = system.ActorOf(Props.Create(() => new Actor(serviceWithLogger)), "serviceConsumer");
                registry.Register<Actor>(helloActor);
            })
            .ConfigureLoggers(configBuilder =>
            {
                configBuilder.LogLevel = LogLevel.DebugLevel;

                configBuilder.ClearLoggers();

                configBuilder.LogMessageFormatter = typeof(SerilogLogMessageFormatter);
                configBuilder.LogConfigOnStart = false;

                configBuilder.AddLogger<SerilogLogger>();
            });
    });

    services.AddTransient<Service>();
});

var host = hostBuilder.Start();

var system = host.Services.GetRequiredService<ActorSystem>();
var actor = host.Services.GetRequiredService<IRequiredActor<Actor>>().ActorRef;

await actor.Ask(new Actor.RunService(3));

await host.WaitForShutdownAsync();