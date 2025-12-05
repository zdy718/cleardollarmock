namespace BudgetAppCSCE361.Data;

public class AppUser
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Transaction> Transactions { get; set; } = new();
    public List<Tag> Tags { get; set; } = new();
    public List<Budget> Budgets { get; set; } = new();
}

public class Transaction
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public DateTime PostedAt { get; set; }
    public string Description { get; set; } = "";
    public int AmountCents { get; set; }
    public List<TransactionTag> TransactionTags { get; set; } = new();
}

public class Tag
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public int Level { get; set; }   // 1,2,3
    public string Name { get; set; } = "";
    public long? ParentId { get; set; }
    public Tag? Parent { get; set; }
    public List<Tag> Children { get; set; } = new();
    public List<TransactionTag> TransactionTags { get; set; } = new();
}

public class TransactionTag
{
    public long TransactionId { get; set; }
    public Transaction? Transaction { get; set; }
    public long TagId { get; set; }
    public Tag? Tag { get; set; }
}

public class Budget
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public long TagId { get; set; }
    public Tag? Tag { get; set; }
    public DateTime Month { get; set; }  // first day of month
    public int LimitCents { get; set; }
}
