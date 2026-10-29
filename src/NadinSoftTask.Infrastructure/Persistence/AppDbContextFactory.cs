using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace NadinSoftTask.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(ResolveConfigurationDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' is not configured.");

        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();

        optionsBuilder.UseSqlServer(
            connectionString,
            sqlServer => sqlServer.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string ResolveConfigurationDirectory()
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        string[] candidates =
        [
            currentDirectory,
            Path.Combine(currentDirectory, "src", "NadinSoftTask.Api"),
            Path.Combine(currentDirectory, "..", "NadinSoftTask.Api"),
            Path.Combine(currentDirectory, "..", "..", "src", "NadinSoftTask.Api")
        ];

        return candidates
            .Select(Path.GetFullPath)
            .FirstOrDefault(candidate => File.Exists(Path.Combine(candidate, "appsettings.json")))
            ?? currentDirectory;
    }
}
