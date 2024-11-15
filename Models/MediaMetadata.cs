using Creators.Models;
public class MediaMetadata
{
    //public string OriginalName { get; set; }
    //public CreatorUser Uploader {get; set;}
    public string Author {get; set;}
    public MediaGroup? Group{get; set;}
    //Is media will be shown by the static link for non logged in users
    public bool IsPublic {get; set;} = true;
    //Is anybody can use this media attachment or only the author
    public bool IsExclusiveToAuthor {get; set;} = false;
}