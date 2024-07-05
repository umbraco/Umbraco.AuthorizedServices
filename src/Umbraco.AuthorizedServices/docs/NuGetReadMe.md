**Umbraco Authorized Services** is an Umbraco package designed to reduce the effort needed to integrate third party solutions that require authentication and authorization via an OAuth flow into Umbraco solutions.  It's based on the premise that working with these services requires a fair bit of plumbing code to handle creating an authorized connection.

This is necessary before the developer working with the service can get to actually using the provided API to implement the business requirements.

Having worked with a few OAuth integrations across different providers, as would be expected, there are quite a few similarities to the flow that needs to be implemented.  Steps include:

- Redirecting to an authentication endpoint.
- Handling the response including an authentication code and exchanging it for an access token.
- Securely storing the token.
- Including the token in API requests.
- Serializing requests and deserializing the API responses.
- Handling cases where the token has expired and obtaining a new one via a refresh token.

There are though also differences, across request and response structures and variations in the details of the flow itself.

The idea of the package is to try to implement a single, best practice implementation of working with OAuth that can be customized, via configuration or code, for particular providers.

For more information please see the [package documentation](https://docs.umbraco.com/umbraco-dxp/packages/authorized-services) and/or [open-source code repository](https://github.com/umbraco/Umbraco.AuthorizedServices).
