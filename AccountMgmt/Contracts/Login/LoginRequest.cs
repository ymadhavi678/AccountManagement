namespace WebAPI.Contracts.Login;

public sealed record LoginRequest(string UserId, string password);