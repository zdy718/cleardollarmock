using Microsoft.AspNetCore.Mvc;
using BudgetApp.Server.Accessors;
using BudgetAppCSCE361.Data;

namespace BudgetApp.Server.Controllers
{

[ApiController]
    [Route("api/[controller]")]
    public class BudgetsController : ControllerBase
    {
        private readonly IBudgetAccessor _accessor;
        private readonly ILogger<BudgetsController> _logger;

        public BudgetsController(IBudgetAccessor accessor, ILogger<BudgetsController> logger)
        {
            _accessor = accessor;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Budget>>> GetAll([FromQuery] Guid userId)
        {
            var budgets = await _accessor.GetAllAsync(userId);
            return Ok(budgets);
        }

        [HttpGet("{budgetId:long}")]
        public async Task<ActionResult<Budget>> GetById([FromQuery] Guid userId, long budgetId)
        {
            var budget = await _accessor.GetByIdAsync(userId, budgetId);
            if (budget == null)
                return NotFound();
            return Ok(budget);
        }

        [HttpPost]
        public async Task<ActionResult<Budget>> Create([FromQuery] Guid userId, [FromBody] Budget budget)
        {
            try
            {
                var created = await _accessor.AddAsync(userId, budget);
                return CreatedAtAction(nameof(GetById), new { userId, budgetId = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{budgetId:long}")]
        public async Task<ActionResult<Budget>> Update(
            [FromQuery] Guid userId,
            long budgetId,
            [FromBody] Budget budget)
        {
            if (budgetId != budget.Id)
                return BadRequest("Budget ID mismatch");

            try
            {
                var updated = await _accessor.UpdateAsync(userId, budget);
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

        [HttpDelete("{budgetId:long}")]
        public async Task<ActionResult> Delete([FromQuery] Guid userId, long budgetId)
        {
            var deleted = await _accessor.DeleteAsync(userId, budgetId);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }

}