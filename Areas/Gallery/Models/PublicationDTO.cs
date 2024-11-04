using  Creators.Models;
class PublicationDTO
{
    public string Title { get; set; }
    public string? TextContent {get;set;}
    public string? Description { get; set;}
    public Guid MediaID { get; set; }
    public Category Category{ get; set; }
    public List<Tag> Tags {get; set;}    
    public bool IsNSFW { get; set; }

}