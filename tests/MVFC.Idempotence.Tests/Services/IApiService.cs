namespace MVFC.Idempotence.Tests.Services;

internal interface IApiService
{
    [Post("/api/orders")]
    Task<ApiResponse<OrderCreatedResponse>> CreateOrderAsync(
        [HeaderCollection] IDictionary<string, string> headers, 
        [Body] CreateOrderRequest payload);

    [Put("/api/orders/{orderId}")]
    Task<ApiResponse<OrderUpdatedResponse>> UpdateOrderAsync(
        Guid orderId,
        [Body] UpdateOrderRequest payload);

    [Post("/api/payments")]
    Task<ApiResponse<PaymentResponse>> CreatePaymentAsync(
        [HeaderCollection] IDictionary<string, string> headers,
        [Body] PaymentRequest payload);

    [Post("/api/invoices")]
    Task<ApiResponse<InvoiceResponse>> CreateInvoiceAsync(
        [AliasAs("externalId")] string externalId,
        [Body] InvoiceRequest payload);

    [Post("/api/shipping/dispatch")]
    Task<ApiResponse<ShippingResponse>> DispatchShippingAsync(
        [HeaderCollection] IDictionary<string, string> headers,
        [Body] DispatchRequest payload);

    [Post("/api/shipping/cancel")]
    Task<ApiResponse<ShippingResponse>> CancelShippingAsync(
        [HeaderCollection] IDictionary<string, string> headers,
        [Body] CancelRequest payload);

    [Delete("/api/idempotency/{key}")]
    Task<ApiResponse<string>> DeleteIdempotencyAsync(string key);
}