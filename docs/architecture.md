# CarRent system architecture

~~~mermaid
flowchart LR
    Browser["Browser<br/>React 19 + TypeScript<br/>Vite / Nginx"]
    API["ASP.NET Core API<br/>Controllers + JWT"]
    App["Application layer<br/>MediatR use cases<br/>Validation + mappings"]
    Domain["Domain layer<br/>Users, cars, bookings,<br/>payments"]
    Infra["Infrastructure layer<br/>EF Core, Identity,<br/>provider adapters"]
    SQL[("SQL Server<br/>Identity + app data")]
    Redis[("Redis<br/>Favorite car IDs")]
    Google["Google Identity"]
    GitHub["GitHub OAuth"]
    PayPal["PayPal Checkout<br/>+ webhooks"]
    Cloudinary["Cloudinary<br/>image storage"]

    Browser <-->|"HTTP / JSON"| API
    API --> App
    App --> Domain
    App --> Infra
    Infra --> SQL
    Infra --> Redis
    Browser <-->|"Google ID token"| Google
    Browser -->|"OAuth authorization code"| GitHub
    Browser <-->|"approval redirect"| PayPal
    API <-->|"token exchange / webhook"| GitHub
    API <-->|"order / webhook verification"| PayPal
    API -->|"verify Google ID token"| Google
    Infra <-->|"upload / URL"| Cloudinary
~~~

The API composes application use cases with infrastructure implementations through dependency injection. EF Core migrations and registered seeders run at startup when Database:ApplyMigrations is enabled; Docker Compose enables this setting.
