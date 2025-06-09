using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using FraudDetection.Database;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        // Pobierz environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        string connectionString;

        if (environment == "Development")
        {
            connectionString = "Host=145.239.80.20;Database=ClaimGuardDBdev;Username=postgres;Password=salooniksql";
        }
        else if (environment == "Production")
        {
            connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                ?? throw new InvalidOperationException("DB_CONNECTION_STRING environment variable is not set.");
        }
        else
        {
            throw new InvalidOperationException("Invalid environment specified. Use 'Development' or 'Production'.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new DatabaseContext(optionsBuilder.Options);
    }
}
