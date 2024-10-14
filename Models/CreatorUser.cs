using Microsoft.AspNetCore.Identity;

namespace Creators.Models;

class CreatorUser : IdentityUser
{
    string? PfpId{ get; set; }
    List<Tag>? BlacklistedTags { get; set; }
}