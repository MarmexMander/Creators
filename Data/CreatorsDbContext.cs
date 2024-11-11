using Microsoft.EntityFrameworkCore;
using Creators.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Creators.Extentions;

namespace Creators.Data;
public class CreatorsDbContext: IdentityDbContext<CreatorUser>
{
    //public DbSet<CreatorUser> Users{ get; set; }
    public DbSet<Publication> Publications{ get; set; }
    public DbSet<Comment> Comments{ get; set; }
    public DbSet<Media> Medias{ get; set; }
    public DbSet<Tag> Tags{ get; set; }
    public DbSet<Category> Categories{ get; set; }
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
        users.Navigation(u=>u.Pfp).AutoInclude();
        users.Property(u=>u.UploadTier)
        .HasConversion( ut => (UploadTierEnum)ut, ute => (UploadTier)ute)
        .HasDefaultValue((UploadTier)UploadTierEnum.Tier1);
        
        var comments = modelBuilder.Entity<Comment>();
        comments.HasOne(c => c.Author).WithMany();
        comments.HasMany(c => c.Children).WithOne(c => c.Parent);
        comments.Navigation(c=>c.Author).AutoInclude();
        //comments.Navigation(c=>c.Parent).AutoInclude();
        comments.Navigation(c=>c.Publication).AutoInclude();

        var publications = modelBuilder.Entity<Publication>();
        //publications.HasOne(p => p.Preview).WithMany();
        publications.HasOne(p => p.MediaContent).WithMany();
        publications.HasMany(p => p.Tags).WithMany();
        publications.HasMany(p => p.Comments).WithOne(c => c.Publication);
        //publications.Navigation(p=>p.Preview).AutoInclude();

        var tags = modelBuilder.Entity<Tag>();
        tags.HasOne(t=>t.AliasedTo).WithMany();
        tags.HasMany(t => t.Categories).WithMany();

        modelBuilder.Entity<Category>().SeedEnumValues<Category, CategoryEnum>(ce=>ce);
        modelBuilder.Entity<UploadTier>().SeedEnumValues<UploadTier, UploadTierEnum>(ce=>ce);

        var media = modelBuilder.Entity<Media>();
        media.HasOne(m=>m.Group).WithMany(g=>g.Medias).HasForeignKey("GroupId");
        media.HasOne(m => m.Uploader).WithMany();

        //modelBuilder.Entity<Category>().

        base.OnModelCreating(modelBuilder);
    }


    //Couses concurency exception for some reaason. For now using extention method on model creation

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // => optionsBuilder
    //    .EnableSensitiveDataLogging()
    //    .UseSeeding((context, _) 
    //        => {
    //            (context as CreatorsDbContext).Categories.SeedEnumValues<Category, CategoryEnum>((categoryEnum) => categoryEnum);
    //            context.SaveChanges();
    //        })
    //    .UseAsyncSeeding(async(context, _, ct) 
    //        => {
    //            (context as CreatorsDbContext).Categories.SeedEnumValues<Category, CategoryEnum>((categoryEnum) => categoryEnum);
    //            await context.SaveChangesAsync(ct);
    //        });

}