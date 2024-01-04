using Movies.Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
    public class CRUDService : IIntegrationService
    {
        private static HttpClient _httpClient = new HttpClient();

        public CRUDService()
        {
            _httpClient.BaseAddress = new Uri("http://localhost:57863");

            //Timeout of 30 seconds
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
        }
        public async Task Run()
        {
            await GetResource();
        }

        private async Task GetResource()
        {
            //Bad Practise - HttpClient implements IDisposable
            //However, we should not wrap them in a using statement
            //They are meant to be long-lived.
            //using (var httpClient = new HttpClient())
            //{
            //}

            //Combines the base address with whatever is passed in requestUri
            var response = await _httpClient.GetAsync("api/movies");

            response.EnsureSuccessStatusCode();

            //We get actual content from response.Content
            var content = await response.Content.ReadAsStringAsync();

            //Using system.text.json
            //Failed - Movies are not getting deserialized from Json to object. Since model has JsonProperty decorated in the deserialized object.(See Movie)
            //var movies = JsonSerializer.Deserialize<IEnumerable<Movie>>(content);

            //Success - Explicitly passing JsonSerializerOption to deserialize into Camel case
            var movies = JsonSerializer.Deserialize<IEnumerable<Movie>>(content, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}