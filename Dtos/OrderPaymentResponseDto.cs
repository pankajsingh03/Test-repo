namespace OrderService.Dtos;

public record OrderPaymentResponseDto(
    OrderDto Order,
    PaymentDto Payment);