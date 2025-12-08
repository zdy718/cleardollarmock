using System.Collections.Generic;

namespace BudgetApp.Server.Accessors
{
    public interface ITagAccessor
    {
        List<Tag> GetAll(string userId);
        void Add(string userId, Tag tag);
    }
}