namespace OrderService.Dtos;

public record PaymentRequestDto(
    int OrderId,
    decimal Amount,
    string Currency,
    string Method);