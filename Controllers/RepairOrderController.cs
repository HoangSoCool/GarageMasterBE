using GarageMasterBE.Models;
using GarageMasterBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageMasterBE.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class RepairOrderController : ControllerBase
  {
    private readonly RepairOrderService _repairOrderService;

    public RepairOrderController(RepairOrderService repairOrderService)
    {
      _repairOrderService = repairOrderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var orders = await _repairOrderService.GetAllAsync();
      return Ok(orders);
    }

    [HttpGet("{id:length(24)}")]
    public async Task<IActionResult> GetById(string id)
    {
      var order = await _repairOrderService.GetByIdAsync(id);
      if (order == null)
        return NotFound();
      return Ok(order);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Create([FromBody] RepairOrder order)
    {
      // Validate các trường bắt buộc
      if (string.IsNullOrEmpty(order.CustomerId) ||
          string.IsNullOrEmpty(order.LicensePlate) ||
          string.IsNullOrEmpty(order.Description) ||
          string.IsNullOrEmpty(order.EmployeeId) || // Bắt buộc có employeeId
          !Enum.IsDefined(typeof(RepairOrderStatus), order.Status))
      {
        return BadRequest("Thiếu hoặc sai định dạng trường dữ liệu!");
      }

      if (order.CreatedAt == default)
        order.CreatedAt = DateTime.UtcNow;

      await _repairOrderService.CreateAsync(order);
      return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:length(24)}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Update(string id, [FromBody] RepairOrder updatedOrder)
    {
      var existing = await _repairOrderService.GetByIdAsync(id);
      if (existing == null)
        return NotFound();

      updatedOrder.Id = id;
      var success = await _repairOrderService.UpdateAsync(id, updatedOrder);
      if (!success)
        return BadRequest();

      return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Delete(string id)
    {
      var existing = await _repairOrderService.GetByIdAsync(id);
      if (existing == null)
        return NotFound();

      var success = await _repairOrderService.DeleteAsync(id);
      if (!success)
        return BadRequest();

      return NoContent();
    }
  }
}