using Microsoft.AspNetCore.Mvc;

class LocalMediaFileProvider : IMediaFileProvider, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly string _mediaLocation;
    private readonly SemaphoreSlim _writeSemaphore = new(8);
    bool _isDisposed = false;

    public LocalMediaFileProvider(IConfiguration configuration, ILogger logger){
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

    public FileStreamResult GetMediaStreamAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> GetMediaUrlAsync(Guid guid) //TODO: change url generation to static medial controller
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(LocalMediaFileProvider));
        DirectoryInfo mediaDir = new DirectoryInfo(_mediaLocation);

        return
         mediaDir.GetFiles(guid.ToString() + "*", SearchOption.TopDirectoryOnly)
        .First().FullName;
    }

    public async Task<bool> UploadMedia(IFormFile file, Guid guid)
    {
        await _writeSemaphore.WaitAsync();
        try
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(LocalMediaFileProvider));
            string fileExtention = Path.GetExtension(file.FileName);
            string filePath = Path.Combine(_mediaLocation, $"{guid}.{fileExtention}");
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