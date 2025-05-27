using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReportService.Services
{
    public class ContactReportService
    {
        private readonly HttpClient _httpClient;

        public ContactReportService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://contactservice:5002/");
        }

        public async Task<ContactStats> GetContactStatsAsync(string location)
        {
            var response = await _httpClient.GetAsync($"api/Contact/stats?location={location}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ContactStats>(content);
        }
    }

    public class ContactStats
    {
        public int PersonCount { get; set; }
        public int PhoneNumberCount { get; set; }
    }
} 