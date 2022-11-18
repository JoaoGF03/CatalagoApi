using CatalogoApi.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get the connection string from the appsettings.json file
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// This code creates a database context with the given connection string.
// The database context is used to interact with the database.
// The database context is injected into the application using dependency injection.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
  app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");


app.Run();
