using System.ComponentModel.DataAnnotations;

namespace TransactionAPI.Models
{
    public class Account
    {
        [Key]
        public long Id { get; set; }

        public decimal Balance { get; set; }
    }
}
