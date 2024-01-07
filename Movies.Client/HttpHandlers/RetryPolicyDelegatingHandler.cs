using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client.HttpHandlers
{
    public class RetryPolicyDelegatingHandler : DelegatingHandler
    {
        private readonly int _maximumAmountOfRetries = 3;

        //Delegating Handler has 2 constructors
        //First without inner handler - Called when the executing delegating handler is the last handler in the request pipeline
        //Second with inner handler - Called when the executing delegating handler has to call the next handler in the pipeline

        public RetryPolicyDelegatingHandler(int maximumAmountOfRetries) 
            : base()
        {
            _maximumAmountOfRetries = maximumAmountOfRetries;
        }

        /// <summary>
        /// This is required if someone directly instantiates RetryPolicyDelegatingHandler inside httpclient
        /// </summary>
        /// <param name="innerHandler"></param>
        /// <param name="maximumAmountOfRetries"></param>
        public RetryPolicyDelegatingHandler(HttpMessageHandler innerHandler, int maximumAmountOfRetries) 
            : base(innerHandler)
        {
            _maximumAmountOfRetries = maximumAmountOfRetries;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;

            for(int i = 0; i < _maximumAmountOfRetries; i++)
            {
                //Passing request to next handler in the pipeline
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    //If response is success, it'll return from here
                    return response;
                }
            }

            //If after retries, it did not get a success one, it will return from here
            return response;
        }
    }
}
