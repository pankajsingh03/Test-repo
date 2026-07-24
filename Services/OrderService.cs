using OrderService.Dtos;

namespace OrderService.Services;

public class OrderService : IOrderService
{
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly List<OrderDto> _orders = new()
    {
        new(1, 1001, 2, 24.50m, 49.00m, "Created", DateTime.UtcNow.AddHours(-4)),
        new(2, 1002, 1, 99.99m, 99.99m, "Processing", DateTime.UtcNow.AddHours(-2)),
        new(3, 1003, 3, 12.00m, 36.00m, "Completed", DateTime.UtcNow.AddHours(-1))
    };

    private int _nextId = 4;

    public OrderService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient;
    }

    public IEnumerable<OrderDto> GetOrders() => _orders;

    public OrderDto? GetOrder(int id) => _orders.FirstOrDefault(order => order.Id == id);

    public async Task<OrderPaymentResponseDto> CreateOrderAsync(OrderCreateDto request, CancellationToken cancellationToken = default)
    {
        var order = new OrderDto(
            _nextId++,
            request.ProductId,
            request.Quantity,
            request.UnitPrice,
            request.Quantity * request.UnitPrice,
            "Created",
            DateTime.UtcNow);

        _orders.Add(order);

        var paymentRequest = new PaymentRequestDto(
            order.Id,
            order.Total,
            "USD",
            "CreditCard");

        var payment = await _paymentServiceClient.ProcessPaymentAsync(paymentRequest, cancellationToken);

        return new OrderPaymentResponseDto(order, payment);
    }
}