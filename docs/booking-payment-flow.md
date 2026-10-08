# CarRent booking and PayPal payment flow

~~~mermaid
sequenceDiagram
    autonumber
    actor Customer
    participant UI as React client
    participant API as CarRent API
    participant App as Booking/payment handlers
    participant DB as SQL Server
    participant PayPal
    participant Worker as Booking expiration service

    Customer->>UI: Choose car and rental dates
    UI->>API: POST /api/bookings (JWT)
    API->>App: Validate request and availability
    App->>DB: Create pending booking
    API-->>UI: Booking ID and details

    UI->>API: POST /api/payments/bookings/{id}/pay (JWT)
    API->>App: Create PayPal order
    App->>DB: Create pending payment record
    App->>PayPal: Create order
    PayPal-->>API: Order approval URL
    API-->>UI: Approval URL
    UI->>PayPal: Redirect customer to approve
    PayPal-->>UI: Return to success or cancel URL

    PayPal->>API: POST /api/payments/webhook
    API->>PayPal: Verify webhook signature
    PayPal-->>API: Verification result
    API->>App: Process verified payment event
    App->>DB: Update payment and booking status

    loop After returning from PayPal
        UI->>API: GET /api/payments/bookings/{id}/status
        API->>DB: Read booking and payment status
        API-->>UI: Current status
    end

    Worker->>DB: Expire eligible pending bookings
~~~

Payment completion is confirmed by processing PayPal webhook events; the browser return route alone is not confirmation. The API exposes a status endpoint for the frontend to refresh the result after returning from PayPal.
