using Movies.Client.Exceptions;
using Movies.Client;
using System.Net;
using Moq;
using Moq.Protected;

namespace Movies.UnitTests
{
    public class TestableClassWithApiAccessUnitTests
    {
        [Fact]
        public async void GetMovie_On401Response_MustThrowUnauthorizedApiAccessException()
        {
            //Specifying the Return401UnauthorizedResponseHandler handler here
            var httpClient = new HttpClient(new Return401UnauthorizedResponseHandler())
            {
                BaseAddress = new Uri("http://localhost:57863")
            };

            //Pass httpclient to TestableClassWithApiAccess class
            var testableClass = new TestableClassWithApiAccess(httpClient);

            await Assert.ThrowsAsync<UnauthorizedApiAccessException>(
                  () => testableClass.GetMovie(CancellationToken.None));
        }

        [Fact]
        public async void GetMovie_On401Response_MustThrowUnauthorizedApiAccessException_WithMoq()
        {
            //Mocking an HTTP message handler so that we don't have to create custom handlers for all test use-cases
            var unauthorizedResponseHttpMessageHandlerMock = new Mock<HttpMessageHandler>();

            //Since we want to mock SendAsync method which is protected, we won't be able to directly mock it. We need to use Mock.Protected() to do so.
            unauthorizedResponseHttpMessageHandlerMock
              .Protected()
              .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
              .ReturnsAsync(new HttpResponseMessage()
              {
                  StatusCode = HttpStatusCode.Unauthorized
              });

            var httpClient = new HttpClient(unauthorizedResponseHttpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:57863")
            };

            var testableClass = new TestableClassWithApiAccess(httpClient);

            await Assert.ThrowsAsync<UnauthorizedApiAccessException>(
                () => testableClass.GetMovie(CancellationToken.None));
        }
    }
}