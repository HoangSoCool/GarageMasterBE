using GarageMasterBE.Models;
using GarageMasterBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageMasterBE.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class RepairDetailController : ControllerBase
  {
    private readonly RepairDetailService _repairDetailService;

    public RepairDetailController(RepairDetailService repairDetailService)
    {
      _repairDetailService = repairDetailService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var details = await _repairDetailService.GetAllAsync();
      return Ok(details);
    }

    [HttpGet("{id:length(24)}")]
    public async Task<IActionResult> GetById(string id)
    {
      var detail = await _repairDetailService.GetByIdAsync(id);
      if (detail == null)
        return NotFound();
      return Ok(detail);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Create([FromBody] RepairDetail detail)
    {
      await _repairDetailService.CreateAsync(detail);
      return CreatedAtAction(nameof(GetById), new { id = detail.Id }, detail);
    }

    [HttpPut("{id:length(24)}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Update(string id, [FromBody] RepairDetail updatedDetail)
    {
      var existing = await _repairDetailService.GetByIdAsync(id);
      if (existing == null)
        return NotFound();

      updatedDetail.Id = id;
      var success = await _repairDetailService.UpdateAsync(id, updatedDetail);
      if (!success)
        return BadRequest();

      return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Delete(string id)
    {
      var existing = await _repairDetailService.GetByIdAsync(id);
      if (existing == null)
        return NotFound();

      var success = await _repairDetailService.DeleteAsync(id);
      if (!success)
        return BadRequest();

      return NoContent();
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrderId(string orderId)
    {
      var details = await _repairDetailService.GetByOrderIdAsync(orderId);
      if (details == null) return NotFound();
      return Ok(details);
    }
  }
}