using Microsoft.EntityFrameworkCore;
using Creators.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Creators.Data;
public class CreatorsDbContext: IdentityDbContext<CreatorUser>
{
    //public DbSet<CreatorUser> Users{ get; set; }
    public DbSet<Publication> Publications{ get; set; }
    public DbSet<Comment> Comments{ get; set; }
    public DbSet<Media> Medias{ get; set; }
    public DbSet<Tag> Tags{ get; set; }
    public CreatorsDbContext(DbContextOptions<CreatorsDbContext> options)
    :base(options)
    {
        Database.EnsureCreated();
    }

    //TODO: Move configuration to coresponding separate files
    override protected void OnModelCreating(ModelBuilder modelBuilder){
        //TODO: Rename m2m relations' tables in user configuration
        var users = modelBuilder.Entity<CreatorUser>();
        users.HasOne(u => u.Pfp).WithMany();
        users.HasMany(u => u.BlacklistedTags).WithMany().UsingEntity("UsersBlacklistedTags");
        users.HasMany(u=>u.Publications).WithOne(p=>p.Author);
        users.HasMany(u=>u.Favorites).WithMany().UsingEntity("UsersFavoritePublications");
        users.HasMany(u=>u.VotedUp).WithMany().UsingEntity("UsersVotedUpPublications");
        users.HasMany(u=>u.VotedDown).WithMany().UsingEntity("UsersVotedDownPublications");
        
        var comments = modelBuilder.Entity<Comment>();
        comments.HasOne(c => c.Author).WithMany();
        comments.HasMany(c => c.Children).WithOne(c => c.Parent);

        var publications = modelBuilder.Entity<Publication>();
        publications.HasOne(p => p.Preview).WithMany();
        publications.HasOne(p => p.MediaContent).WithMany();
        publications.HasMany(p => p.Tags).WithMany();
        publications.HasMany(p => p.Comments).WithOne(c => c.Publication);

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

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     string user = System.Environment.GetEnvironmentVariable("POSTGRES_USER");
    //     string pwd = System.Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
    //     string db = System.Environment.GetEnvironmentVariable("POSTGRES_DB");
    //     optionsBuilder
    //     .UseNpgsql($"Server=db;Port=5432;Database={db};User Id={user};Password={pwd};");
    //     base.OnConfiguring(optionsBuilder);
    // }
}