using CatalagoApi.Services;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace CatalagoApi.ApiEndpoints;

public static class AutenticacaoEndpoints
{
  public static void MapAutenticacaoEndpoints(this WebApplication app)
  {
    app.MapPost("/login", [AllowAnonymous] (User user, ITokenService tokenService) =>
    {
      if (user is null)
        return Results.BadRequest("Invalid client credentials");

      if (user.Username == "admin" && user.Password == "admin")
      {
        var tokenString = tokenService.GenerateToken(
            app.Configuration["Jwt:Key"],
            app.Configuration["Jwt:Issuer"],
            app.Configuration["Jwt:Issuer"],
            user
          );

        return Results.Ok(new { token = tokenString });
      }
      else
        return Results.Unauthorized();
    })
    .Produces<User>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized)
    .WithName("Login")
    .WithTags("Autenticacao");
  }
}