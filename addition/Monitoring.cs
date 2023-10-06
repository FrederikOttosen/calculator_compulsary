using System.Diagnostics;
using System.Reflection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace addition;

public static class Monitoring
{
    public static readonly ActivitySource ActivitySource = new("RPS_" + Assembly.GetEntryAssembly()?.GetName().Name, "1.0.0");

    public static void Initialize()
    {
        Console.WriteLine("Initializing OpenTelemetry Tracing.");
        Sdk.CreateTracerProviderBuilder()
            .AddZipkinExporter(o =>
            {
                o.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
                Console.WriteLine($"Setting up Zipkin exporter with endpoint: {o.Endpoint}");

            })
            .AddConsoleExporter()
            .AddSource(ActivitySource.Name)
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: ActivitySource.Name))
            .Build();
    }
}