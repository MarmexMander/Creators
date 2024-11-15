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
    private readonly SemaphoreSlim //TODO: use configuration for semaphore threshold
        _hugeMediaSemaphore = new(5),
        _mediaSemaphore = new(100); 
    private static readonly ConcurrentDictionary<ClaimsPrincipal, MediaData> _mediaCache = new();
    

    public StaticMediaController(CreatorsDbContext db, UserManager<CreatorUser> userManager, IMediaFileManager mediaFileManager, MediaLimiterService mediaLimiter){
        _db = db;
        _userManager = userManager;
        _mediaFileManager = mediaFileManager;
        _mediaLimiter = mediaLimiter;
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
        var limits = await _mediaLimiter.AnalyzeMediaUserLimits(_mediaCache[User].Media);
        _mediaCache[User].Limits = limits;
        return limits;
    }

    public async Task<ActionResult<FileUploadResult>> PreloadNAnalyse(IFormFile file, CancellationToken ct){
        //Await for queue to upload file to server memory
        bool isHuge = file.Length < 1024*1024*512;
        if(isHuge)
            await _hugeMediaSemaphore.WaitAsync(ct);
        else
            await _mediaSemaphore.WaitAsync();
        //Copy file to server memory
        if(_mediaCache[User].Media != null)
            _mediaCache[User].Media.Dispose();
        _mediaCache[User].Media = new();
        await file.CopyToAsync(_mediaCache[User].Media);
        //Gather metadata
        _mediaCache[User].Metadata = new(){ //TODO: Reimplement all this shit with builders
            OriginalName = file.FileName,
            Uploader = await _userManager.GetUserAsync(User),
            MimeType = file.ContentType,
        };
        //Analyse the media
        FileUploadResult res = new();
        var limits = await AnalyseCurrentMedia();
        if(limits != null)
            res.Success = false;
        else
        {
            res.Success = true;
            res.limitations = limits.Value;
        }
        return res;
    }

    [Authorize]
    public async Task<ActionResult<FileUploadResult>> UploadPreloaded(MediaMetadata metadata){
        //Check user
        var user = await _userManager.GetUserAsync(User);
        var cahedMetadata = _mediaCache[User].Metadata;
        if(user == null || user != cahedMetadata.Uploader)
            return BadRequest("Uploading user is not consistent. Do not change users while trying to upload");
        //Check metadata
        cahedMetadata.Author = metadata.Author;
        cahedMetadata.Group = metadata.Group;
        cahedMetadata.IsPublic = metadata.IsPublic;
        cahedMetadata.IsExclusiveToAuthor = metadata.IsExclusiveToAuthor;
        cahedMetadata.UploadedAt = DateTime.UtcNow;

        //Compress if needed
        var limits = _mediaCache[User].Limits ?? await AnalyseCurrentMedia();
        if(limits.Value.Limited)
            _mediaCache[User].ConvertedMedia = 
                await _mediaLimiter.RecodeMediaToLimits(
                    _mediaCache[User].Media,
                    limits.Value
                );
        else
            _mediaCache[User].ConvertedMedia = _mediaCache[User].Media;
        //Add metaadata to DB to get GUID
        _db.Medias.Add(cahedMetadata);
        //Upload
        bool uploadSuccess =
            await _mediaFileManager.UploadMediaAsync(
                _mediaCache[User].ConvertedMedia,
                cahedMetadata
            );

        var result = new FileUploadResult(){
            Success = uploadSuccess,
            Guid = uploadSuccess ? cahedMetadata.Guid : null,
            limitations = limits.Value
        };
        
        return result;
    }
}