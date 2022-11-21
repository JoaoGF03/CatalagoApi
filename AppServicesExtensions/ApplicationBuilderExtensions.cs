namespace CatalagoApi.Extensions;

public static class ApplicationBuilderExtensions
{
  public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app, IWebHostEnvironment environment)
  {
    if (environment.IsDevelopment())
      app.UseDeveloperExceptionPage();

    return app;
  }

  public static IApplicationBuilder UseAppCors(this IApplicationBuilder app)
  {
    app.UseCors(builder =>
    {
      builder.AllowAnyOrigin();
      builder.AllowAnyMethod();
      builder.AllowAnyHeader();
    });

    return app;
  }

  public static IApplicationBuilder UseSwaggerEndpoints(this IApplicationBuilder app)
  {
    app.UseSwagger();
    app.UseSwaggerUI(c => { });

    return app;
  }
}