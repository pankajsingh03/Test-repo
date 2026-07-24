using System.Net.Http.Json;
using OrderService.Dtos;

namespace OrderService.Services;

public class PaymentServiceClient : IPaymentServiceClient
{
    private readonly HttpClient _httpClient;

    public PaymentServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaymentDto> ProcessPaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/payments/process-payment", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payment = await response.Content.ReadFromJsonAsync<PaymentDto>(cancellationToken: cancellationToken);
        return payment ?? throw new InvalidOperationException("PaymentService returned no payload.");
    }
}