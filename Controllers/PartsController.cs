using GarageMasterBE.Models;
using GarageMasterBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageMasterBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartsController : ControllerBase
    {
        private readonly PartsService _partsService;

        public PartsController(PartsService partsService)
        {
            _partsService = partsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var parts = await _partsService.GetAllAsync();
            return Ok(parts);
        }

        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> GetById(string id)
        {
            var part = await _partsService.GetByIdAsync(id);
            if (part == null)
                return NotFound();

            return Ok(part);
        }

        [HttpGet("searchByName")]
        public async Task<IActionResult> SearchByName([FromQuery] string name)
        {
            var parts = await _partsService.GetByNameAsync(name);
            return Ok(parts);
        }

        [HttpGet("byBrand/{brandId:length(24)}")]
        public async Task<IActionResult> GetByBrandId(string brandId)
        {
            var parts = await _partsService.GetByBrandIdAsync(brandId);
            return Ok(parts);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Parts part)
        {
            try
            {
                await _partsService.CreateAsync(part);
                return CreatedAtAction(nameof(GetById), new { id = part.Id }, part);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:length(24)}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] Parts updatedPart)
        {
            try
            {
                var existing = await _partsService.GetByIdAsync(id);
                if (existing == null)
                    return NotFound();

                updatedPart.Id = id;
                var success = await _partsService.UpdateAsync(id, updatedPart);
                if (!success)
                    return BadRequest();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:length(24)}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _partsService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            var success = await _partsService.DeleteAsync(id);
            if (!success)
                return BadRequest();

            return NoContent();
        }

        [HttpPut("{id:length(24)}/quantity")]
        public async Task<IActionResult> UpdateQuantity(string id, [FromBody] QuantityUpdateModel model)
        {
            var part = await _partsService.GetByIdAsync(id);
            if (part == null)
                return NotFound();

            part.Quantity += model.QuantityChange;
            if (part.Quantity < 0) part.Quantity = 0;

            var success = await _partsService.UpdateAsync(id, part);
            if (!success)
                return BadRequest(new { message = "Cập nhật số lượng thất bại" });

            return Ok(part);
        }
    }
}
