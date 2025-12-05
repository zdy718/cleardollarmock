using Microsoft.AspNetCore.Mvc;
using BudgetApp.Server.Accessors;
using BudgetAppCSCE361.Data;

namespace BudgetApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserAccessor _accessor;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserAccessor accessor, ILogger<UsersController> logger)
        {
            _accessor = accessor;
            _logger = logger;
        }

        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<AppUser>> GetUser(Guid userId)
        {
            var user = await _accessor.GetByIdAsync(userId);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<AppUser>> GetUserByEmail(string email)
        {
            var user = await _accessor.GetByEmailAsync(email);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<AppUser>> CreateUser([FromBody] AppUser user)
        {
            try
            {
                var created = await _accessor.CreateAsync(user);
                return CreatedAtAction(nameof(GetUser), new { userId = created.UserId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{userId:guid}")]
        public async Task<ActionResult<AppUser>> UpdateUser(Guid userId, [FromBody] AppUser user)
        {
            if (userId != user.UserId)
                return BadRequest("User ID mismatch");

            try
            {
                var updated = await _accessor.UpdateAsync(user);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpDelete("{userId:guid}")]
        public async Task<ActionResult> DeleteUser(Guid userId)
        {
            var deleted = await _accessor.DeleteAsync(userId);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
    
}