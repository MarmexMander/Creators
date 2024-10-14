namespace Creators.Models;
class Comment
{
    public CreatorUser Author { get; set; }
    public Comment? Parent { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get;set;}
}