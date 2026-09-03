using MemoLingo.Application.Services;
using MemoLingo.Infrastructure;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Front", policy =>
        policy.WithOrigins("https://localhost:7009", "http://localhost:5151")
              .AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "MemoLingo API";
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// O NSwag sobe o host in-process no ambiente "NSwagGenerator" durante o build,
// portanto qualquer inicialização pesada deve ser pulada nesse ambiente.
if (!app.Environment.IsEnvironment("NSwagGenerator"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseCors("Front");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
