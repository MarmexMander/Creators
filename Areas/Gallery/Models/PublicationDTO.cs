using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using  Creators.Models;
public class PublicationDTO
{
    [DisplayName("Publicaaation Title")]
    public string Title { get; set; }
    [DisplayName("Write your work here")]
    public string? TextContent {get;set;}
    [DisplayName("Description")]
    public string? Description { get; set;}
    [DisplayName("Upload your work here")]
    public Guid Media { get; set; }
    [DisplayName("Category")]
    public CategoryEnum Category{ get; set; }
    [DisplayName("Tags")]
    public IEnumerable<string> Tags {get; set;}    
    public bool IsNSFW { get; set; }

}