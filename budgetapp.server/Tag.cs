using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Server
{
    public class Tag
    {
        [Key] // Marks this as the Primary Key
        public int TagId { get; set; }
        public string UserId { get; set; } = string.Empty; // New field
        public int? ParentTagId { get; set; }
        public string TagName { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
    }
}