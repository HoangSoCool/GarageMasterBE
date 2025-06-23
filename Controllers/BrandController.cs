using GarageMasterBE.Models;
using GarageMasterBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageMasterBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController : ControllerBase
    {
        private readonly BrandService _brandService;

        public BrandController(BrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var brands = await _brandService.GetAllAsync();
            return Ok(brands);
        }

        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> GetById(string id)
        {
            var brand = await _brandService.GetByIdAsync(id);
            if (brand == null)
                return NotFound();

            return Ok(brand);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchByName([FromQuery] string name)
        {
            var brands = await _brandService.GetByNameAsync(name);
            return Ok(brands);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create([FromBody] Brand brand)
        {
            await _brandService.CreateAsync(brand);
            return CreatedAtAction(nameof(GetById), new { id = brand.Id }, brand);
        }

        [HttpPut("{id:length(24)}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] Brand updatedBrand)
        {
            var existing = await _brandService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            updatedBrand.Id = id;
            var success = await _brandService.UpdateAsync(id, updatedBrand);
            if (!success)
                return BadRequest();

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _brandService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            var success = await _brandService.DeleteAsync(id);
            if (!success)
                return BadRequest();

            return NoContent();
        }
    }
}
