using System.Drawing;
using Creators.Services;

public class FileUploadResult
{
    public Guid? Guid { get; set; }
    public bool? Success { get; set; }

    public MediaLimiterService.MediaLimitsData limitations;
}