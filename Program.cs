using CatalogoApi.Context;
using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get the connection string from the appsettings.json file.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Add the DbContext to the dependency injection container.
builder.Services.AddDbContext<AppDbContext>(options =>
    // Configure the context to use PostgreSQL.
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Create a new Categoria
app.MapPost("/categorias", async (AppDbContext db, Categoria categoria) =>
{
  // Check if the Nome and Descricao are present
  if (string.IsNullOrEmpty(categoria.Nome) || string.IsNullOrEmpty(categoria.Descricao))
    return Results.BadRequest(new { message = "The Nome and Descricao are required." });

  // Check if a Categoria with the same name already exists
  var existingCategoria = await db.Categorias.FirstOrDefaultAsync(c => c.Nome == categoria.Nome);
  if (existingCategoria is not null)
    return Results.Conflict(new { message = "Categoria already exists" });

  // Add the new Categoria to the DbContext
  db.Categorias.Add(categoria);
  // Save the changes to the database
  await db.SaveChangesAsync();
  // Return a 201 Created response and include the new Categoria in the response body
  return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
}).WithTags("Categorias");

// Get all Categorias
app.MapGet("/categorias", async (AppDbContext db) =>
{
  return Results.Ok(await db.Categorias.ToListAsync());
}).WithTags("Categorias");

// Get a Categoria by id
app.MapGet("/categorias/{id:int}", async (AppDbContext db, int id) =>
{
  // Find the category with the specified id
  var categoria = await db.Categorias.FindAsync(id);

  // If it exists, return it
  if (categoria is not null)
    return Results.Ok(categoria);

  // Otherwise return a 404 Not Found
  return Results.NotFound(new { message = "Categoria not found" });
}).WithTags("Categorias");

// Update a Categoria
app.MapPut("/categorias/{id:int}", async (AppDbContext db, int id, Categoria categoria) =>
{
  // Check that the id provided in the URL matches the id of the record being updated
  if (id != categoria.CategoriaId)
    // If the ids do not match, return a 400 Bad Request response
    return Results.BadRequest(new { message = "Categoria id mismatch" });

  // Find the record in the database with the id provided in the URL
  var categoriaDB = await db.Categorias.FindAsync(id);

  // Check that the record was found
  if (categoriaDB is null)
    // If the record was not found, return a 404 Not Found response
    return Results.NotFound(new { message = "Categoria not found" });

  // Update the record from the database with the data from the Categoria object that was passed in the request body if not null, otherwise keep the existing value
  categoriaDB.Nome = categoria.Nome ?? categoriaDB.Nome;
  categoriaDB.Descricao = categoria.Descricao ?? categoriaDB.Descricao;
  // Save the changes to the database
  await db.SaveChangesAsync();

  // Return a 200 Ok response with the Categoria object from the database as the response body
  return Results.Ok(categoriaDB);
}).WithTags("Categorias");

// Delete a Categoria
app.MapDelete("/categorias/{id:int}", async (AppDbContext db, int id) =>
{
  // Find the category
  var categoria = await db.Categorias.FindAsync(id);
  if (categoria is null)
    // If not found, return a 404
    return Results.NotFound(new { message = "Categoria not found" });

  // Remove the category
  db.Categorias.Remove(categoria);
  // Save changes
  await db.SaveChangesAsync();

  // Return a 204 No Content
  return Results.NoContent();
}).WithTags("Categorias");

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
  app.UseDeveloperExceptionPage();
}

app.Run();
