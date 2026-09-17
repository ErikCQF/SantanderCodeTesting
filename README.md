# Hacker News Best Stories API

## Overview

This API retrieves the best stories from the Hacker News API and returns the top *N* stories ordered by score in descending order.

---

## Running the Application

### Prerequisites

- .NET 8 SDK

### Configuration

The application is configured through `appsettings.json`:

```json
{
  "HackerServiceSettings": {
    "MaxConcurrentCalls": 10
  },
  "HttpClientSettings": {
    "BaseUrl": "https://hacker-news.firebaseio.com/v0/",
    "MaxHttpCalls": 50
  }
}
```

| Setting | Description |
|----------|-------------|
| `MaxConcurrentCalls` | Maximum number of story detail requests that can be processed concurrently. This helps prevent excessive traffic to the Hacker News API. |
| `BaseUrl` | Base URL of the Hacker News API. |
| `MaxHttpCalls` | Maximum number of simultaneous HTTP connections allowed to the Hacker News API. |

### Start the API

```bash
dotnet restore
dotnet build
dotnet run
```

### Swagger

When running in Development mode, Swagger UI is available for testing the endpoint.

---

## Usage

Example request:

```http
GET /api/stories/best?n=10
```

Example response:

```json
[
  {
    "title": "...",
    "uri": "...",
    "postedBy": "...",
    "time": "...",
    "score": 123,
    "commentCount": 45
  }
]
```

---

## Assumptions

- The Hacker News API is available and reachable.
- Story details are retrieved directly from the Hacker News API.
- Consumers expect the results to be returned in descending order of score.
- Temporary failures (timeouts, rate limiting, transient errors) may occur and should be handled automatically.

---

## Scalability and Resilience

To efficiently handle large numbers of requests without overloading the Hacker News API, the solution implements:

- **Configurable concurrency limits** using `MaxConcurrentCalls`.
- **SemaphoreSlim throttling** to limit outbound requests.
- **HttpClientFactory** and connection pooling for efficient HTTP connection management.
- **Connection limits** using `MaxHttpCalls`.
- **Polly Retry Policy** with exponential backoff and jitter for transient failures (`5xx`, `408`, and `429` responses).
- **Polly Circuit Breaker Policy** to temporarily stop requests when the external service is experiencing repeated failures.

These measures help ensure reliability while protecting the Hacker News API from excessive traffic.

---

## Future Enhancements

Given additional time, the following improvements could be made:
- Unit Tests.
- In-memory or distributed caching to reduce calls to the Hacker News API.
- Integration and load testing.
- OpenTelemetry metrics and monitoring.
- Docker containerisation and CI/CD pipeline support.
- Health checks and operational dashboards.
