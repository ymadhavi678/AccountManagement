using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using WebAPI.Contracts.Login;
using WebAPI.Services;

namespace WebAPI.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly ILoginService _loginService;

    // The same encryption key used in the frontend for encryption
    private readonly string _encryptionKey = "AAECAwQFBgcICQoLDA0ODw==";

    public AuthenticationController(ILoginService loginService) =>
        _loginService = loginService;

    [HttpPost("login")]

    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        //Decrypt the username and password
       var decryptedUsername = Decrypt(request.UserId);
       var decryptedPassword = Decrypt(request.password);

        var token = await _loginService.SignInAsync(decryptedUsername, decryptedPassword);
        return Ok(new LoginResponse { Token = token });
    }


    [HttpPost("test")]

    public async Task<IActionResult> Test([FromBody] LoginRequest request)
    {
        //Decrypt the username and password
        //var decryptedUsername = Decrypt(request.UserId);
        //var decryptedPassword = Decrypt(request.password);

        //var token = await _loginService.SignInAsync("decryptedUsername", "decryptedPassword");
        var token = await _loginService.SignInAsync("decryptedUsername", "decryptedPassword");
        return Ok(new LoginResponse { Token = token });
    }

    private string Decrypt(string cipherText)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes aesAlg = Aes.Create())
        {
            var key = Encoding.UTF8.GetBytes(_encryptionKey);

            using (var decryptor = aesAlg.CreateDecryptor(key, aesAlg.IV))
            {
                using (var msDecrypt = new MemoryStream(cipherBytes))
                {
                    aesAlg.IV = ReadByteArray(msDecrypt);

                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    private static byte[] ReadByteArray(Stream s)
    {
        byte[] rawLength = new byte[sizeof(int)];
        if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
        {
            throw new SystemException("Stream did not contain properly formatted byte array");
        }

        byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
        if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
        {
            throw new SystemException("Did not read byte array properly");
        }

        return buffer;
    }

}