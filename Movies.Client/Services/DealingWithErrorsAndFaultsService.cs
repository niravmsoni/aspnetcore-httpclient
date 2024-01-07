using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Marvin.StreamExtensions;

namespace Movies.Client.Services
{
    public class DealingWithErrorsAndFaultsService : IIntegrationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public DealingWithErrorsAndFaultsService(IHttpClientFactory httpClientFactory,
            MoviesClient moviesClient)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task Run()
        {
            //await GetMovieAndDealWithInvalidResponses(_cancellationTokenSource.Token);
            await PostMovieAndHandleValdationErrors(_cancellationTokenSource.Token);
        }

        /// <summary>
        /// Get request. Handled 404 and 401. For rest, EnsureSuccessStatusCode() method will handle
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task GetMovieAndDealWithInvalidResponses(CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("MoviesClient");

            //Passing in wrong Guid in Get request. Should fail now
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/movies/030a43b0-f9a5-405a-811c-bf342524b2be");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using (var response = await httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken))
            {
                if (!response.IsSuccessStatusCode)
                {
                    // inspect the status code
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // show this to the user
                        Console.WriteLine("The requested movie cannot be found.");
                        return;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // Trigger a login flow
                        return;
                    }

                    //Throw exception for other status codes
                    response.EnsureSuccessStatusCode();
                }
                var stream = await response.Content.ReadAsStreamAsync();

                var movie = stream.ReadAndDeserializeFromJson<Movie>();
            }
        }

        /// <summary>
        /// Creating a post request with invalid request body. Handled 422 - Unprocessable entity
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task PostMovieAndHandleValdationErrors(
            CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("MoviesClient");

            //Bad Request model
            var movieForCreation = new MovieForCreation(null, Guid.Empty, null, new DateTimeOffset(), "Pulp Fiction");

            var serializedMovieForCreation = JsonConvert.SerializeObject(movieForCreation);

            using (var request = new HttpRequestMessage(
                HttpMethod.Post,
                "api/movies"))
            {
                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                request.Content = new StringContent(serializedMovieForCreation);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                using (var response = await httpClient.SendAsync(request,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken))
                {
                    var stream = await response.Content.ReadAsStreamAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                        {
                            // read out the response body and log it to the console window
                            var validationErrors = stream.ReadAndDeserializeFromJson();
                            Console.WriteLine(validationErrors);
                            return;
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            // trigger a login flow
                            return;
                        }
                        response.EnsureSuccessStatusCode();
                    }

                    //Success response
                    var movie = stream.ReadAndDeserializeFromJson<Movie>();
                }
            }
        }
    }
}
