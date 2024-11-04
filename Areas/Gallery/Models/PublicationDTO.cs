using  Creators.Models;
public class PublicationDTO
{
    public string Title { get; set; }
    public string? TextContent {get;set;}
    public string? Description { get; set;}
    public IFormFile Media { get; set; }
    public Category Category{ get; set; }
    public IEnumerable<string> Tags {get; set;}    
    public bool IsNSFW { get; set; }

}