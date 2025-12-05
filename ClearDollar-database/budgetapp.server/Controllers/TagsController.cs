using Microsoft.AspNetCore.Mvc;
using BudgetApp.Server.Accessors;
using BudgetAppCSCE361.Data;

namespace BudgetApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ITagAccessor _accessor;
        private readonly ILogger<TagsController> _logger;

        public TagsController(ITagAccessor accessor, ILogger<TagsController> logger)
        {
            _accessor = accessor;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Tag>>> GetAll([FromQuery] Guid userId)
        {
            _logger.LogInformation($"GET tags for userId: {userId}");
            var tags = await _accessor.GetAllAsync(userId);
            return Ok(tags);
        }

        [HttpGet("{tagId:long}")]
        public async Task<ActionResult<Tag>> GetById([FromQuery] Guid userId, long tagId)
        {
            var tag = await _accessor.GetByIdAsync(userId, tagId);
            if (tag == null)
                return NotFound();
            return Ok(tag);
        }

        [HttpGet("level/{level:int}")]
        public async Task<ActionResult<List<Tag>>> GetByLevel([FromQuery] Guid userId, int level)
        {
            try
            {
                var tags = await _accessor.GetByLevelAsync(userId, level);
                return Ok(tags);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("children/{parentId:long}")]
        public async Task<ActionResult<List<Tag>>> GetChildren([FromQuery] Guid userId, long parentId)
        {
            try
            {
                var tags = await _accessor.GetChildrenAsync(userId, parentId);
                return Ok(tags);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<Tag>> Create([FromQuery] Guid userId, [FromBody] Tag tag)
        {
            try
            {
                var created = await _accessor.AddAsync(userId, tag);
                return CreatedAtAction(nameof(GetById), new { userId, tagId = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{tagId:long}")]
        public async Task<ActionResult<Tag>> Update([FromQuery] Guid userId, long tagId, [FromBody] Tag tag)
        {
            if (tagId != tag.Id)
                return BadRequest("Tag ID mismatch");

            try
            {
                var updated = await _accessor.UpdateAsync(userId, tag);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{tagId:long}")]
        public async Task<ActionResult> Delete([FromQuery] Guid userId, long tagId)
        {
            try
            {
                var deleted = await _accessor.DeleteAsync(userId, tagId);
                if (!deleted)
                    return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}