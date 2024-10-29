using Creators.Data;
using Creators.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("static")]
public class StaticMediaController : Controller
{

    private readonly IConfiguration _configuration;
    private readonly string _mediaLocation;
    private readonly CreatorsDbContext _db;
    private readonly UserManager<CreatorUser> _userManager;
    

    public StaticMediaController(CreatorsDbContext db, UserManager<CreatorUser> userManager, IConfiguration configuration){
        _db = db;
        _userManager = userManager;
        _configuration = configuration;
        _mediaLocation = _configuration.GetValue<string>("LocalMediaLocation");
    }

    private FileStreamResult? GetLocalMedia(Media media){
        if (media == null) return null;
        var fileExtention = Path.GetExtension(media.OriginalName);
        var filePath = Path.Combine(_mediaLocation, $"{media.Guid}{fileExtention}"); //TODO: Maybe separate file extention and name in DB or save local media without extentions
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, media.MimeType);
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> Media(Guid id){
        var media = await _db.Medias.FindAsync(id);
        if(media == null) return NotFound();

        var mediaResult = GetLocalMedia(media);
        if(mediaResult == null) return NotFound();
        
        if(media.IsPublic)
            return mediaResult;            
        var user = await _userManager.GetUserAsync(User);

        if(user == null || !user.AccessFlags.HasFlag(AccessFlags.NSFWContent))
            return Forbid();

        return mediaResult;
    }
}