using Microsoft.AspNetCore.Mvc;

interface IMediaFileProvider
{
    Task<string?> GetMediaUrlAsync(Guid guid);
    FileStreamResult GetMediaStreamAsync(Guid guid);
    Task<bool> UploadMedia(IFormFile file, Guid guid);
}