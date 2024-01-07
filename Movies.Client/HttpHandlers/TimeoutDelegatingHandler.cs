using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client.HttpHandlers
{
    public class TimeoutDelegatingHandler : DelegatingHandler
    {
        //Specifying default 100 seconds timeout
        //It will override value passed by caller
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(100);

        public TimeoutDelegatingHandler(TimeSpan timeout) : base()
        {
            _timeout = timeout;
        }

        public TimeoutDelegatingHandler(HttpMessageHandler innerHandler, TimeSpan timeout) : base(innerHandler)
        {
            _timeout = timeout;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            //Creating LinkedTokenSource. Here we are explicitly creating a new cancellationToken and telling it to cancel after timeout seconds pass
            //Purpose behind using LinkedTokenSource is - If the caller cancels the request, then that cancellation information would be propagated to this 
            //Newly created LinkedToken as well
            //2 scenario possible:
            //1 - Timeout - In that case, the token created here will initiate cancellation
            //2 - Caller canceled request - In this case, cancellationToken passed to the method will be the reason for cancelation. Because it is linked to this token, the cancellation would happen since its linked to this newly created Token.
            using (var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                linkedCancellationTokenSource.CancelAfter(_timeout);
                try
                {
                    return await base.SendAsync(request, cancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    //Using this we check if the Cancellation was requeseted on the original request. If this is false, meaning it's timeout that is resulting in cancelation.
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        throw new TimeoutException("Request timed out", ex);
                    }
                    throw;
                }
            }
            
        }
    }
}
