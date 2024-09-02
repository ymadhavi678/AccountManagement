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
        
        string keyBase64 = "MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY="; // Replace with the actual key in Base64
        string ivBase64 = "YWJjZGVmMDEyMzQ1Njc4OQ=="; // Replace with the actual IV in Base64

        public TransactionsController(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferMoney([FromBody] EncryptedDataRequest request)
        {
            // Decrypt the data
            var decryptedData = Decrypt(request.EncryptedData, keyBase64, ivBase64);

            // Process the decrypted data (e.g., parse JSON)
            var formData = decryptedData.Split('-').ToArray<string>();

            

            var result = await _transactionRepository.TransferMoneyAsync(Convert.ToInt64(formData[0].ToString()), Convert.ToInt64(formData[1].ToString()), Convert.ToDecimal(formData[2].ToString()), formData[3]?.ToString());

            if (!result)
            {
                return BadRequest("Transfer failed");
            }

            return Ok("Transfer successful");
        }




        public static string Decrypt(string encryptedDataBase64, string keyBase64, string ivBase64)
        {
            // Convert the Base64 strings to byte arrays
            byte[] encryptedData = Convert.FromBase64String(encryptedDataBase64);
            byte[] key = Convert.FromBase64String(keyBase64);
            byte[] iv = Convert.FromBase64String(ivBase64);

            // Ensure that the key and IV have the correct lengths
            if (key.Length != 32)
            {
                throw new ArgumentException("Invalid key length. Key must be 32 bytes (256 bits).");
            }
            if (iv.Length != 16)
            {
                throw new ArgumentException("Invalid IV length. IV must be 16 bytes (128 bits).");
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None; // We'll handle the padding manually

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    using (MemoryStream ms = new MemoryStream(encryptedData))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            // Decrypt the data
                            byte[] decryptedData = new byte[encryptedData.Length];
                            int decryptedBytesCount = cs.Read(decryptedData, 0, decryptedData.Length);

                            // Remove PKCS7 padding
                            int paddingLength = decryptedData[decryptedBytesCount - 1];
                            byte[] unpaddedData = new byte[decryptedBytesCount - paddingLength];
                            Array.Copy(decryptedData, unpaddedData, unpaddedData.Length);

                            // Convert decrypted byte array to string
                            return Encoding.UTF8.GetString(unpaddedData);
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
