using CargaOpinionesClientes.Worker;
using CargaOpinionesClientes.Worker.Extractors;
using CargaOpinionesClientes.Worker.Interfaces;
using CargaOpinionesClientes.Worker.Persistence;
using CargaOpinionesClientes.Worker.Services;
using Microsoft.EntityFrameworkCore;

var builder =
    Host.CreateApplicationBuilder(args);

var transactionalConnection =
    builder.Configuration.GetConnectionString(
        "TransactionalDatabase")
    ?? throw new InvalidOperationException(
        "No se encontró TransactionalDatabase.");

var warehouseConnection =
    builder.Configuration.GetConnectionString(
        "DataWarehouseDatabase")
    ?? throw new InvalidOperationException(
        "No se encontró DataWarehouseDatabase.");

builder.Services.AddDbContext<TransactionalDbContext>(
    options =>
    {
        options.UseSqlServer(
            transactionalConnection,
            sqlOptions =>
            {
                sqlOptions.CommandTimeout(60);
                sqlOptions.EnableRetryOnFailure();
            });
    });

builder.Services.AddDbContext<DataWarehouseDbContext>(
    options =>
    {
        options.UseSqlServer(
            warehouseConnection,
            sqlOptions =>
            {
                sqlOptions.CommandTimeout(60);
                sqlOptions.EnableRetryOnFailure();
            });
    });

builder.Services.AddScoped<
    IDimensionLoadService,
    DimensionLoadService>();

builder.Services.AddScoped<
    IFactLoadService,
    FactLoadService>();

builder.Services.AddSingleton<
    StagingWriterService>();

builder.Services.AddSingleton<
    ExtractionOrchestratorService>();

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

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();