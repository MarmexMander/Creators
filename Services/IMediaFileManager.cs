using Creators.Models;
using Microsoft.AspNetCore.Mvc;
namespace Creators.Services;

public interface IMediaFileManager
{
    string GetMediaStaticUrl(Media metadata);
    FileStream GetMediaStream(Media metadata);
    string GetMediaPreviewStaticUrl(Media metadata);
    FileStream GetMediaPreviewStream(Media metadata);
    //FileStream GetMediaStreamAsync(Guid guid);

    //FIXME: Remove metadata argument. Temoral quick-n-junk workaround. Only files IO here
    Task<bool> UploadMediaAsync(Stream file, Media metadata ,CancellationToken ct = default);
}