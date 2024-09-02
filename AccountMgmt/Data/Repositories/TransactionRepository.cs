using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
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

        public async Task<bool> TransferMoneyAsync(long fromAccountId, long toAccountId, decimal amount, string desc = "NA")
        {
            // Validate input parameters
            if (fromAccountId <= 0 || toAccountId <= 0)
                throw new InvalidOperationException("Account IDs must be positive.");

            if (fromAccountId == toAccountId)
                throw new InvalidOperationException("Cannot transfer money to the same account.");

            if (amount <= 0)
                throw new InvalidOperationException("Transfer amount must be positive.");

            // Define the SQL query using named parameters
            var sql = "EXEC TransferMoney @FromAccountId, @ToAccountId, @Amount, @Description";

            // Create SQL parameters
            var parameters = new[]
            {
        new SqlParameter("@FromAccountId", SqlDbType.BigInt) { Value = fromAccountId },
        new SqlParameter("@ToAccountId", SqlDbType.BigInt) { Value = toAccountId },
        new SqlParameter("@Amount", SqlDbType.Decimal) { Value = amount },
        new SqlParameter("@Description", SqlDbType.NVarChar, 255) { Value = desc }
    };

            
                try
                {
                    // Execute the stored procedure with named parameters
                    await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                    
                    return true;
                }
                catch (Exception ex)
                {
                   
                

                    return false;
                }
            }
        }

    }

