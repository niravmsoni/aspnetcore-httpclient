using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
    public class HttpClientFactoryInstanceManagementService : IIntegrationService
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public async Task Run()
        {
            //await TestDisposeHttpClient(_cancellationTokenSource.Token);
            await TestReuseHttpClient(_cancellationTokenSource.Token);
        }


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
    }
}
