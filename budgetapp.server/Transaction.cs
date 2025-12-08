namespace BudgetApp.Server
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        // Date of the transaction (uses DateOnly like other models in this project)
        public DateOnly Date { get; set; }

        // Monetary amount for the transaction
        public decimal Amount { get; set; }

        // Merchant details / description
        public string MerchantDetails { get; set; } = string.Empty;

        // Optional tag associated with the transaction (may be null)
        public int? TagId { get; set; }

    }
}