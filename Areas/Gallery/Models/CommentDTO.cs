namespace Creators.Areas.Gallery.Models;

public class CommentDTO
{
    public string Content { get; set; }
    public Guid PublicationId { get; set; }
    public Guid? ParentCommentId { get; set; }
}