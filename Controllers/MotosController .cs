using GarageMasterBE.Models;
using GarageMasterBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageMasterBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MotosController : ControllerBase
    {
        private readonly MotoService _motoService;

        public MotosController(MotoService motoService)
        {
            _motoService = motoService;
        }

        // ✅ GET: api/Motos
        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<Moto>>> GetAll()
        {
            var motos = await _motoService.GetAllAsync();
            return Ok(motos);
        }

        // ✅ GET: api/Motos/{licensePlate}
        [HttpGet("{licensePlate}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<Moto>> GetByLicensePlate(string licensePlate)
        {
            var moto = await _motoService.GetByLicensePlateAsync(licensePlate);
            if (moto == null)
                return NotFound(new { message = "Xe không tồn tại" });

            return Ok(moto);
        }

        // ✅ GET: api/Motos/by-customer/{customerId}
        [HttpGet("by-customer/{customerId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<Moto>>> GetByCustomerId(string customerId)
        {
            var motos = await _motoService.GetByCustomerIdAsync(customerId);
            return Ok(motos);
        }

        // ✅ POST: api/Motos
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create([FromBody] Moto moto)
        {
            await _motoService.CreateAsync(moto);
            return CreatedAtAction(nameof(GetByLicensePlate), new { licensePlate = moto.LicensePlate }, moto);
        }

        // ✅ PUT: api/Motos/{licensePlate}
        [HttpPut("{licensePlate}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(string licensePlate, Moto updatedMoto)
        {
            var existing = await _motoService.GetByLicensePlateAsync(licensePlate);
            if (existing == null)
                return NotFound(new { message = "Xe không tồn tại" });

            updatedMoto.LicensePlate = licensePlate; // Giữ nguyên khóa chính
            var success = await _motoService.UpdateAsync(licensePlate, updatedMoto);
            return success ? NoContent() : StatusCode(500, new { message = "Cập nhật thất bại" });
        }

        // ✅ DELETE: api/Motos/{licensePlate}
        [HttpDelete("{licensePlate}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(string licensePlate)
        {
            var existing = await _motoService.GetByLicensePlateAsync(licensePlate);
            if (existing == null)
                return NotFound(new { message = "Xe không tồn tại" });

            var success = await _motoService.DeleteAsync(licensePlate);
            return success ? NoContent() : StatusCode(500, new { message = "Xóa thất bại" });
        }
    }
}
