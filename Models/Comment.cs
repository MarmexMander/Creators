namespace Creators.Models;
public class Comment
{
    public Guid Id { get; set; }
    public CreatorUser Author { get; set; }
    public Comment? Parent { get; set; }
    public List<Comment> Children { get; set;} = new List<Comment>();
    public string Content { get; set; }
    public DateTime CreatedAt { get;set;}
    public Publication Publication { get; set; }
}