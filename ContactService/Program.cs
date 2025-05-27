using ContactService.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using ContactService.Services;
using ContactService.Infrastructure.Interfaces;
using ContactService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;

// Guid'ler için serializer kaydı
BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Program başlatılıyor...");

try
{
    Console.WriteLine("Temel servisler ekleniyor...");
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Contact Service API",
            Version = "v1",
            Description = "Contact Service API Documentation"
        });
    });
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });
    
    // Infrastructure services
    builder.Services.AddSingleton<MongoDbContext>();
    
    // Application services
    builder.Services.AddScoped<IPersonService, PersonService>();
    builder.Services.AddScoped<IContactInfoService, ContactInfoService>();
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    
    // Repositories
    builder.Services.AddScoped<IPersonRepository, PersonRepository>();
    builder.Services.AddScoped<IContactInfoRepository, ContactInfoRepository>();
    
    // Kafka Producer ve Consumer
    // builder.Services.AddSingleton<ContactService.Infrastructure.Kafka.KafkaProducer>();
    // builder.Services.AddHostedService<ContactService.Infrastructure.Kafka.KafkaReportRequestConsumer>();
    
    Console.WriteLine("Tüm servisler eklendi.");
}
catch (Exception ex)
{
    Console.WriteLine($"Servis ekleme sırasında hata: {ex.Message}\n{ex}");
    throw;
}

Console.WriteLine("app.Build() çağrılıyor...");
var app = builder.Build();
Console.WriteLine("app.Build() tamamlandı.");

app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contact Service API V1");
    c.RoutePrefix = "swagger";
});
app.UseCors();
app.MapControllers();

try
{
    Console.WriteLine("app.Run() çağrılıyor...");
    app.Run();
    Console.WriteLine("app.Run() sonrası!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error starting the application: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    // Hata durumunda uygulamanın kapanmasını engelle
    Console.WriteLine("Uygulama hata ile karşılaştı ama kapanmayacak. API endpoint'leri hala çalışıyor olabilir.");
    // Uygulamayı sonsuz döngüde tut
    while (true)
    {
        Thread.Sleep(1000);
    }
}
