using Microsoft.AspNetCore.Identity;

namespace Creators.Models;

[Flags]
public enum AccessFlags
{
    //0-4 reserved for content accesses
    NSFWContent = 1 << 0,
    //5-9 reserved for moderation accesses
    ModerationVoting = 1 << 5,
    ContentModeration = 1 << 6,
    UsersModeration = 1 << 7,
    //10-14 reserved for administration accesses
    UserAdministration = 1 << 10,
    ContentAdministration = 1 << 11,
    TechAdministration = 1 << 12,
    //15-24 reserved for restrictions
    ReadOnly = 1 << 15,
    RestricContentFlagging = 1 << 16,
    //25-30 reserved for user tiers [depricated] (we moved to the MediaLimiterService.cs)
    // UploaderTier1 = 1 << 25,
    // UploaderTier2 = 1 << 26,
    // UploaderTier3 = 1 << 27,
    // UploaderTier4 = 1 << 28,
    // UploaderTier5 = 1 << 29,
}

public class CreatorUser : IdentityUser //TODO: Add default values
{
    public Media? Pfp{ get; set; }
    public List<Tag>? BlacklistedTags { get; set; }
    public AccessFlags AccessFlags{ get; set; }
    public List<Publication> Publications{ get; set; }
    public DateTime DateOfBirth{ get; set; }
    public List<Publication> Favorites{ get; set; }
    public List<Publication> VotedUp{ get; set; }
    public List<Publication> VotedDown{ get; set; }
    public List<IdentityRole<int>> Roles{ get; set; }
    public UploadTier UploadTier { get; set; }
}