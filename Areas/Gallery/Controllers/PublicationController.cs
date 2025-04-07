using Microsoft.AspNetCore.Mvc;
using Creators.Models;
using Creators.Areas.Gallery.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Xml.Schema;
using Microsoft.AspNetCore.Authorization;
using Creators.Services;

namespace Creators.Areas.Gallery.Controllers;
[Area("Gallery")]
public class PublicationController : Controller
{
    private readonly Data.CreatorsDbContext _dbContext;
    private readonly UserManager<CreatorUser> _userManager;
    private readonly IMediaFileManager _mediaManager;
    public PublicationController(
        Data.CreatorsDbContext dbContext,
        UserManager<CreatorUser> userManager,
        IMediaFileManager mediaManager
    )
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _mediaManager = mediaManager;
    }

    public IActionResult Test()
    {
        return View();
    }
    
    public async Task<IActionResult> Show(Guid id)
    {
        var publication = await _dbContext.Publications
        .Include(p=>p.MediaContent)
        .Include(p=>p.Comments)
        .Include(p=>p.Author)
        .FirstAsync(p=>p.Id == id);
        return View(publication);
    }

    [HttpGet, Authorize]
    public IActionResult Add()
    {
        return View();
    }


    [HttpPost, Authorize]
    public async Task<ActionResult<Guid>> Add(PublicationDTO model)
    {    
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid publication data.");
        }
        var tags = await _dbContext.Tags.AsNoTracking().Where(t=>model.Tags.Contains(t.Name)).ToListAsync();
        var invalidTags = tags.Where(t => !(t.Categories?.Contains(model.Category) ?? true)); //If tag categories is null, it handled as universal
        //TODO: Generate warning for user
        if (invalidTags?.Any() == true)
             tags.RemoveAll(t => invalidTags.Contains(t));
        //Assemble the entity
        Publication publication = new(){
            Title = model.Title,
            Author = await _userManager.GetUserAsync(User),
            MediaContentId = model.Media,
            Tags = tags,
            TextContent = model.TextContent,
            Description = model.Description,
            Category = model.Category,
            IsNSFW = model.IsNSFW,
            CreatedAt = DateTime.UtcNow
        };

        //Write new entity to the DB
        var x = await _dbContext.Publications.AddAsync(publication);
        _dbContext.Entry(publication.Category).State = EntityState.Modified;
        publication.Tags.ForEach(
            t => _dbContext.Entry(t).State = EntityState.Modified
            );

        await _dbContext.SaveChangesAsync();
        return publication.Id;
    }
    
    

    [HttpPost("comment"), Authorize]
    public async Task<IActionResult> PostComment(CommentDTO model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid comment data.");
        }

        var publication = await _dbContext.Publications.FirstAsync(p=>p.Id == model.PublicationId);

        if (publication == null)
        {
            return NotFound("Publication not found.");
        }

        var comment = new Comment
        {
            Content = model.Content,
            CreatedAt = DateTime.UtcNow,
            Author = await _userManager.GetUserAsync(User), 
            Publication = publication,
            Parent = model.ParentCommentId.HasValue 
                        ? await _dbContext.Comments.FirstAsync(c=> c.Id == model.ParentCommentId.Value)
                        : null
        };

        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction("Show", new { id = model.PublicationId });
    }

    [HttpGet]
    public async Task<IActionResult> Feed(){
        //TODO: Filter by user and guest blacklists
        var publications = await _dbContext.Publications
        .Include(p=>p.MediaContent) //TODO: Maybe change to use of MediaContentId to not load all the metadata
        .OrderByDescending(p=>p.CreatedAt).Take(35).ToListAsync(); 

        return View("Feed", publications);
    }
    

}