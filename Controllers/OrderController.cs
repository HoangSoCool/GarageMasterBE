using GarageMasterBE.Models;
using GarageMasterBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageMasterBE.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class OrderController : ControllerBase
  {
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
      _orderService = orderService;
    }

    // POST: api/Order
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Order order)
    {
      if (order == null || order.Items == null || order.Items.Count == 0)
        return BadRequest("Đơn hàng không hợp lệ.");

      order.CreatedAt = DateTime.UtcNow;
      var created = await _orderService.CreateAsync(order);
      return Ok(created);
    }

    // GET: api/Order
    [HttpGet]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> GetAll()
    {
      var orders = await _orderService.GetAllAsync();
      return Ok(orders);
    }

    // GET: api/Order/user/{userId}
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin,Employee,Customer")]
    public async Task<IActionResult> GetByUser(string userId)
    {
      var orders = await _orderService.GetByUserIdAsync(userId);
      return Ok(orders);
    }
  }
}