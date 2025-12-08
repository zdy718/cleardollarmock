namespace BudgetApp.Server
{
    public class Tag
    {

        // Optional parent tag for hierarchical tagging (may be null)
        public int? ParentTagId { get; set; }

        public int TagId { get; set; }

        // Tag text / label
        public string TagName { get; set; } = string.Empty;

        // Budget amount for this tag (use decimal for currency)
        public decimal BudgetAmount { get; set; }
    }
}