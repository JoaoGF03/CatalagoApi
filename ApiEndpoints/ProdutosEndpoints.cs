using CatalogoApi.Context;
using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalagoApi.ApiEndpoints;

public static class ProdutosEndpoints
{
  public static void MapProdutosEndpoints(this WebApplication app)
  {
    app.MapPost("/produtos", async (AppDbContext db, Produto produto) =>
    {
      var categoria = await db.Categorias.FindAsync(produto.CategoriaId);
      if (categoria is null)
        return Results.NotFound(new { message = "Categoria not found" });

      var existingProduto = await db.Produtos.FirstOrDefaultAsync(p => p.Nome == produto.Nome);
      if (existingProduto is not null)
        return Results.Conflict(new { message = "Produto already exists" });

      db.Produtos.Add(produto);

      await db.SaveChangesAsync();

      return Results.Created($"/produtos/{produto.ProdutoId}", produto);
    })
    .Produces<Produto>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status409Conflict)
    .WithTags("Produtos");

    app.MapGet("/produtos", async (AppDbContext db) =>
    {
      return Results.Ok(await db.Produtos.ToListAsync());
    })
    .Produces<List<Produto>>(StatusCodes.Status200OK)
    .WithTags("Produtos");

    app.MapGet("/produtos-por-pagina", async (AppDbContext db, int? numeroPagina, int? tamanhoPagina) =>
    {
      numeroPagina = numeroPagina <= 0 ? 1 : numeroPagina ?? 1;
      tamanhoPagina = tamanhoPagina <= 0 ? 10 : tamanhoPagina ?? 10;

      var produtos = await db.Produtos
        .Skip((numeroPagina.Value - 1) * tamanhoPagina.Value)
        .Take(tamanhoPagina.Value)
        .ToListAsync();

      return Results.Ok(produtos);
    })
    .Produces<List<Produto>>(StatusCodes.Status200OK)
    .WithTags("Produtos");

    app.MapGet("/produtos/{id:int}", async (AppDbContext db, int id) =>
    {
      var produto = await db.Produtos.FindAsync(id);
      if (produto is not null)
        return Results.Ok(produto);

      return Results.NotFound(new { message = "Produto not found" });
    })
    .Produces<Produto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithTags("Produtos");

    app.MapGet("/produtos/nome/{filter}", async (AppDbContext db, string filter) =>
    {
      var produtos = await db.Produtos
        .Where(produto => produto.Nome.ToLower().Contains(filter.ToLower()))
        .ToListAsync();

      return produtos.Count() > 0
        ? Results.Ok(produtos)
        : Results.NotFound(Array.Empty<Produto>());
    })
    .Produces<List<Produto>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithTags("Produtos");

    app.MapPut("/produtos/{id:int}", async (AppDbContext db, int id, Produto produto) =>
    {
      if (id != produto.ProdutoId)
        return Results.BadRequest(new { message = "Produto id mismatch" });

      var produtoDB = await db.Produtos.FindAsync(id);
      if (produtoDB is null)
        return Results.NotFound(new { message = "Produto not found" });

      var existingProduto = await db.Produtos.FirstOrDefaultAsync(p => p.Nome == produto.Nome);
      if (existingProduto is not null && existingProduto.ProdutoId != produto.ProdutoId)
        return Results.Conflict(new { message = "Produto already exists" });

      produtoDB.Nome = produto.Nome;
      produtoDB.Descricao = produto.Descricao;
      produtoDB.Preco = produto.Preco;
      produtoDB.Imagem = produto.Imagem;
      produtoDB.Estoque = produto.Estoque;
      produtoDB.DataCompra = produto.DataCompra;
      produtoDB.CategoriaId = produto.CategoriaId;

      await db.SaveChangesAsync();

      return Results.Ok(produtoDB);
    })
    .Produces<Produto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status409Conflict)
    .WithTags("Produtos");

    app.MapDelete("/produtos/{id:int}", async (AppDbContext db, int id) =>
    {
      var produto = await db.Produtos.FindAsync(id);
      if (produto is null)
        return Results.NotFound(new { message = "Produto not found" });

      db.Produtos.Remove(produto);

      await db.SaveChangesAsync();

      return Results.NoContent();
    })
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .WithTags("Produtos");
  }
};