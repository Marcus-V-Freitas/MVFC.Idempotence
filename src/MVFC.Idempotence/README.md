# MVFC.Idempotence

A lightweight and efficient idempotency library for ASP.NET Core Minimal APIs, powered by Redis.

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Idempotence/actions/workflows/ci.yml/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Idempotence/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Idempotence/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Idempotence)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Idempotence)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Idempotence)

[Portuguese README](README.pt-br.md)

---

## Overview

`MVFC.Idempotence` ensures that identical HTTP requests are processed only once, preventing unwanted side effects from retries or duplicate submissions. It provides a simple, attribute-like experience for Minimal APIs using endpoint filters.

In distributed systems, idempotency is critical. Whether dealing with payment processing, order creation, or any write operation, the same request should always produce the same result without executing the business logic more than once.

| Package | Service | Downloads |
|---|---|---|
| [MVFC.Idempotence](src/MVFC.Idempotence/README.md) | Idempotency for Minimal APIs | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Idempotence) |

---

## Features

- **Redis-Based**: Reliable storage for idempotency keys and cached responses using StackExchange.Redis.
- **Fluent Configuration**: Easy setup within the ASP.NET Core service builder.
- **Minimal API Integration**: Natively designed for the modern `.MapPost`, `.MapPut`, etc. style.
- **Flexible Key Resolution**: Resolve idempotency keys from headers, route values, or custom logic.
- **Configurable TTL**: Control how long cached results remain in Redis.
- **Group Support**: Apply idempotency to entire route groups at once.

---

## Installation

Install via the .NET CLI:

```bash
dotnet add package MVFC.Idempotence
```

Or via the NuGet Package Manager:

```bash
Install-Package MVFC.Idempotence
```

---

## Configuration

### 1. Register Services

In your `Program.cs`, register the idempotency services and point to your Redis connection string.

```csharp
var builder = WebApplication.CreateBuilder(args);
var redisConnection = builder.Configuration.GetConnectionString("redis")!;

builder.Services.AddIdempotencyRedis(redisConnection, cfg =>
{
    cfg.Ttl = TimeSpan.FromHours(24);       // Default TTL for cached results
    cfg.HeaderName = "X-Idempotency-Key";   // Default header to look for the key
    cfg.AllowedMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH" };
});
```

---

## Usage Examples

Enable idempotency on specific endpoints using the `.WithIdempotency()` extension methods.

### Basic Usage (Header-Based)

By default, the library looks for the key in the configured `HeaderName` (e.g., `X-Idempotency-Key`).

```csharp
app.MapPost("/api/orders", async (CreateOrderRequest req) =>
{
    // Your logic here
    return Results.Created($"/api/orders/{Guid.NewGuid()}", new { req.ProductId });
}).WithIdempotency();
```

### Route-Based Idempotency

If your idempotency key is part of the URL (common for `PUT` operations), specify the route parameter name.

```csharp
app.MapPut("/api/orders/{orderId:guid}", async (Guid orderId, UpdateOrderRequest req) =>
{
    // This endpoint is now idempotent based on the {orderId} route value
    return Results.Ok(new { orderId, Status = "Updated" });
}).WithIdempotency(fromRoute: "orderId");
```

### Custom Key Resolver

For more complex scenarios, provide a custom function to resolve the key from the `HttpContext`.

```csharp
app.MapPost("/api/payments", async (PaymentRequest req) =>
{
    return Results.Accepted();
}).WithIdempotency(ctx =>
{
    // Custom logic to get the key from anywhere in the context
    return ctx.Request.Headers["Custom-Key"].FirstOrDefault();
});
```

### Group-Level Usage

Apply idempotency to an entire group of endpoints. Useful when all endpoints under a prefix should follow the same idempotency rule.

```csharp
var orders = app.MapGroup("/api/orders").WithIdempotency();

orders.MapPost("/", CreateOrder);
orders.MapPut("/{id}", UpdateOrder);
```

### Per-Endpoint TTL Override

Override the TTL or specific options for a single endpoint.

```csharp
app.MapPost("/api/expensive-operation", () => Results.Ok())
   .WithIdempotency(ttl: TimeSpan.FromMinutes(30));
```

---

## Configuration Parameters

| Parameter        | Type           | Description                                                    |
| :--------------- | :------------- | :------------------------------------------------------------- |
| `Ttl`            | `TimeSpan`     | Duration the result remains cached in Redis.                   |
| `HeaderName`     | `string`       | The HTTP header used to identify the idempotency key.          |
| `AllowedMethods` | `ISet<string>` | HTTP methods allowed for idempotency (e.g., POST, PUT, PATCH). |

---

## How It Works

1. An incoming request arrives at an idempotent endpoint.
2. The library extracts the idempotency key (from header, route, or custom resolver).
3. It checks Redis for a cached response using that key.
4. **If a cached response exists**, it is returned immediately — business logic is **not** executed again.
5. **If no cached response exists**, the request proceeds normally; the response is stored in Redis with the configured TTL.

---

## Integrations

`MVFC.Idempotence` is built to work seamlessly with:

- **StackExchange.Redis**: The underlying client for Redis communication.
- **ASP.NET Core Minimal APIs**: Native support for endpoint filters.
- **Distributed Cache**: Uses standard cache abstractions where possible.

---

## Project Structure

- **[src](src)**: Main library source code for `MVFC.Idempotence`.
- **[playground](playground)**: Demo/test environment (sample API) for validating the implementation.
- **[tests](tests)**: Test suite to ensure stability and expected behavior.

---

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a history of changes and releases.

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[Apache-2.0](LICENSE)
