using Creators.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Creators.Development;

public class DbContextFactory : IDesignTimeDbContextFactory<CreatorsDbContext>
{
    public CreatorsDbContext CreateDbContext(string[] args)
    {
        string user = System.Environment.GetEnvironmentVariable("POSTGRES_USER");
        string pwd = System.Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        string db = System.Environment.GetEnvironmentVariable("POSTGRES_DB");
        var optionsBuilder = new DbContextOptionsBuilder<CreatorsDbContext>();
        optionsBuilder.UseNpgsql($"Server=localhost;Port=5432;Database={db};User Id={user};Password={pwd}");
        
        return new CreatorsDbContext(optionsBuilder.Options);
    }
}