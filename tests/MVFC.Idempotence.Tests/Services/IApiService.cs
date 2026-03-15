namespace MVFC.Idempotence.Tests.Services;

internal interface IApiService
{
    [Post("/api/orders")]
    public Task<ApiResponse<OrderCreatedResponse>> CreateOrderAsync(
        [HeaderCollection] IDictionary<string, string> headers, 
        [Body] CreateOrderRequest payload);

    [Put("/api/orders/{orderId}")]
    public Task<ApiResponse<OrderUpdatedResponse>> UpdateOrderAsync(
        Guid orderId,
        [Body] UpdateOrderRequest payload);

    [Post("/api/payments")]
    public Task<ApiResponse<PaymentResponse>> CreatePaymentAsync(
        [HeaderCollection] IDictionary<string, string> headers,
        [Body] PaymentRequest payload);

    [Post("/api/invoices")]
    public Task<ApiResponse<InvoiceResponse>> CreateInvoiceAsync(
        [AliasAs("externalId")] string externalId,
        [Body] InvoiceRequest payload);

    [Post("/api/shipping/dispatch")]
    public Task<ApiResponse<ShippingResponse>> DispatchShippingAsync(
        [HeaderCollection] IDictionary<string, string> headers,
        [Body] DispatchRequest payload);

    [Post("/api/shipping/cancel")]
    public Task<ApiResponse<ShippingResponse>> CancelShippingAsync(
        [HeaderCollection] IDictionary<string, string> headers,
        [Body] CancelRequest payload);

    [Delete("/api/idempotency/{key}")]
    public Task<ApiResponse<string>> DeleteIdempotencyAsync(string key);
}
