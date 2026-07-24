namespace OrderService.Dtos;

public record OrderDto(
    int Id,
    int ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal Total,
    string Status,
    DateTime CreatedAt);
