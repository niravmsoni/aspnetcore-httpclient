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

	- Response headers
		- Contain information on generated response or about server
		- Provided by server
		- Common headers
			- Content-Type: application/json