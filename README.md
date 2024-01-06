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

