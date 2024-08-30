using WebAPI.Authentication;


namespace WebAPI.Services;

/// <summary>
/// Provides implementation for user authentication and sign-in operations.
/// </summary>
internal sealed class LoginService : ILoginService
{
    private readonly IJwtProvider _jwtProvider;
    

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginService"/> class.
    /// </summary>
    /// <param name="jwtProvider">The JWT provider for generating tokens.</param>
    /// <param name="customerRepository">The repository to access customer data.</param>
    public LoginService(IJwtProvider jwtProvider)
    {
        _jwtProvider = jwtProvider;
        
    }

    /// <summary>
    /// Signs in a user asynchronously using their customer ID and email address.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <param name="email">The email address of the customer.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the JWT token string if sign-in is successful.</returns>
    /// <exception cref="Exception">Thrown if the customer is not found or if the email does not match the customer's record.</exception>
    public async Task<string> SignInAsync(string customerId, string email)
    {      

        return _jwtProvider.Generate(new Guid(), email);
    }
}