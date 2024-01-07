using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client
{
    /// <summary>
    /// Using this delegating handler in Unit tests.
    /// This would help us write a unit test that mimicks as if we got Unauthorized response from actual downstream API
    /// </summary>
    public class Return401UnauthorizedResponseHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            return Task.FromResult(response);
        }
    }
}
