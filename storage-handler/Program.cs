using System.Data;
using MySqlConnector;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "StorageHandler_Tracer";
const string serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry().WithTracing(tcb =>
{
    tcb
        .AddSource(serviceName)
        .AddZipkinExporter(c =>
        {
            c.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
        })
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter();
});

builder.Services.AddSingleton(TracerProvider.Default.GetTracer(serviceName));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbConnection service to DI
builder.Services.AddTransient<IDbConnection>(db =>
    new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();