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

    public async Task<bool> UploadMediaAsync(Media metadata, IFormFile file)//TODO: implement full uploading and metadata generation
    {
        if (_isDisposed)
                throw new ObjectDisposedException(nameof(LocalMediaFileManager));

        await _writeSemaphore.WaitAsync();
        try
        {
            string fileExtention = Path.GetExtension(metadata.OriginalName);
            string filePath = Path.Combine(_mediaLocation, $"{metadata.Guid}{fileExtention}");
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while uploading file {file.FileName}"); //TODO: add more verbose output
            return false;
        }
        finally
        {
            _writeSemaphore.Release();
        }
    }
}