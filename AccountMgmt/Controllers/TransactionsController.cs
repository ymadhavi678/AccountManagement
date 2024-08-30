using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Repositories;

namespace TransactionAPI.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;
        // Replace these values with your actual key and IV
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef"); // 32 bytes for AES-256
        private static readonly byte[] Iv = Encoding.UTF8.GetBytes("abcdef0123456789");
        // 16 bytes for AES

        public TransactionsController(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferMoney([FromBody] EncryptedDataRequest request)
        {
            // Decrypt the data
            var decryptedData = DecryptStringFromBytes_Aes(Convert.FromBase64String(request.EncryptedData), Key, Iv);

            // Process the decrypted data (e.g., parse JSON)
            var formData = Newtonsoft.Json.JsonConvert.DeserializeObject<TransferRequest>(decryptedData);

            // Log or process the form data
            Console.WriteLine($"Recipient Account: {formData.RecipientAccount}");
            Console.WriteLine($"Amount: {formData.TransferAmount}");
            Console.WriteLine($"Description: {formData.TransferDescription}");

            var result = await _transactionRepository.TransferMoneyAsync(formData.FromAccountId, formData.RecipientAccount, formData.TransferAmount);

            if (!result)
            {
                return BadRequest("Transfer failed");
            }

            return Ok("Transfer successful");
        }



        // Method to decrypt string using AES
        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        try
                        {
                            return sr.ReadToEnd();
                        }
                        catch (Exception e)
                        {
                            return "";
                        }

                    }
                }
            }
        }
    }

    public class TransferRequest
    {
        public long FromAccountId { get; set; }
        public long RecipientAccount { get; set; }
        public decimal TransferAmount { get; set; }

        public string TransferDescription { get; set; }
    }


    public class EncryptedDataRequest
    {
        public string EncryptedData { get; set; }
    }
}
