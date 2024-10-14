namespace Creators.Models;
class Publication
{
    public string Title { get; set; }
    public Media Preview { get; set; }
    public Media? MediaContent { get; set; }
    public string? TextContent { get; set; }
    public string? Description { get; set; }
    public Categorys Category { get; set; }
    public List<Tag> Tags{ get; set; }
    public bool IsNSFW { get; set; }
    public List<Comment> Comments{ get; set; }
}