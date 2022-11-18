using CatalogoApi.Context;
using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

#region CRUD Categoria 
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
.WithTags("Categorias");

app.MapGet("/categorias", async (AppDbContext db) =>
{
  return Results.Ok(await db.Categorias.ToListAsync());
})
.Produces<List<Categoria>>(StatusCodes.Status200OK)
.WithTags("Categorias");

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
#endregion

#region CRUD Produto
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
#endregion

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
  app.UseDeveloperExceptionPage();

app.Run();
