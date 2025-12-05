using Microsoft.AspNetCore.Mvc;
using BudgetApp.Server.Accessors;
using BudgetAppCSCE361.Data;

namespace BudgetApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionAccessor _accessor;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionAccessor accessor, ILogger<TransactionsController> logger)
        {
            _accessor = accessor;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Transaction>>> GetAll([FromQuery] Guid userId)
        {
            _logger.LogInformation($"GET transactions for userId: {userId}");
            var transactions = await _accessor.GetAllAsync(userId);
            return Ok(transactions);
        }

        [HttpGet("{transactionId:long}")]
        public async Task<ActionResult<Transaction>> GetById([FromQuery] Guid userId, long transactionId)
        {
            var transaction = await _accessor.GetByIdAsync(userId, transactionId);
            if (transaction == null)
                return NotFound();
            return Ok(transaction);
        }

        [HttpGet("daterange")]
        public async Task<ActionResult<List<Transaction>>> GetByDateRange(
            [FromQuery] Guid userId,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            var transactions = await _accessor.GetByDateRangeAsync(userId, start, end);
            return Ok(transactions);
        }

        [HttpGet("tag/{tagId:long}")]
        public async Task<ActionResult<List<Transaction>>> GetByTag([FromQuery] Guid userId, long tagId)
        {
            var transactions = await _accessor.GetByTagAsync(userId, tagId);
            return Ok(transactions);
        }

        [HttpGet("untagged")]
        public async Task<ActionResult<List<Transaction>>> GetUntagged([FromQuery] Guid userId)
        {
            var transactions = await _accessor.GetUntaggedAsync(userId);
            return Ok(transactions);
        }

        [HttpPost]
        public async Task<ActionResult<Transaction>> Create([FromQuery] Guid userId, [FromBody] Transaction transaction)
        {
            try
            {
                var created = await _accessor.AddAsync(userId, transaction);
                return CreatedAtAction(nameof(GetById), new { userId, transactionId = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<List<Transaction>>> CreateBulk(
            [FromQuery] Guid userId,
            [FromBody] List<Transaction> transactions)
        {
            try
            {
                var created = await _accessor.AddBulkAsync(userId, transactions);
                return Ok(created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{transactionId:long}")]
        public async Task<ActionResult<Transaction>> Update(
            [FromQuery] Guid userId,
            long transactionId,
            [FromBody] Transaction transaction)
        {
            if (transactionId != transaction.Id)
                return BadRequest("Transaction ID mismatch");

            try
            {
                var updated = await _accessor.UpdateAsync(userId, transaction);
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

        [HttpDelete("{transactionId:long}")]
        public async Task<ActionResult> Delete([FromQuery] Guid userId, long transactionId)
        {
            var deleted = await _accessor.DeleteAsync(userId, transactionId);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}