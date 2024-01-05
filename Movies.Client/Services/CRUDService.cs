using Movies.Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Xml.Serialization;
using System.IO;

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

            //Ideally no headers are set but its' generally good practice to clear the request headers(If they're set by any other system)
            _httpClient.DefaultRequestHeaders.Clear();

            //Adding XML. Telling that both options are valid(XML and JSON). Setting overload to 0.9 for Xml
            //_httpClient.DefaultRequestHeaders.Accept.Add(
            //    new MediaTypeWithQualityHeaderValue("application/xml", 0.9));

            //Setting accept headers. First as Json. It's a collection, we could specify more than 1 accept headers
            //Would default to Quality as 1 for application/json - Meaning highest priority
            //_httpClient.DefaultRequestHeaders.Accept.Add(
            //    new MediaTypeWithQualityHeaderValue("application/json"));
   
        }
        public async Task Run()
        {
            //await GetResource();
            //await GetResourceThroughHttpRequestMessage();
            //await CreateResource();
            //await UpdateResource();
            await DeleteResource();
        }

        /// <summary>
        /// Plain get call using HttpClient
        /// </summary>
        /// <returns></returns>
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

            var movies = new List<Movie>();

            //Inspecting response header to decide
            if (response.Content.Headers.ContentType.MediaType == "application/json")
            {
                //Using system.text.json
                //Failed - Movies are not getting deserialized from Json to object. Since model has JsonProperty decorated in the deserialized object.(See Movie)
                //var movies = JsonSerializer.Deserialize<IEnumerable<Movie>>(content);

                //Success - Explicitly passing JsonSerializerOption to deserialize into Camel case
                movies = JsonSerializer.Deserialize<List<Movie>>(content, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            else if(response.Content.Headers.ContentType.MediaType == "application/xml")
            {
                var serializer = new XmlSerializer((typeof(List<Movie>)));
                movies = (List<Movie>)serializer.Deserialize(new StringReader(content));
            }

            //Do something with movie list
        }

        #region Using HttpRequestMessage
        /// <summary>
        /// Get call via HttpRequestMessage
        /// </summary>
        /// <returns></returns>
        private async Task GetResourceThroughHttpRequestMessage()
        {
            //Any default set on HttpClient will be ignored here
            var request = new HttpRequestMessage(HttpMethod.Get, "api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //Using SendAsync - We can pass in any type of request here but with HttpRequestMessage
            var response = await _httpClient.SendAsync(request);
            
            //No changes here
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var movies = JsonSerializer.Deserialize<List<Movie>>(content, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Post call via HttpRequestMessage
        /// </summary>
        /// <returns></returns>
        private async Task CreateResource()
        {
            var movieToCreate = new MovieForCreation("Description", Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), "Crime, Drama", new DateTimeOffset(new DateTime(2024, 01, 01)), "Jawan");

            //This will serialize with default settings
            var serializedMovieToCreate = JsonSerializer.Serialize(movieToCreate);

            var request = new HttpRequestMessage(HttpMethod.Post, "api/movies");

            //Informing server, we're willing to accept response in application/json
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //Setting content - Approach#1
            request.Content = new StringContent(serializedMovieToCreate);

            //Setting content - Approach#2 - In this case, no need to explicitly set content type header
            //request.Content = new StringContent(serializedMovieToCreate, Encoding.UTF8, "application/json");

            //Setting content type so that server knows how to interpret the request
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var createdMovie = JsonSerializer.Deserialize<Movie>(content, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// PUT = Full update only
        /// </summary>
        /// <returns></returns>
        private async Task UpdateResource()
        {
            var movieToUpdate = new MovieForUpdate("Description1", Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), "Crime, Drama", new DateTimeOffset(new DateTime(2024, 01, 01)), "JawanUpdated");

            //This will serialize with default settings
            var serializedMovieToUpdate = JsonSerializer.Serialize(movieToUpdate);

            var request = new HttpRequestMessage(HttpMethod.Put, "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //Setting content - Approach#1
            request.Content = new StringContent(serializedMovieToUpdate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var updatedMovie = JsonSerializer.Deserialize<Movie>(content, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Successful delete request returns 204 - NoContent.
        /// </summary>
        /// <returns></returns>
        private async Task DeleteResource()
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
        }
        #endregion

        #region Using ShortCut methods
        /// <summary>
        /// Using PostAsync shortcut method
        /// </summary>
        /// <returns></returns>
        private async Task PostResourceShortcut()
        {
            var movieToCreate = new MovieForCreation("Description", Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), "Crime, Drama", new DateTimeOffset(new DateTime(2024, 01, 01)), "Jawan");

            var response = await _httpClient.PostAsync(
                "api/movies",
                new StringContent(
                    JsonSerializer.Serialize(movieToCreate),
                    Encoding.UTF8,
                    "application/json"));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var createdMovie = JsonSerializer.Deserialize<Movie>(content,
               new JsonSerializerOptions
               {
                   PropertyNamingPolicy = JsonNamingPolicy.CamelCase
               });
        }

        /// <summary>
        /// Using PutAsync shortcut method
        /// </summary>
        /// <returns></returns>
        private async Task PutResourceShortcut()
        {
            var movieToUpdate = new MovieForUpdate("Description1", Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), "Crime, Drama", new DateTimeOffset(new DateTime(2024, 01, 01)), "JawanUpdated");

            var response = await _httpClient.PutAsync(
               "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b",
               new StringContent(
                   JsonSerializer.Serialize(movieToUpdate),
                   System.Text.Encoding.UTF8,
                   "application/json"));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var updatedMovie = JsonSerializer.Deserialize<Movie>(content,
               new JsonSerializerOptions
               {
                   PropertyNamingPolicy = JsonNamingPolicy.CamelCase
               });
        }

        /// <summary>
        /// Using DeleteAsync shortcut method
        /// </summary>
        /// <returns></returns>
        private async Task DeleteResourceShortcut()
        {
            var response = await _httpClient.DeleteAsync(
                "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
        }
        #endregion
    }
}