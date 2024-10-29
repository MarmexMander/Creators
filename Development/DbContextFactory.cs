using Creators.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Creators.Development;

public class DbContextFactory : IDesignTimeDbContextFactory<CreatorsDbContext>
{
    public CreatorsDbContext CreateDbContext(string[] args)
    {
        if(args.Length != 3)
            throw new ArgumentException("Pass user, password and db name to assemble connection string");

        var optionsBuilder = new DbContextOptionsBuilder<CreatorsDbContext>();
        string user = System.Environment.GetEnvironmentVariable("POSTGRES_USER");
        string pwd = System.Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        string db = System.Environment.GetEnvironmentVariable("POSTGRES_DB");
        var conStr = $"Server=localhost;User Id={args[0]};Password={args[1]};Database={args[2]}";
        optionsBuilder.UseNpgsql(conStr);
        return new CreatorsDbContext(optionsBuilder.Options);
    }
}