# aspnetcore-httpclient

- Request Header
	- Accept - Tells the server the mediatype client is willing to accept
		- application/xml
		- application/json

- Response Header
	- Content-Type - The type of content that message body has
		- application/xml
		- application/json
		- text/html


- Content Negotiation
	- Mechanism used for serving different representations of a resource at the same URI
	- Request Headers
		- Contains information on resource to be fetched or about client itself
		- Supplied by client
		- Considered as best practice to be as strict as possible. Setting accept header improves reliability
		- Most common header
			- Accept: application/json, application/xml, text/html
		- Others
			- Accept-Encoding
			- Accept-Language
			- Accept-Charset
		- We can specify more than 1 possible content types in the Accept Headers that we are willing to accept response in.
		- Equal Preference
			- If we pass Accept: application/json, application/xml - We are giving EQUAL Preference to both types of content
			- If we pass Accept: application/json, application/xml;q=0.9 -  Code is giving higher preference to Json

		- In the HTTP "Accept" header, the "q" parameter is used to indicate the quality (or preference) of a media type. The "q" parameter has a value between 0 and 1, where 1 is the highest quality, and 0 is the lowest. If the "q" parameter is not specified, it is assumed to be 1.
		- For example:
			- Accept: application/json;q=0.8, text/html;q=0.6, */*;q=0.1

		- In this example:
			- application/json has a quality of 0.8.
			- text/html has a quality of 0.6.
			- */* (any media type) has a quality of 0.1.

		- Here are some common values and their meanings:

			- 1.0: The client explicitly prefers this media type.
			- 0.9: High preference but not explicit.
			- 0.8 to 0.5: Moderate preference.
			- 0.4 to 0.1: Low preference.

	- Response headers
		- Contain information on generated response or about server
		- Provided by server
		- Common headers
			- Content-Type: application/json

	- Nowadays, XML & Json are not enough. There are chances that within code, if we are calling an external vendor, different endpoints would return data in different format. Setting Accept header does not make sense
	- This is where HttpRequestMessage comes in

	- Using HttpRequestMessage is a more fine-grained low level module using which we can individually prepare the requests to work with different types of Verbs

	- HttpRequestMessage.Content - Type - HttpContent
	- Use a derived class that matches content of message
		- StringContent
		- ObjectContent
		- ByteArrayContent
		- StreamContent..


	- 2 ways to perform CRUD operation (Refer CRUDService)
		- Using HttpRequestMessage
		- Using Shortcut methods

	- 4.Partial Updates
		- PUT is intended for full updates
		- Ideally, PUT is less used since for updating, we need to first get the full resource and then pass it along in the request
		- Today, best practice is to use PATCH instead of PUT when updating
		- Refer standard here - https://datatracker.ietf.org/doc/html/rfc6902
		- Need Nuget package to be installed to support patch operation - Microsoft.AspNetCore.JsonPatch
		- This uses Newtonsoft.Json under the hood - So, we need to ensure that we use that library for serializing our object
		- WE also need to make sure to pass application/json-patch+json as the content type

		- Newtonsoft.Json(Json.NET) vs System.Text.Json
			- System.Text.Json - Focused on speed by using Span<T> but misses some advanced functionality
			- Json.NET - Focused on set of advanced features. Both are great choices
			
		- Patch is very powerful but generally, APIs only support changes in PATCH that go 1 level deep

	- 5. Improving Performance and Memory use with Streams
		- Stream = Abstraction of sequence of bytes such as file, an I/O device or network traffic
		- Classes derived from Stream hide specific details of OS and underlying devices
		- Streams help in avoiding large in-between(Temporary) variables
			- Better for memory use
			- Better for performance
		- API does not need to work with streams to get these advantages at client level
		- Up until now, when we made a get request, 
			- We wait for content to arrive
			- We parse content as string - THIS IS TEMPORARY STRING
			- Deserialize string into POCO

		- This can be improved by removing temporary string assignment and directly reading content as STREAM and deserializing it DIRECTLY
		- response.Content.ReadAsStreamAsync()

		- For working with streams, we need it to be configurable. So, that configuration can be found here - https://github.com/KevinDockx/StreamExtensions/tree/master/src/Marvin.StreamExtensions

		- Conclusion
			- Creating an disposing streams can cause some overhead
			- Using streams keeps memory use low. 
			- Since we do not have that temporary strings here, runtime does not have to invoke garbage collection. This too has a positive impact on performance
			- To keep performance consistently good, using less memory is advisable
			- Advise
				- Always use streams when reading data
				- Use streams for POST/PUT/PATCH Large amounts of data(For positive impact on memory use)
				- If not sure, test


		- Using Compression to reduce bandwidth
			- There are compression providers available in .NET such as GZip and Brotli
			- For Enabling Compression, On API side, 
				- Add compression services using services.AddResponseCompression();
				- Use it in middleware pipeline - app.UseResponseCompression();

			- From client app, when creating HttpClient, create HttpClientHandler and setup AutomaticDecompression = System.Net.DecompressionMethods.GZip in it.


	- 6. Supporting Cancellation
		- BTS HttpClient works with async Tasks
			- Cancelling such task potentially frees up a thread
			- Thread is returned to threadpool for it to be used elsewhere
			- This improves scalability of our app

		- There are 2 ways Task can be canceled
			- We cancel Task
			- Timeout occurs. We should be able to gracefully handle canceled task

		- CancellationTokenSource
			- Manages and sends cancellation notifications

		- CancellationToken
			- Exposed through token property of CancellationTokenSource

		- Asking for cancellation notifies receiver of token that it should cancel its task. So, HTTPClient listens to this cancellation and then takes action to cancel the request

		- Using CancellationTokenSource, we can explicitly Cancel request by calling Cancel() or CancelAfter(Timespan time) method. This will trigger cancellation
		- We need to make sure we pass Token from CancellationTokenSource object to make sure cancellation actually happens after calling cancel() or CancelAfter()

		- Another way to cancel request is through timeout. When configuring HttpClient, make sure we set a timeout. If we set it for 2 seconds, after 2 seconds, HttpClient will throw OperationCanceled Exception

	- 7. Improving HttpClient Instance Management with HttpClientFactory
		- We should not dispose HttpClient (Do not wrap it in a using statement)
		- If we dispose HttpClient, underlying HttpClientHandler is also disposed which closes the underlying connection
			- Reopening connection is slow
			- As it takes time to close connection, we might not have socket available for new one (Resulting in SocketExceptions)

		- Problem#1
			- When we wrap HttpClient in using statement, we see that lot of connections stay in TIME_WAIT state.
			- This will remain in TIME_WAIT state for a default of 240 seconds(Till that time the socket would be occupied)
			- This can be solved by reusing HttpClient

		- Problem#2
			- If we reuse HttpClient, it results in another issue i.e. DNS changes would not be honoured.
			- This could result in requests being served by incorrect server

		- Clean and Efficient Solution
			- Use HttpClientFactory
			- HttpClientFactory - Instantiates HttpClient but reuses the handler from HttpMessageHandler Pool
			- The handlers is held for 2 minutes by default. So, any new HTTP request can reuse the httpclient and handler created previously i.e. available in Pool
			- This is how both problems are solved
				- Reusing handlers allows reusing underlying connections which solve socket issue
				- Disposing handler after 2 mins solves DNS issue

		- HttpClient Factory provides central location for naming and configuring logical HttpClients
		- For these, we can configure handlers and policies
		- Install NuGet - Microsoft.Extensions.Http package
		- Register HttpClient services in DI
		- Within services, inject IHttpClientFactory and whenever we need to make a call to the API, call the CreateClient() method that will return HttpClient
		- Different implementations
			- Default clients
			- Named clients
			- Typed clients
		- Check HttpClientFactoryInstanceManagementService for more details

	- 8. Handling faults and errors
		- Status Codes
			- EnsureSuccessStatusCode() throws HttpRequestException on all but 2xx level status codes
			- Depending on actual status code, we want to act differently
			- Importance of Status Codes
				- Level 200(Success)
					200 - OK
					201 - Created
					204 - No Content - success delete request

				- Level 400 (API correctly rejects requets)
					400 - Bad Request
					401 - Unauthorized
					403 - Forbidden - Authentication success but user does not have access to perform operation
					404 - Not Found
					422 - Unprocessable Entity - Semantic mistakes. Validation failed

				- Level 500 (Faults)
					500 - Internal Server error

		- Ideally, we should handle whatever response codes we know the API could send back. Like 404, 401 etc.
		- For rest of them, we should keep response.EnsureSuccessStatusCode() call - This would throw HttpRequestException if matching condition not found.
		- Refer DealingWithErrorsAndFaultsService.cs

		- When error happens, API can return additional information on error in response body such as:
			- Error Messages
			- Validation errors etc.
		- Not all APIs use status codes correctly
			- At times, API would return 200OK always and populate response body with additional details
			- So, in such cases use combination of status codes and response message to handle response

	- 9. Custom Message Handlers
		- When we invoke HttpClient request, it forms the HttpRequestMessage.
		- Then it goes through Http Message Handlers(1..*) if they are configured as HttpRequest Pipeline
		- The last handler would be HttpClientHandler
		- The same set of handlers are executed in reverse order
		- After that it forms HttpResponseMessage and returns it back to caller
		- We can achieve things like:
			- Token Propagation
			- Retry
			- Timeout etc

		- Retry Policy
			- Requests might fail due to a network hick-up or temporary connection issue
			- Retry policy states that if request fails, it should try again(For set number of times)
			- Refer RetryDelegatingHandler
				- Inherit Delegating handler
				- Override SendAsync() method
				- Make sure to register delegating handler in the HTTP request pipeline

		- Timeout
			- Requests can timeout
			- We can use delegating handler to throw TimeoutException instead of TaskCancelledException