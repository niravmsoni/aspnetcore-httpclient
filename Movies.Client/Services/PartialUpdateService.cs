using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Movies.Client.Models;

namespace Movies.Client.Services
{
    public class PartialUpdateService : IIntegrationService
    {
        private static HttpClient _httpClient = new HttpClient();

        public PartialUpdateService()
        {
            // set up HttpClient instance
            _httpClient.BaseAddress = new Uri("http://localhost:57863");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
        }

        public async Task Run()
        {
            await PatchResource();
            //await PatchResourceShortcut();
        }

        /// <summary>
        /// Patch resource - Uses HttpMethod.Patch
        /// </summary>
        /// <returns></returns>
        public async Task PatchResource()
        {
            //Create a patch object with different operations that need to be done on "5b1c2b4d-48c7-402a-80c3-cc796ad49c6b" object
            var patchDoc = new JsonPatchDocument<MovieForUpdate>();
            patchDoc.Replace(m => m.Title, "Updated title");
            patchDoc.Remove(m => m.Description);

            var serializedChangeSet = JsonConvert.SerializeObject(patchDoc);
            var request = new HttpRequestMessage(HttpMethod.Patch,
                "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");


            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(serializedChangeSet);

            //Important to use application/json-patch+json as the contentType
            request.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/json-patch+json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var updatedMovie = JsonConvert.DeserializeObject<Movie>(content);
        }

        /// <summary>
        /// Using Shortcut method - PatchAsync()
        /// </summary>
        /// <returns></returns>
        public async Task PatchResourceShortcut()
        {
            var patchDoc = new JsonPatchDocument<MovieForUpdate>();
            patchDoc.Replace(m => m.Title, "Updated title");
            patchDoc.Remove(m => m.Description);

            var response = await _httpClient.PatchAsync(
              "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b",
              new StringContent(
                  JsonConvert.SerializeObject(patchDoc),
                  Encoding.UTF8,
                  "application/json-patch+json"));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var updatedMovie = JsonConvert.DeserializeObject<Movie>(content);
        }
    }
}
