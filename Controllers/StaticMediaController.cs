using Creators.Data;
using Creators.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Creators.Services;
using Microsoft.EntityFrameworkCore;
using FFMpegCore;
using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Principal;


[ApiController]
[Route("static")]
public class StaticMediaController : Controller
{
    private class MediaData{
        public Media Metadata { get; set; }
        public MemoryStream Media { get; set; }
        public Stream? ConvertedMedia { get; set; }
        public MediaLimiterService.MediaLimitsData? Limits {get;set;}
    }

    private readonly CreatorsDbContext _db;
    private readonly UserManager<CreatorUser> _userManager;
    private readonly IMediaFileManager _mediaFileManager;
    private readonly MediaLimiterService _mediaLimiter;
    private readonly ILogger<StaticMediaController> _logger;
    private readonly SemaphoreSlim //TODO: use configuration for semaphore threshold
        _hugeMediaSemaphore = new(5),
        _mediaSemaphore = new(100); 
    private static readonly ConcurrentDictionary<string, MediaData> _mediaCache = new();

    private string? _userId;
    private string? UserId {
        get{
            if (_userId == null)
                _userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return _userId;
        }
    }
    

    public StaticMediaController(CreatorsDbContext db, UserManager<CreatorUser> userManager, IMediaFileManager mediaFileManager, MediaLimiterService mediaLimiter, ILogger<StaticMediaController> logger){
        _db = db;
        _userManager = userManager;
        _mediaFileManager = mediaFileManager;
        _mediaLimiter = mediaLimiter;
        _logger = logger;
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> Media(Guid id){
        var media = await _db.Medias.FirstAsync(m=>m.Guid == id);
        if(media == null) return NotFound();

        var mediaStream = _mediaFileManager.GetMediaStream(media);
        FileStreamResult mediaResult = new(mediaStream, media.MimeType);
        if(mediaStream == null) return NotFound();
        
        if(media.IsPublic)
            return mediaResult;            
        var user = await _userManager.GetUserAsync(User);
        if(user == null || !user.AccessFlags.HasFlag(AccessFlags.NSFWContent))
            return Forbid();

        return mediaResult;
    }

    private async Task<MediaLimiterService.MediaLimitsData?> AnalyseCurrentMedia(){
        //TODO: Check if media exists in cache
        var limits = await _mediaLimiter.AnalyzeMediaUserLimits(_mediaCache[UserId].Media);
        _mediaCache[UserId].Limits = limits;
        return limits;
    }

    //FIXME: Bad Request returned sometimes. Maybe problem with the bigger files
    //TODO: Make two separate methods
    [HttpPost("preload"), Authorize]
    public async Task<ActionResult<FileUploadResult>> PreloadNAnalyse(IFormFile file, CancellationToken ct = default){
        //Await for queue to upload file to server memory
        bool isHuge = file.Length < 1024*1024*512;
        if(isHuge)
            await _hugeMediaSemaphore.WaitAsync(ct);
        else
            await _mediaSemaphore.WaitAsync();
        //Copy file to server memory
        if(!_mediaCache.ContainsKey(UserId))
            _mediaCache[UserId] = new ();
        if(_mediaCache[UserId].Media != null)
            _mediaCache[UserId].Media.Dispose();
        _mediaCache[UserId].Media = new();
        await file.CopyToAsync(_mediaCache[UserId].Media);
        //Gather metadata
        //FIXME: The extention gathers from the filename, but we converting all the stuff to predetermined extentions
        _mediaCache[UserId].Metadata = new(){ //TODO: Reimplement all this shit with builders
            OriginalName = file.FileName,
            UploaderId = UserId,
            MimeType = file.ContentType,
        };
        //Analyse the media
        FileUploadResult res = new();
        var limits = await AnalyseCurrentMedia();
        if(limits == null)
            res.Success = false;
        else
        {
            res.Success = true;
            res.Limitations = limits;
        }
        return res;
    }

    [HttpPost("commit"), Authorize]
    public async Task<ActionResult<FileUploadResult>> UploadPreloaded([FromBody]MediaMetadata metadata){
        //Check user
        _logger.LogError("No errors");
        var user = await _userManager.GetUserAsync(User);
        if(!_mediaCache.ContainsKey(UserId) || _mediaCache[UserId].Media == null)
            return BadRequest("No media was preloaded to the server to upload it");
        var cahedMetadata = _mediaCache[UserId].Metadata;
        if(user == null || user.Id != cahedMetadata.UploaderId
        )
            return BadRequest("Uploading user is not consistent. Do not change users while trying to upload");
        //Check metadata
        cahedMetadata.Author = metadata.Author;
        cahedMetadata.IsPublic = metadata.IsPublic;
        cahedMetadata.IsExclusiveToAuthor = metadata.IsExclusiveToAuthor;
        cahedMetadata.UploadedAt = DateTime.UtcNow;

        //Compress if needed
        var limits = _mediaCache[UserId].Limits ?? await AnalyseCurrentMedia(); 
        if(limits.Limited)
            _mediaCache[UserId].ConvertedMedia = 
                await _mediaLimiter.RecodeMediaToLimits(
                    _mediaCache[UserId].Media,
                    limits
                );
        else
            _mediaCache[UserId].ConvertedMedia = _mediaCache[UserId].Media;
        //Add metadata to DB to get GUID
        _db.Medias.Add(cahedMetadata);
        //Upload
        bool uploadSuccess =
            await _mediaFileManager.UploadMediaAsync(
                _mediaCache[UserId].ConvertedMedia,
                cahedMetadata
            );

        if(uploadSuccess)
            _db.SaveChanges();
        else
            _db.Medias.Remove(cahedMetadata);

        var result = new FileUploadResult(){
            Success = uploadSuccess,
            Guid = uploadSuccess ? cahedMetadata.Guid : null,
            Limitations = limits
        };
        
        return result;
    }
    //TODO: Implement Upload method, by invoking Preload and Upload preloaded sequently
}