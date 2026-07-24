namespace OrderService.Dtos;

public record OrderCreateDto(
    int ProductId,
    int Quantity,
    decimal UnitPrice);
