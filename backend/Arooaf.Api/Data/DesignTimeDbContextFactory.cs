using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Arooaf.Api.Data;

// permite crear migraciones sin iniciar la aplicacion
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = config.GetConnectionString("Default")
            ?? "Host=TU_HOST_AQUI;Database=TU_DATABASE_AQUI;Username=TU_USUARIO_AQUI;Password=TU_PASSWORD_AQUI;SslMode=Require;";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}