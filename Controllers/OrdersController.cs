using Microsoft.AspNetCore.Mvc;
using OrderService.Dtos;
using OrderService.Services;
using Microsoft.AspNetCore.Authorization;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<OrderDto>> GetOrders() => Ok(_orderService.GetOrders());

    [HttpGet("{id:int}")]
    public ActionResult<OrderDto> GetOrder(int id)
    {
        var order = _orderService.GetOrder(id);
        return order is not null ? Ok(order) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<OrderPaymentResponseDto>> CreateOrder(OrderCreateDto request, CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOrder), new { id = result.Order.Id }, result);
    }
}