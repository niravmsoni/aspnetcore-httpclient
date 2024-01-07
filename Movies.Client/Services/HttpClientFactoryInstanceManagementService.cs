using Marvin.StreamExtensions;
using Movies.Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
    public class HttpClientFactoryInstanceManagementService : IIntegrationService
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly MoviesClient _moviesClient;

        public HttpClientFactoryInstanceManagementService(IHttpClientFactory httpClientFactory, MoviesClient moviesClient)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _moviesClient = moviesClient ?? throw new ArgumentNullException(nameof(moviesClient));

        }

        public async Task Run()
        {
            //await TestDisposeHttpClient(_cancellationTokenSource.Token);
            //await TestReuseHttpClient(_cancellationTokenSource.Token);
            //await GetMoviesWithHttpClientFromFactory(_cancellationTokenSource.Token);
            //await GetMoviesWithNamedHttpClientFromFactory(_cancellationTokenSource.Token);
            await GetMoviesWithTypedHttpClientFromFactory(_cancellationTokenSource.Token);
        }


        #region Understanding HttpClient instance management without IHttpClientFactory
        /// <summary>
        /// Bad example - Opens up 10 sockets and blocks them till 240 seconds (in TIME_WAIT state)
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task TestDisposeHttpClient(CancellationToken cancellationToken)
        {
            for (var i = 0; i < 10; i++)
            {
                using (var httpClient = new HttpClient())
                {
                    var request = new HttpRequestMessage(
                           HttpMethod.Get,
                           "https://www.google.com");

                    using (var response = await httpClient.SendAsync(request,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken))
                    {
                        var stream = await response.Content.ReadAsStreamAsync();
                        response.EnsureSuccessStatusCode();

                        Console.WriteLine($"Request completed with status code {response.StatusCode}");
                    }
                }
            }
        }

        /// <summary>
        /// Relatively better - WE are reusing same client for multiple calls.
        /// Problem here would be DNS changes won't be honoured
        /// Can lead to requests not arriving at correct server
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task TestReuseHttpClient(CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient();

            for (int i = 0; i < 10; i++)
            {
                var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://www.google.com");

                using (var response = await httpClient.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken))
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    response.EnsureSuccessStatusCode();

                    Console.WriteLine($"Request completed with status code {response.StatusCode}");
                }
            }
        }

        #endregion

        #region Using HttpClient instances from IHttpClientFactory
        /// <summary>
        /// Getting HttpClient instance from HttpClientFactory
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task GetMoviesWithHttpClientFromFactory(CancellationToken cancellationToken)
        {
            //Create HttpClient
            var httpClient = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://localhost:57863/api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var response = await httpClient.SendAsync(request,
               HttpCompletionOption.ResponseHeadersRead,
               cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();
                var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();
            }
        }

        /// <summary>
        /// Getting HttpClient instance from HttpClientFactory using Named Client
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task GetMoviesWithNamedHttpClientFromFactory(CancellationToken cancellationToken)
        {
            //This will also work but it won't have our defaults configured
            //var httpClient = _httpClientFactory.CreateClient();
            //This would have all defaults(BaseAddress, Header, Timeout etc.) configured
            var httpClient = _httpClientFactory.CreateClient("MoviesClient");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using (var response = await httpClient.SendAsync(request,
               HttpCompletionOption.ResponseHeadersRead,
               cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();
                var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();
            }
        }

        /// <summary>
        /// Getting HttpClient instance from HttpClientFactory using Typed Client
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task GetMoviesWithTypedHttpClientFromFactory(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using (var response = await _moviesClient.Client.SendAsync(request,
               HttpCompletionOption.ResponseHeadersRead,
               cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();
                var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();
            }
        }
        #endregion
    }
}
