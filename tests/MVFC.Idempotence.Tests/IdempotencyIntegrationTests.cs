namespace MVFC.Idempotence.Tests;

[Collection("Aspire")]
public sealed class IdempotencyIntegrationTests(AspireFixture fixture) : IClassFixture<AspireFixture>
{
    private readonly IApiService _api = fixture.Api;

    [Fact]
    public async Task Orders_SameKey_ReturnsCached201()
    {
        // Arrange
        var key = MockEntities.Global();
        var payload1 = new CreateOrderRequest(ProductId: "PROD-001", Quantity: 3);
        var payload2 = new CreateOrderRequest(ProductId: "PROD-999", Quantity: 99);

        // Act
        var res1 = await _api.CreateOrderAsync(key, payload1);
        var res2 = await _api.CreateOrderAsync(key, payload2);

        // Assert
        res1.StatusCode.Should().Be(HttpStatusCode.Created);
        res2.StatusCode.Should().Be(HttpStatusCode.Created);
        res1.Content!.OrderId.Should().Be(res2.Content!.OrderId);
    }

    [Fact]
    public async Task Orders_DifferentKeys_ReturnDifferentOrders()
    {
        // Arrange
        var key1 = MockEntities.Global();
        var key2 = MockEntities.Global();
        var payload = new CreateOrderRequest(ProductId: "PROD-001", Quantity: 1);

        // Act
        var res1 = await _api.CreateOrderAsync(key1, payload);
        var res2 = await _api.CreateOrderAsync(key2, payload);

        // Assert
        res1.Content!.OrderId.Should().NotBe(res2.Content!.OrderId);
    }

    [Fact]
    public async Task Orders_MissingKey_ReturnsBadRequest()
    {
        // Arrange
        var key = MockEntities.Empty();
        var payload = new CreateOrderRequest(ProductId: "PROD-001", Quantity: 1);

        // Act
        var res = await _api.CreateOrderAsync(key, payload);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Orders_WrongHeader_ReturnsBadRequest()
    {
        // Arrange
        var key = MockEntities.Payment();
        var payload = new CreateOrderRequest(ProductId: "PROD-001", Quantity: 1);

        // Act
        var res = await _api.CreateOrderAsync(key, payload);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Orders_Put_SameRouteId_ReturnsCached()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var payload1 = new UpdateOrderRequest(Status: "confirmed");
        var payload2 = new UpdateOrderRequest(Status: "cancelled");

        // Act
        var res1 = await _api.UpdateOrderAsync(orderId, payload1);
        var res2 = await _api.UpdateOrderAsync(orderId, payload2);

        // Assert
        res1.StatusCode.Should().Be(HttpStatusCode.OK);
        res2.StatusCode.Should().Be(HttpStatusCode.OK);
        res1.Content!.Status.Should().Be("confirmed");
        res1.Content!.Status.Should().Be("confirmed");
        res1.Content!.UpdatedAt.Should().Be(res2.Content!.UpdatedAt);
    }

    [Fact]
    public async Task Orders_Put_DifferentRouteIds_ExecuteSeparately()
    {
        // Arrange
        var orderId1 = Guid.NewGuid();
        var orderId2 = Guid.NewGuid();
        var payload1 = new UpdateOrderRequest(Status: "confirmed");
        var payload2 = new UpdateOrderRequest(Status: "confirmed");

        // Act
        var res1 = await _api.UpdateOrderAsync(orderId1, payload1);
        var res2 = await _api.UpdateOrderAsync(orderId2, payload2);

        // Assert
        res1.Content!.UpdatedAt.Should().NotBe(res2.Content!.UpdatedAt);
    }

    [Fact]
    public async Task Payments_SameKey_ReturnsCachedTransaction()
    {
        // Arrange
        var key = MockEntities.Payment();
        var payload = new PaymentRequest(OrderId: Guid.NewGuid(), Amount: 150.00m);

        // Act
        var res1 = await _api.CreatePaymentAsync(key, payload);
        var res2 = await _api.CreatePaymentAsync(key, payload);

        // Assert
        res1.Content!.TransactionId.Should().Be(res2.Content!.TransactionId);
    }

    [Fact]
    public async Task Payments_GlobalHeader_ReturnsBadRequest()
    {
        // Arrange
        var key = MockEntities.Global();
        var payload = new PaymentRequest(OrderId: Guid.NewGuid(), Amount: 100m);

        // Act
        var res = await _api.CreatePaymentAsync(key, payload);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Payments_InvalidAmount_ReturnsUnprocessable_NotCached()
    {
        // Arrange
        var key = MockEntities.Payment();
        var payload = new PaymentRequest(OrderId: Guid.NewGuid(), Amount: -10m);

        // Act
        var res1 = await _api.CreatePaymentAsync(key, payload);
        var res2 = await _api.CreatePaymentAsync(key, payload);

        // Assert
        res1.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        res2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Invoices_SameExternalId_ReturnsCached()
    {
        // Arrange
        var externalId = $"EXT-{Guid.NewGuid()}";
        var payload1 = new InvoiceRequest(ExternalId: externalId, Value: 500m);
        var payload2 = new InvoiceRequest(ExternalId: externalId, Value: 999m);

        // Act
        var res1 = await _api.CreateInvoiceAsync(externalId, payload1);
        var res2 = await _api.CreateInvoiceAsync(externalId, payload2);

        // Assert
        res1.StatusCode.Should().Be(HttpStatusCode.Created);
        res2.StatusCode.Should().Be(HttpStatusCode.Created);
        res1.Content!.CreatedAt.Should().Be(res2.Content!.CreatedAt);
        res1.Content!.Value.Should().Be(500m);
    }

    [Fact]
    public async Task Invoices_MissingQueryParam_ReturnsBadRequest()
    {
        // Arrange
        string? key = null;
        var payload = new InvoiceRequest(ExternalId: "EXT-001", Value: 100m);

        // Act
        var res = await _api.CreateInvoiceAsync(key!, payload);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Shipping_Dispatch_SameKey_ReturnsCached()
    {
        // Arrange
        var key = MockEntities.Global();
        var payload = new DispatchRequest(OrderId: Guid.NewGuid());

        // Act
        var res1 = await _api.DispatchShippingAsync(key, payload);
        var res2 = await _api.DispatchShippingAsync(key, payload);

        // Assert
        res1.Content!.TrackingId.Should().Be(res2.Content!.TrackingId);
        res2.Content!.Status.Should().Be("dispatched");
    }

    [Fact]
    public async Task Shipping_Cancel_SameKey_ReturnsCached()
    {
        // Arrange
        var key = MockEntities.Global();
        var payload = new CancelRequest(OrderId: Guid.NewGuid());

        // Act
        var res1 = await _api.CancelShippingAsync(key, payload);
        var res2 = await _api.CancelShippingAsync(key, payload);

        // Assert
        res1.Content!.TrackingId.Should().Be(res2.Content!.TrackingId);
    }

    [Fact]
    public async Task Delete_Key_AllowsReexecution()
    {
        // Arrange
        var value = MockEntities.NewKey();
        var key = MockEntities.Global(value);
        var payload = new CreateOrderRequest(ProductId: "PROD-001", Quantity: 1);

        // Act
        var body1 = await _api.CreateOrderAsync(key, payload);
        var del = await _api.DeleteIdempotencyAsync(value);
        var body2 = await _api.CreateOrderAsync(key, payload);

        // Assert
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);
        body1.Content!.OrderId.Should().NotBe(body2.Content!.OrderId);
    }
}