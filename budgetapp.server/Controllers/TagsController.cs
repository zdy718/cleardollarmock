using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using BudgetApp.Server.Accessors;

namespace BudgetApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ILogger<TagsController> _logger;
        private readonly ITagAccessor _accessor;

        public TagsController(ILogger<TagsController> logger, ITagAccessor accessor)
        {
            _logger = logger;
            _accessor = accessor;
        }

        [HttpGet]
        public IEnumerable<Tag> Get(string userId)
        {
            Console.WriteLine($"GET tags for userId: {userId}");
            return _accessor.GetAll(userId);
        }
    }
}