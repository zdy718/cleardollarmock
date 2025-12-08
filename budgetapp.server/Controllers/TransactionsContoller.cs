using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetApp.Server.Accessors;

namespace BudgetApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ILogger<TransactionsController> _logger;
        private readonly ITransactionAccessor _accessor;

        public TransactionsController(ILogger<TransactionsController> logger, ITransactionAccessor accessor)
        {
            _logger = logger;
            _accessor = accessor;
        }

        [HttpGet]
        public IEnumerable<Transaction> Get(string userId)
        {
            Console.WriteLine($"GET transactions for userId: {userId}");
            return _accessor.GetAll(userId);
        }
    }
}