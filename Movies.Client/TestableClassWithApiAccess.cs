using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Movies.Client.Models;
using Marvin.StreamExtensions;
using Movies.Client.Exceptions;

namespace Movies.Client
{
    public class TestableClassWithApiAccess
    {
        private readonly HttpClient _httpClient;

        //This httpclient instance would be provided by UT
        public TestableClassWithApiAccess(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Test method
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedApiAccessException"></exception>
        public async Task<Movie> GetMovie(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/movies/030a43b0-f9a5-405a-811c-bf342524b2be");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using (var response = await _httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // inspect the status code
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // show this to the user
                        Console.WriteLine("The requested movie cannot be found.");
                        return null;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // trigger a login flow
                        throw new UnauthorizedApiAccessException();
                    }

                    response.EnsureSuccessStatusCode();
                }
                return stream.ReadAndDeserializeFromJson<Movie>();
            }
        }

    }
}
