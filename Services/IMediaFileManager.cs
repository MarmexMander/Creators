using Creators.Models;
using Microsoft.AspNetCore.Mvc;
namespace Creators.Services;

public interface IMediaFileManager
{
    string GetMediaStaticUrl(Media metadata);
    FileStreamResult GetMediaWebStream(Media metadata);
    //FileStream GetMediaStreamAsync(Guid guid);
    Task<bool> UploadMediaAsync( Media metadata, IFormFile file);
}