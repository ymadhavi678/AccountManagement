using Microsoft.EntityFrameworkCore;
using WebAPI.Data;

namespace TransactionAPI.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> TransferMoneyAsync(long fromAccountId, long toAccountId, decimal amount)
        {
            var sql = "EXEC TransferMoney @p0, @p1, @p2";
            var parameters = new object[] { fromAccountId, toAccountId, amount };

            try
            {
                await _context.Database.ExecuteSqlRawAsync(sql, parameters);
                return true;
            }
            catch (Exception)
            {
                // Log exception
                return false;
            }
        }
    }
}
