using GarageMasterBE.Models;
using GarageMasterBE.Services;
using Microsoft.AspNetCore.Mvc;

namespace GarageMasterBE.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ReviewsController : ControllerBase
  {
    private readonly ReviewService _reviewService;

    public ReviewsController(ReviewService reviewService)
    {
      _reviewService = reviewService;
    }

    // GET: api/Reviews
    [HttpGet]
    public async Task<ActionResult<List<Review>>> GetAll()
    {
      var reviews = await _reviewService.GetAllAsync();
      return Ok(reviews);
    }

    // POST: api/Reviews
    [HttpPost]
    public async Task<IActionResult> CreateOrUpdate([FromBody] Review review)
    {
      if (string.IsNullOrEmpty(review.UserId) || string.IsNullOrEmpty(review.Email))
        return BadRequest("Thiếu thông tin user.");

      await _reviewService.CreateOrUpdateAsync(review);
      return Ok(new { message = "Đánh giá đã được lưu." });
    }
  }
}