using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
            //await GetPosterWithStreamAndCompletionMode();
            await PostPosterWithStream();

            //Test methods
            //await TestGetPosterWithoutStream();
            //await TestGetPosterWithStream();
            //await TestGetPosterWithStreamAndCompletionMode();
        }

        #region Get Methods
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
                ////Using StreamReader to read the incoming stream(In our case content)
                //using (var streamReader = new StreamReader(stream))
                //{
                //    //Using JsonTextReader to read JsonData
                //    using (var jsonTextReader = new JsonTextReader(streamReader))
                //    {
                //        var jsonSerializer = new JsonSerializer();
                //        var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);
                //    }
                //}

                //Refactored here
                var poster = stream.ReadAndDeserializeFromJson<Poster>();
            }
        }

        /// <summary>
        /// Read Json response as stream with Completion mode as Header - Better
        /// </summary>
        /// <returns></returns>
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

                ////Using StreamReader to read the incoming stream(In our case content)
                //using (var streamReader = new StreamReader(stream))
                //{
                //    //Using JsonTextReader to read JsonData
                //    using (var jsonTextReader = new JsonTextReader(streamReader))
                //    {
                //        var jsonSerializer = new JsonSerializer();
                //        var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);
                //    }
                //}

                //Refactored here
                var poster = stream.ReadAndDeserializeFromJson<Poster>();
            }
        }

        /// <summary>
        /// Method that will be used to test
        /// </summary>
        /// <returns></returns>
        private async Task GetPosterWithoutStream()
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var posters = JsonConvert.DeserializeObject<Poster>(content);
        }

        #endregion

        #region Post Methods
        private async Task PostPosterWithStream()
        {
            // generate a movie poster of 500KB
            var random = new Random();
            var generatedBytes = new byte[524288];
            random.NextBytes(generatedBytes);

            var posterForCreation = new PosterForCreation(generatedBytes, "A new poster for The Big Lebowski");
            
            //This is what we used to convert object to string in CRUD Service
            // ==> JsonConvert.SerializeObject(posterForCreation);

            //This is going to hold our data when posting it
            var memoryContentStream = new MemoryStream();
            
            //Serialize to Json and Write to memory stream
            memoryContentStream.SerializeToJsonAndWrite(posterForCreation);

            //Set to position 0 since that's where we want to start streaming it from
            memoryContentStream.Seek(0, SeekOrigin.Begin);

            using (var request = new HttpRequestMessage(
               HttpMethod.Post,
               $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (var streamContent = new StreamContent(memoryContentStream))
                {
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    request.Content = streamContent;

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var createdContent = await response.Content.ReadAsStringAsync();
                    var createdPoster = JsonConvert.DeserializeObject<Poster>(createdContent);

                    // do something with the newly created poster     
                }
            }
        }
        #endregion

        #region Test Methods
        /// <summary>
        /// Test Method
        /// </summary>
        /// <returns></returns>
        public async Task TestGetPosterWithoutStream()
        {
            // warmup - So that first run does not get calculated in performance
            await GetPosterWithoutStream();

            // start stopwatch 
            var stopWatch = Stopwatch.StartNew();

            // run requests
            for (int i = 0; i < 200; i++)
            {
                await GetPosterWithoutStream();
            }

            // stop stopwatch
            stopWatch.Stop();
            Console.WriteLine($"Elapsed milliseconds without stream: " +
                $"{stopWatch.ElapsedMilliseconds}, " +
                $"averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
        }

        /// <summary>
        /// Test Method
        /// </summary>
        /// <returns></returns>
        public async Task TestGetPosterWithStream()
        {
            // warmup - So that first run does not get calculated in performance
            await GetPosterWithStream();

            // start stopwatch 
            var stopWatch = Stopwatch.StartNew();

            // run requests
            for (int i = 0; i < 200; i++)
            {
                await GetPosterWithStream();
            }

            // stop stopwatch
            stopWatch.Stop();
            Console.WriteLine($"Elapsed milliseconds with stream: " +
                $"{stopWatch.ElapsedMilliseconds}, " +
                $"averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
        }

        /// <summary>
        /// Test Method
        /// </summary>
        /// <returns></returns>
        public async Task TestGetPosterWithStreamAndCompletionMode()
        {
            // warmup - So that first run does not get calculated in performance
            await GetPosterWithStreamAndCompletionMode();

            // start stopwatch 
            var stopWatch = Stopwatch.StartNew();

            // run requests
            for (int i = 0; i < 200; i++)
            {
                await GetPosterWithStreamAndCompletionMode();
            }

            // stop stopwatch
            stopWatch.Stop();
            Console.WriteLine($"Elapsed milliseconds with stream and completionmode: " +
                $"{stopWatch.ElapsedMilliseconds}, " +
                $"averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
        }
        #endregion
    }
}
