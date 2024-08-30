using System.Threading.Tasks;

namespace TransactionAPI.Repositories
{
    public interface ITransactionRepository
    {
        Task<bool> TransferMoneyAsync(long fromAccountId, long toAccountId, decimal amount);
    }
}
