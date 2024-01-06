using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
    public class StreamService : IIntegrationService
    {
        private static HttpClient _httpClient = new HttpClient();
        public StreamService()
        {
            // set up HttpClient instance
            _httpClient.BaseAddress = new Uri("http://localhost:57863");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
        }

        public async Task Run()
        {
            //await GetPosterWithStream();
            await GetPosterWithStreamAndCompletionMode();
        }

        /// <summary>
        /// Read Json response as Stream - Better and more memory efficient
        /// </summary>
        /// <returns></returns>
        private async Task GetPosterWithStream()
        {
            var request = new HttpRequestMessage(
                  HttpMethod.Get,
                  $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            //Reading content as Stream
            //Since its stream, we need to dispose it off. Wrap it in a using block
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                //Using StreamReader to read the incoming stream(In our case content)
                using (var streamReader = new StreamReader(stream))
                {
                    //Using JsonTextReader to read JsonData
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        var jsonSerializer = new JsonSerializer();
                        var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);
                    }
                }
            }
        }

        private async Task GetPosterWithStreamAndCompletionMode()
        {
            var request = new HttpRequestMessage(
                  HttpMethod.Get,
                  $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //Using overload of SendAsync. Default is ResponseContentRead
            //With ResponseHeadersRead(Default), the client execution is blocked till ENTIRE response is received by the caller
            //With ResponseHeadersRead, client execution is resumed as soon as HEADER has been received and CONTENT IS STILL BEING DOWNLOADED
            var response = await _httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            using (var stream = await response.Content.ReadAsStreamAsync())
            {

                //Using StreamReader to read the incoming stream(In our case content)
                using (var streamReader = new StreamReader(stream))
                {
                    //Using JsonTextReader to read JsonData
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        var jsonSerializer = new JsonSerializer();
                        var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);
                    }
                }
            }
        }

    }
}
