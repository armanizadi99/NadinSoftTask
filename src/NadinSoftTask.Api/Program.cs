using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NadinSoftTask.Api;
using NadinSoftTask.Application;
using NadinSoftTask.Application.Common.Security;
using NadinSoftTask.Infrastructure;
using NadinSoftTask.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddJwtOptions(builder.Configuration);
builder.Services.AddApi();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
    AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();

app.UseApiDocumentation();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
