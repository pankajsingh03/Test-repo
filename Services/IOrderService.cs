using OrderService.Dtos;

namespace OrderService.Services;

public interface IOrderService
{
    IEnumerable<OrderDto> GetOrders();
    OrderDto? GetOrder(int id);
   // OrderDto CreateOrder(OrderCreateDto request);
    Task<OrderPaymentResponseDto> CreateOrderAsync(OrderCreateDto request, CancellationToken cancellationToken = default);
   
}
