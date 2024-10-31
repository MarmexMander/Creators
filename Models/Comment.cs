using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Creators.Models;
public class Comment
{
    private readonly ILazyLoader _lazyLoader;
    public Comment(ILazyLoader lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }

    public Comment(){}

    private List<Comment> _children;

    public Guid Id { get; set; }
    public CreatorUser Author { get; set; }
    public Comment? Parent { get; set; }
    public List<Comment> Children {
         get => _lazyLoader.Load(this, ref _children);
         set => _children = value;
    }
    public string Content { get; set; }
    public DateTime CreatedAt { get;set;}
    public Publication Publication { get; set; }
}