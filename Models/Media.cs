using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Creators.Models;

//TODO: Add lazy loading of rarely used props
public class Media
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid {get; set;}
    public string OriginalName {get; set;}
    public string MimeType{get; set;}
    public CreatorUser Uploader {get; set;}
    public string Author {get; set;}
    public DateTime UploadedAt {get; set;}
    public MediaGroup? Group{get; set;}
    public int LinksCount {get; set;} = 0;
    //Is media will be shown by the static link for non logged in users
    public bool IsPublic {get; set;} = true;
    //Is anybody can use this media attachment or only the author
    public bool IsExclusiveToAuthor {get; set;} = false;
}