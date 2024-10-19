using Microsoft.EntityFrameworkCore;
using Creators.Models;

namespace Creators.Data;
public class CreatorsDbContext:DbContext
{
    public DbSet<CreatorUser> Users{ get; set; }
    public DbSet<Publication> Publications{ get; set; }
    public DbSet<Comment> Comments{ get; set; }
    public DbSet<Media> Medias{ get; set; }
    public DbSet<Tag> Tags{ get; set; }
    public CreatorsDbContext(DbContextOptions<CreatorsDbContext> options)
    :base(options)
    {
        Database.EnsureCreated();
    }

    override protected void OnModelCreating(ModelBuilder modelBuilder){
        //TODO: Rename m2m relations' tables in user configuration
        var users = modelBuilder.Entity<CreatorUser>();
        users.HasOne(u => u.Pfp).WithMany();
        users.HasMany(u => u.BlacklistedTags).WithMany();
        users.HasMany(u=>u.Publications).WithOne(p=>p.Author);
        users.HasMany(u=>u.Favorites).WithMany();
        users.HasMany(u=>u.VotedUp).WithMany();
        users.HasMany(u=>u.VotedDown).WithMany();
        
        var comments = modelBuilder.Entity<Comment>();
        comments.HasOne(c => c.Author).WithMany();
        comments.HasMany(c => c.Children).WithOne(c => c.Parent);

        var publications = modelBuilder.Entity<Publication>();
        publications.HasOne(p => p.Preview).WithMany();
        publications.HasOne(p => p.MediaContent).WithMany();
        publications.HasMany(p => p.Tags).WithMany();
        publications.HasMany(p => p.Comments).WithOne();

        //Not sure is right config to Tag.Name -> TagInfo.Id
        modelBuilder.Entity<Tag>()
        .HasOne(t=>t.Info)
        .WithOne(ti => ti.Tag)
        .HasForeignKey<TagInfo>(ti => ti.Id)
        .IsRequired();

        var media = modelBuilder.Entity<Media>();
        media.HasOne(m=>m.Group).WithMany(g=>g.Medias).HasForeignKey("GroupId");
        media.HasOne(m => m.Author).WithMany();

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string user = System.Environment.GetEnvironmentVariable("POSTGRES_USER");
        string pwd = System.Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        optionsBuilder
        .UseNpgsql($"Server=db;Port=5432;Database=creators_db;User Id={user};Password={pwd};");//TODO: Obfuscate better
        base.OnConfiguring(optionsBuilder);
    }
}