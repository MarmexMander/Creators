namespace Creators.Models;

public class TagInfo
{
    public string Id { get; set; }
    public Tag Tag { get; set; }
    public virtual List<string> Aliases{ get; set; }
    //TODO: localizable aliases?
    //TODO: Lazy load. 
    public Category Category{ get; set; }
    public bool IsNSFW{ get; set; }
}