using CatalogoApi.Context;
using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalagoApi.ApiEndpoints;

public static class CategoriasEndpoints
{
  public static void MapCategoriasEndpoints(this WebApplication app)
  {
    app.MapPost("/categorias", async (AppDbContext db, Categoria categoria) =>
    {
      var existingCategoria = await db.Categorias.FirstOrDefaultAsync(c => c.Nome == categoria.Nome);
      if (existingCategoria is not null)
        return Results.Conflict(new { message = "Categoria already exists" });

      db.Categorias.Add(categoria);

      await db.SaveChangesAsync();

      return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
    })
    .Produces<Categoria>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status409Conflict)
    .WithTags("Categorias")
    .RequireAuthorization();

    app.MapGet("/categorias", async (AppDbContext db) =>
    {
      return Results.Ok(await db.Categorias.ToListAsync());
    })
    .Produces<List<Categoria>>(StatusCodes.Status200OK)
    .WithTags("Categorias")
    .RequireAuthorization();

    app.MapGet("/categorias/{id:int}", async (AppDbContext db, int id) =>
    {
      var categoria = await db.Categorias.FindAsync(id);
      if (categoria is not null)
        return Results.Ok(categoria);

      return Results.NotFound(new { message = "Categoria not found" });
    })
    .Produces<Categoria>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithTags("Categorias");

    app.MapGet("/categorias-produtos", async (AppDbContext db) =>
    {
      return Results.Ok(await db.Categorias.Include(categoria => categoria.Produtos).ToListAsync());
    })
    .Produces<List<Categoria>>(StatusCodes.Status200OK)
    .WithTags("Categorias");

    app.MapPut("/categorias/{id:int}", async (AppDbContext db, int id, Categoria categoria) =>
    {
      if (id != categoria.CategoriaId)
        return Results.BadRequest(new { message = "Categoria id mismatch" });

      var categoriaDB = await db.Categorias.FindAsync(id);
      if (categoriaDB is null)
        return Results.NotFound(new { message = "Categoria not found" });

      var existingCategoria = await db.Categorias.FirstOrDefaultAsync(c => c.Nome == categoria.Nome);
      if (existingCategoria is not null && existingCategoria.CategoriaId != categoria.CategoriaId)
        return Results.Conflict(new { message = "Categoria already exists" });

      categoriaDB.Nome = categoria.Nome;
      categoriaDB.Descricao = categoria.Descricao;

      await db.SaveChangesAsync();

      return Results.Ok(categoriaDB);
    })
    .Produces<Categoria>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status409Conflict)
    .WithTags("Categorias");

    app.MapDelete("/categorias/{id:int}", async (AppDbContext db, int id) =>
    {
      var categoria = await db.Categorias.FindAsync(id);
      if (categoria is null)
        return Results.NotFound(new { message = "Categoria not found" });

      db.Categorias.Remove(categoria);

      await db.SaveChangesAsync();

      return Results.NoContent();
    })
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .WithTags("Categorias");
  }
}