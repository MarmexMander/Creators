namespace Creators.Models;
class Comment
{
    public Guid Id { get; set; }
    public CreatorUser Author { get; set; }
    public Comment? Parent { get; set; }
    public List<Comment> Children { get; set;}
    public string Content { get; set; }
    public DateTime CreatedAt { get;set;}
}