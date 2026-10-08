# CarRent data model

The diagram shows application entities and key relationships. Standard ASP.NET Core Identity role and claims tables are omitted for readability.

~~~mermaid
erDiagram
    USER ||--o{ BOOKING : creates
    USER ||--o{ REFRESH_TOKEN : owns
    CAR ||--o{ BOOKING : reserved_for
    BOOKING ||--o| PAYMENT : has

    USER {
        guid Id PK
        string Email
        string FullName
        string DriverLicenseNumber
        string LicenseStatus
        string DriverLicenseImageUrl
    }

    CAR {
        guid Id PK
        string Name
        int Year
        decimal DailyPrice
        string BodyType
        string EngineType
        string DriveType
        string TransmissionType
        int HorsePower
        string PhotoUrl
        int SeatsCount
        int TrunkVolumeLiters
    }

    BOOKING {
        guid Id PK
        guid UserId FK
        guid CarId FK
        datetime StartDate
        datetime EndDate
        string Status
        decimal TotalPrice
        datetime CreatedAt
    }

    PAYMENT {
        guid Id PK
        guid BookingId FK
        string TransactionId
        decimal Amount
        string Status
        datetime PaidAt
    }

    REFRESH_TOKEN {
        guid Id PK
        string Token
        guid UserId FK
        datetime ExpiresOnUtc
        bool IsRevoked
        datetime CreatedOnUtc
        datetime RevokedOnUtc
        string ReplacedByToken
    }
~~~

Favorites are stored separately in Redis and are not relational entities. A unique index on Payment.BookingId allows at most one payment record per booking.
