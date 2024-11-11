using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Creators.Models;
public class Publication
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [MaxLength(100)]
    public string Title { get; set; }
    public CreatorUser Author { get; set; }
    public Media? MediaContent { get; set; }
    public string? TextContent { get; set; }
    public string? Description { get; set; }
    public Category Category { get; set; }
    public List<Tag> Tags{ get; set; }
    public bool IsNSFW { get; set; } = false;
    public List<Comment> Comments{ get; set; }
    public int Score { get; set; } = 0;
    public int FavCount { get; set; } = 0;
}