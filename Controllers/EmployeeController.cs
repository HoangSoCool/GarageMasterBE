using GarageMasterBE.Models;
using GarageMasterBE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GarageMasterBE.Controllers
{
    [ApiController]
    [Route("api/v1/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeService _employeeService;
        private readonly UserService _userService;
        private readonly EmailService _emailService;

        public EmployeesController(EmployeeService employeeService, UserService userService, EmailService emailService)
        {
            _employeeService = employeeService;
            _userService = userService;
            _emailService = emailService;
        }

        // GET: api/v1/employees
        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<Employee>>> GetAll()
        {
            var employees = await _employeeService.GetAllAsync();
            return Ok(employees);
        }

        // GET: api/v1/employees/{id}
        [HttpGet("{id:length(24)}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<Employee>> GetById(string id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null) return NotFound();
            return Ok(employee);
        }

        // POST: api/v1/employees
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Employee>> Create(Employee employee)
        {
            await _employeeService.CreateAsync(employee);
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }

        // PUT: api/v1/employees/{id}
        [HttpPut("{id:length(24)}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, Employee updatedEmployee)
        {
            var existing = await _employeeService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            updatedEmployee.Id = existing.Id;
            updatedEmployee.UserId = existing.UserId; // Không cho phép thay đổi UserId
            var result = await _employeeService.UpdateAsync(id, updatedEmployee);

            if (!result) return BadRequest("Update failed.");
            return NoContent();
        }

        // DELETE: api/v1/employees/{id}
        [HttpDelete("{id:length(24)}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _employeeService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // Xoá employee
            var result = await _employeeService.DeleteAsync(id);

            // Xoá user liên quan (nếu có)
            if (!string.IsNullOrEmpty(existing.UserId))
            {
                await _userService.DeleteByIdAsync(existing.UserId);
            }

            if (!result) return BadRequest("Delete failed.");
            return NoContent();
        }

        // GET: api/v1/employees/search?name=abc
        [HttpGet("search")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<Employee>>> SearchByName([FromQuery] string name)
        {
            var employees = await _employeeService.SearchByNameAsync(name);
            return Ok(employees);
        }

        // POST: api/v1/employees/register
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterEmployeeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userService.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("Email đã tồn tại!");

            // Tạo user mới với role Employee
            var user = await _userService.CreateUserAsync(dto.Email, dto.Password, "Employee");
            if (user == null)
                return StatusCode(500, "Tạo tài khoản không thành công.");

            // Gửi email xác nhận
            if (!string.IsNullOrWhiteSpace(user.EmailConfirmationCode))
            {
                try
                {
                    await _emailService.SendConfirmationEmailAsync(user.Email, user.EmailConfirmationCode);
                }
                catch
                {
                    // Có thể log lỗi gửi email, nhưng vẫn cho phép tạo employee
                }
            }

            // Tạo employee mới
            var employee = new Employee
            {
                UserId = user.Id!,
                Name = dto.Name,
                Phone = dto.Phone,
                Address = dto.Address,
                EmployeeRole = EmployeeRole.Employee,
                DateJoined = DateTime.UtcNow
            };
            await _employeeService.CreateAsync(employee);

            return Ok(new { message = "Đăng ký thành công. Vui lòng kiểm tra email để xác nhận tài khoản.", employee });
        }
    }
}
