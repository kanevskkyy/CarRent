# CarRent authentication flow

CarRent supports password sign-in, Google ID-token sign-in, and GitHub OAuth authorization-code sign-in. All successful methods issue the same application JWT and refresh-token pair.

~~~mermaid
sequenceDiagram
    autonumber
    actor User
    participant UI as React client
    participant Provider as Google or GitHub
    participant API as CarRent API
    participant Identity as ASP.NET Identity
    participant DB as SQL Server

    alt Google
        User->>UI: Select Google sign-in
        UI->>Provider: Authenticate user
        Provider-->>UI: Google ID token
        UI->>API: POST /api/auth/google (ID token)
        API->>Provider: Validate ID token
        Provider-->>API: Verified identity claims
    else GitHub
        User->>UI: Select GitHub sign-in
        UI->>Provider: Authorize application
        Provider-->>UI: Redirect with authorization code
        UI->>API: POST /api/auth/github (code)
        API->>Provider: Exchange code and request user profile
        Provider-->>API: Verified GitHub profile
    else Email and password
        User->>UI: Submit credentials
        UI->>API: POST /api/auth/login
        API->>Identity: Validate credentials
        Identity-->>API: User identity
    end

    API->>Identity: Find or create application user
    Identity->>DB: Read or persist user
    API->>DB: Revoke active refresh token(s), persist replacement
    API-->>UI: JWT access token + refresh token + user
    UI->>API: Authenticated request with Bearer JWT
    API-->>UI: Protected resource
~~~

To renew a session, the client posts its refresh token to /api/auth/refresh; the API rotates the stored token and returns a new pair. /api/auth/logout revokes the submitted refresh token. Provider-specific local setup is documented in [DOCKER.md](../DOCKER.md#oauth-local-setup).
