using Movies.Client.Exceptions;
using Movies.Client;

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
    }
}