using System;
using System.ComponentModel.DataAnnotations;

namespace TransactionAPI.Models
{
    public class Transaction
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage ="Source Account number is required")]
        [StringLength(16, MinimumLength =8, ErrorMessage = "Source Account nnumber must be between 8 to 16")]
        public long FromAccountId { get; set; }

        [Required(ErrorMessage = "Target Account number is required")]
        [StringLength(16, MinimumLength = 8, ErrorMessage = "Target Account nnumber must be between 8 to 16")]
        public long ToAccountId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
