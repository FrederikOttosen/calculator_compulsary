using System.Diagnostics;
using System.Reflection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace addition;

public static class Monitoring
{
    public static readonly ActivitySource ActivitySource = new("RPS_" + Assembly.GetEntryAssembly()?.GetName().Name, "1.0.0");
    private static TracerProvider _tracerProvider;
    
    static Monitoring()
    {
        // Configure tracing
        var serviceName = Assembly.GetExecutingAssembly().GetName().Name;
        var version = "1.0.0";

        _tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddZipkinExporter(config =>
            {
                config.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
                Console.WriteLine($"Setting up Zipkin exporter with endpoint: {config.Endpoint}");

            })
            .AddConsoleExporter()
            .AddSource(ActivitySource.Name)
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: version))
            .Build();
    }
}