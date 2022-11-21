using CatalagoApi.ApiEndpoints;
using CatalagoApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddSwagger();
builder.AddPersistence();
builder.Services.AddCors();
builder.AddAuthenticationJwt();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapAutenticacaoEndpoints();
app.MapProdutosEndpoints();
app.MapCategoriasEndpoints();

var environment = app.Environment;

// if (environment.IsDevelopment())
app.UseExceptionHandling(environment)
  .UseSwaggerEndpoints()
  .UseAppCors();

app.UseAuthentication();
app.UseAuthorization();

app.Run();