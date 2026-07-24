namespace OrderService.Dtos;

public record PaymentDto(
    int PaymentId,
    int OrderId,
    decimal Amount,
    string Currency,
    string Method,
    string Status,
    DateTime ProcessedAt);