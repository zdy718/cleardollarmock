using BudgetApp.Server.Data;
using System.Collections.Generic;
using System.Linq;

namespace BudgetApp.Server.Accessors
{
    public class SqlTagAccessor : ITagAccessor
    {
        private readonly BudgetDbContext _context;

        public SqlTagAccessor(BudgetDbContext context)
        {
            _context = context;
        }

        public List<Tag> GetAll(string userId)
        {
            // Filter by the specific UserId provided in the request
            return _context.Tags.Where(t => t.UserId == userId).ToList();
        }

        public void Add(string userId, Tag tag)
        {
            tag.UserId = userId; // Ensure the tag is assigned to the correct user
            _context.Tags.Add(tag);
            _context.SaveChanges();
        }
    }
}