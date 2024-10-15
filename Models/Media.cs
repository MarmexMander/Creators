namespace Creators.Models;
class Media
{
    public Guid Guid {get; set;}
    public string OriginalName {get; set;}
    public string MimeType{get; set;}
    public CreatorUser Author {get; set;}
    public DateTime UploadedAt {get; set;}
    public int LinksCount {get; set;}
    public bool IsNSFW {get; set;}
}