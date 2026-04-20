using Back;
using Back.Data;
 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseStaticFiles();
app.UseCors("NextJs");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("fixed");

// API Documentation
 


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Project API V1");
        options.SwaggerEndpoint("/openapi/v2.json", "Project API V2");
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
        options.EnableFilter();
    });
}

// Seeder
using (var scope = app.Services.CreateScope())
{
    await Seeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
