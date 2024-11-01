namespace Creators.Models;
public class Publication
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public CreatorUser Author { get; set; }
    public Media Preview { get; set; }
    public Media? MediaContent { get; set; }
    public string? TextContent { get; set; }
    public string? Description { get; set; }
    public Category Category { get; set; }
    public List<Tag> Tags{ get; set; }
    public bool IsNSFW { get; set; }
    public List<Comment> Comments{ get; set; }
    public int Score { get; set; }
    public int FavCount { get; set; }
}