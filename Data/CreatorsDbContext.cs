using Microsoft.EntityFrameworkCore;
using Creators.Models;

namespace Creators.Data;
class CreatorsDbContext:DbContext
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


        base.OnModelCreating(modelBuilder);
    }
}