using Marvin.StreamExtensions;
using Movies.Client.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
    public class CancellationService : IIntegrationService
    {
        private static HttpClient _httpClient = new HttpClient(
            new HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip
            });

        public CancellationService()
        {
            // set up HttpClient instance
            _httpClient.BaseAddress = new Uri("http://localhost:57863");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
        }

        public async Task Run()
        {
            await GetTrailerAndCancel();
        }

        private async Task GetTrailerAndCancel()
        {
            var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/trailers/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            //Approach#1 - Creating CancellationTokenSource object and setting making sure Cancellation is forcefully triggered bu calling - Cancel/CancelAfter() methods
            var cancellationTokenSource = new CancellationTokenSource();
            //This will cancel after 1 second
            cancellationTokenSource.CancelAfter(1000);

            using (var response = await _httpClient.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationTokenSource.Token))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                response.EnsureSuccessStatusCode();
                var trailer = stream.ReadAndDeserializeFromJson<Trailer>();
            }
        }
    }
}
