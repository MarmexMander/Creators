using Creators.Data;
using Creators.Models;
using Microsoft.AspNetCore.Mvc;
namespace Creators.Services;

class LocalMediaFileManager : IMediaFileManager, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly string _mediaLocation;
    private readonly SemaphoreSlim _writeSemaphore = new(8);
    bool _isDisposed = false;

    //TODO:?Use only guid to access file to fully separate from entity?
    public LocalMediaFileManager(IConfiguration configuration, ILogger<LocalMediaFileManager> logger){
        _configuration = configuration;
        _mediaLocation = _configuration.GetValue<string>("LocalMediaLocation");
        _logger = logger;
    }
    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        _writeSemaphore.Wait();
        _writeSemaphore.Dispose();
    }

    public FileStream GetMediaStream(Media metadata)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(LocalMediaFileManager));

        if (metadata == null) return null;
        var fileExtention = Path.GetExtension(metadata.OriginalName);
        var filePath = Path.Combine(_mediaLocation, $"{metadata.Guid}{fileExtention}"); 
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return fileStream;
    }

    public string GetMediaStaticUrl(Media metadata)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(LocalMediaFileManager));

        return $"/static/{metadata.Guid}";
    }


    public async Task<bool> UploadMediaAsync(Stream file, Media mediaData, CancellationToken ct = default)
    {
        if (_isDisposed)
                throw new ObjectDisposedException(nameof(LocalMediaFileManager));

        bool shouldRevert = false; 

        //Try to upload the file
        try
        {
            await _writeSemaphore.WaitAsync(ct);
            string fileExtention = Path.GetExtension(mediaData.OriginalName);
            string filePath = Path.Combine(_mediaLocation, $"{mediaData.Guid}{fileExtention}");
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream, ct);
            }
            
            if(ct.IsCancellationRequested){
                shouldRevert = true;
                return false;
            }
        }
        catch(TaskCanceledException){
            shouldRevert = true;
            _logger.LogWarning($"The uploadding of file {mediaData.OriginalName} was cancelled");
            return false;
        }
        catch (Exception ex)
        {
            shouldRevert = true;
            _logger.LogError(ex, $"Error while uploading file {mediaData.OriginalName}: {ex.Message}");
            return false;
        }
        finally
        {
            if (shouldRevert)
            {
                
            }
            else
            {

            }
            _writeSemaphore.Release();
        }
        return true;
    }

    //FIXME: Implemet I(Thumb)Manager service and use it to generate and show previews
    public string GetMediaPreviewStaticUrl(Media metadata)
    {
        return GetMediaStaticUrl(metadata);
    }

    //FIXME: Implemet I(Thumb)Manager service and use it to generate and show previews
    public FileStream GetMediaPreviewStream(Media metadata)
    {
        return GetMediaStream(metadata);
    }
}