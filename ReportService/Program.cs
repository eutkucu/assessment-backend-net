using ReportService.Infrastructure;
using ReportService.Infrastructure.Kafka;
using ReportService.Infrastructure.Repositories;
using ReportService.Infrastructure.Interfaces;
using ReportService.Services;
using MongoDB.Driver;
using Confluent.Kafka;
using MediatR;
using System.Reflection;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// MongoDB Configuration
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("MongoDB");
    var client = new MongoClient(connectionString);
    return client.GetDatabase("ReportDB");
});

// MongoDB Context
builder.Services.AddSingleton<MongoDbContext>();

// Repository Registration
builder.Services.AddSingleton<IReportRepository, ReportRepository>();

// Kafka Configuration
builder.Services.AddSingleton<IConsumer<string, string>>(sp =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = builder.Configuration["KafkaSettings:BootstrapServers"],
        GroupId = "report-service-group",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false,
        EnablePartitionEof = true,
        MaxPollIntervalMs = 300000, // 5 dakika
        SessionTimeoutMs = 30000, // 30 saniye
        HeartbeatIntervalMs = 10000 // 10 saniye
    };
    return new ConsumerBuilder<string, string>(config).Build();
});

builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = builder.Configuration["KafkaSettings:BootstrapServers"],
        RetryBackoffMs = 1000,
        MessageTimeoutMs = 5000,
        EnableIdempotence = false // Idempotence'ı devre dışı bırakıyoruz
    };
    return new ProducerBuilder<string, string>(config).Build();
});

// Service Registration
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddSingleton<ReportManager>();
builder.Services.AddSingleton<ContactReportService>();
builder.Services.AddHttpClient<ContactReportService>(client =>
{
    client.BaseAddress = new Uri("http://contactservice:5002/");
});

// Kafka Consumer Service
builder.Services.AddHostedService<KafkaConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global hata yakalama middleware'i - En başta olmalı
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An unhandled exception occurred during request processing");
        
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "Internal Server Error", message = ex.Message });
    }
});

app.UseRouting();
app.UseCors();

app.MapControllers();

// Uygulama başlamadan önce Kafka bağlantısını test et
try
{
    var consumer = app.Services.GetRequiredService<IConsumer<string, string>>();
    var producer = app.Services.GetRequiredService<IProducer<string, string>>();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Kafka bağlantıları başarılı.");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Kafka bağlantı hatası");
}

app.Run();
