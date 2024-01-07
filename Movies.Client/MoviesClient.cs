using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Client
{
    public class MoviesClient
    {
        public MoviesClient(HttpClient client)
        {
            Client = client;
        }

        public HttpClient Client { get; }
    }
}
