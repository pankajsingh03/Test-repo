using OrderService.Dtos;

namespace OrderService.Services;

public interface IPaymentServiceClient
{
    Task<PaymentDto> ProcessPaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken = default);
}