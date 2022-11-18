using CatalogoApi.Models;

namespace CatalagoApi.Services;

public interface ITokenService
{
  string GenerateToken(string key, string issuer, string audience, User user);
}