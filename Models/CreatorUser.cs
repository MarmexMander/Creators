using Microsoft.AspNetCore.Identity;

namespace Creators.Models;

[Flags]
enum AccessFlags
{
    //0-4 reserved for other content accesses
    NSFWContent = 1 << 0,
    //5-9 reserved for other moderation accesses
    ModerationVoting = 1 << 5,
    ContentModeration = 1 << 6,
    UsersModeration = 1 << 7,
    //10-14 reserved for other administration accesses
    UserAdministration = 1 << 10,
    ContentAdministration = 1 << 11,
    TechAdministration = 1 << 12,
    //15-24 reserved for other restrictions
    ReadOnly = 1 << 15,
    RestricContentFlagging = 1 << 16,
    //25-30 reserved for user tiers
    UploaderTier1 = 1 << 25
}

class CreatorUser : IdentityUser
{
    public string? PfpId{ get; set; }
    public List<Tag>? BlacklistedTags { get; set; }
    public AccessFlags AccessFlags{ get; set; }
    public List<Publication> Publications{ get; set; }
    public DateTime DateOfBirth{ get; set; }
    public List<Publication> Favorites{ get; set; }
    public List<Publication> VotedUp{ get; set; }
    public List<Publication> VotedDown{ get; set; }
}