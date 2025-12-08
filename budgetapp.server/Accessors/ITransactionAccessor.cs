using System.Collections.Generic;

namespace BudgetApp.Server.Accessors
{
    public interface ITransactionAccessor
    {
        List<Transaction> GetAll(string userId);
        void Add(string userId, Transaction transaction);
    }
}