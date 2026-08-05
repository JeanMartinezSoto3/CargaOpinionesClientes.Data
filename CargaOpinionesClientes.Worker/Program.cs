using CargaOpinionesClientes.Worker;
using CargaOpinionesClientes.Worker.Extractors;
using CargaOpinionesClientes.Worker.Interfaces;
using CargaOpinionesClientes.Worker.Services;

var builder =
    Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<
    ExtractionOrchestratorService>();

builder.Services.AddSingleton<
    StagingWriterService>();

builder.Services.AddSingleton<
    IExtractor,
    CsvExtractor>();

builder.Services.AddSingleton<
    IExtractor,
    DatabaseExtractor>();

builder.Services.AddSingleton<
    IExtractor,
    ApiExtractor>();

builder.Services.AddHttpClient(
    "CommentsApi",
    client =>
    {
        client.Timeout =
            TimeSpan.FromSeconds(30);

        client.DefaultRequestHeaders.Add(
            "User-Agent",
            "CargaOpinionesClientes.Worker");
    });

var host = builder.Build();

await host.RunAsync();