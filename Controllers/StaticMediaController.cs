using Creators.Data;
using Creators.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Creators.Services;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("static")]
public class StaticMediaController : Controller
{
    private readonly CreatorsDbContext _db;
    private readonly UserManager<CreatorUser> _userManager;
    private readonly IMediaFileManager _mediaFileManager;
    

    public StaticMediaController(CreatorsDbContext db, UserManager<CreatorUser> userManager, IMediaFileManager mediaFileManager){
        _db = db;
        _userManager = userManager;
        _mediaFileManager = mediaFileManager;
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> Media(Guid id){
        var media = await _db.Medias.FirstAsync(m=>m.Guid == id);
        if(media == null) return NotFound();

        var mediaResult = _mediaFileManager.GetMediaWebStream(media);
        if(mediaResult == null) return NotFound();
        
        if(media.IsPublic)
            return mediaResult;            
        var user = await _userManager.GetUserAsync(User);

        if(user == null || !user.AccessFlags.HasFlag(AccessFlags.NSFWContent))
            return Forbid();

        return mediaResult;
    }
}