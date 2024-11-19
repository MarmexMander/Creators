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
        
        //FIXME: finish here

        //Assemble the entity
        Publication publication = new(){
            Title = model.Title,
            Author = await _userManager.GetUserAsync(User),
            //FIXME: finish here
        };

        //Write new entity to the DB
        var x = await _dbContext.Publications.AddAsync(publication);
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
    
}