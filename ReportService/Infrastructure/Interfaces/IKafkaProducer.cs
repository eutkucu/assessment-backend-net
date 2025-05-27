using System.Threading.Tasks;

namespace ReportService.Infrastructure.Interfaces
{
    public interface IKafkaProducer
    {
        Task ProduceAsync(string key, string value);
    }
} 