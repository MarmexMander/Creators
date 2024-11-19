using System.Collections.ObjectModel;
using System.Data.Common;
using System.Drawing;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Creators.Models;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using Microsoft.AspNetCore.Identity;

namespace Creators.Services;

public class MediaLimiterService
{
    public enum MediaType{
        Video,
        Audio,
        Image,
        Unsupported,
        Other
    }

    private struct MediaAnalysisData{
        public IMediaAnalysis mediaAnalysis;
        public MediaType mediaType;
        public long size;
    }

    public class MediaLimitsData{
        public MediaType mediaType;
        public bool Limited {get; set;}
        public float? CompressionRatio {get; set;}
        public int? UploadedBitrate {get; set;}
        public Size? UploadedResolution {get; set;}
        public long Size;
    }

    private readonly HttpContext? _httpContext;
    private readonly UserManager<CreatorUser> _userManager;
    private readonly ReadOnlyDictionary<UploadTierEnum, UploadTier.MediaLimitations> _tierLimits;
    public MediaLimiterService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, UserManager<CreatorUser> userManager){
        _httpContext = httpContextAccessor.HttpContext;
        _userManager = userManager;
        //read limits from config
        
        var tierLimits = new Dictionary<UploadTierEnum, UploadTier.MediaLimitations>();

        foreach (UploadTierEnum tier in Enum.GetValues(typeof(UploadTierEnum)))
        {
            var sectionPath = $"MediaLimits:{(int)tier}";
            var limitations = configuration.GetSection(sectionPath).Get<UploadTier.MediaLimitations>();

            if (limitations != null)
            {
                tierLimits[tier] = limitations;
            }
            else
            {
                throw new InvalidOperationException($"Media limitations not found in configuration for tier {tier}");
            }
        }

        _tierLimits = new ReadOnlyDictionary<UploadTierEnum, UploadTier.MediaLimitations>(tierLimits);
    }

    private async Task<UploadTier?> GetCurrentUserTier(){
        if(_httpContext == null)
            return null;
        CreatorUser? user = await _userManager.GetUserAsync(_httpContext.User);
        return user?.UploadTier;
    }

    private MediaType DetermineMediaType(IMediaAnalysis mediaAnalysis){
        if (mediaAnalysis == null)
            return MediaType.Unsupported;

        if (mediaAnalysis.PrimaryVideoStream != null)
            if(mediaAnalysis.PrimaryVideoStream.Duration.TotalSeconds == 0)
                return MediaType.Image;
            else
                return MediaType.Video;

        if (mediaAnalysis.PrimaryAudioStream != null)
            return MediaType.Audio;
        
        return MediaType.Unsupported;
    }

    private Size GetLimitedResolution(Size origResolution, Size maxResolution)
    {
        if (origResolution.Width <= maxResolution.Width && origResolution.Height <= maxResolution.Height)
        {
            return origResolution;
        }

        double widthScale = (double)maxResolution.Width / origResolution.Width;
        double heightScale = (double)maxResolution.Height / origResolution.Height;

        double scaleFactor = Math.Min(widthScale, heightScale);

        int newWidth = (int)(origResolution.Width * scaleFactor);
        int newHeight = (int)(origResolution.Height * scaleFactor);

        return new Size(newWidth, newHeight);
    }
    
    private bool IsLimited(MediaAnalysisData data, UploadTier.MediaLimitations limits){
        return data.mediaType switch
        {
            MediaType.Video => 
                data.mediaAnalysis.PrimaryVideoStream == null ||
                data.mediaAnalysis.PrimaryVideoStream.BitRate > limits.VideoBitrate ||
                data.mediaAnalysis.PrimaryVideoStream.Width > limits.VideoResolution.Width ||
                data.mediaAnalysis.PrimaryVideoStream.Height > limits.VideoResolution.Height ||
                data.size > limits.VideoSize,
            
            MediaType.Audio => 
                data.mediaAnalysis.PrimaryAudioStream == null ||
                data.mediaAnalysis.PrimaryAudioStream.BitRate > (long)limits.AudioBitrate ||
                data.size > limits.AudioSize,
            
            MediaType.Image => 
                data.mediaAnalysis.PrimaryVideoStream == null ||
                data.mediaAnalysis.PrimaryVideoStream.Width > limits.ImageResolution.Width ||
                data.mediaAnalysis.PrimaryVideoStream.Height > limits.ImageResolution.Height ||
                data.size > limits.ImageSize,
            
            _ => throw new ArgumentException("Unknown type of media data", nameof(data))
        };
    }

    private void SetLimits(MediaAnalysisData data, UploadTier.MediaLimitations limits, ref MediaLimitsData result){
        if(result.Limited == null)
            result.Limited = IsLimited(data, limits);
        if(result.Limited == false)
            return;

        switch (data.mediaType){
            case MediaType.Video:
                var videoStream = data.mediaAnalysis.PrimaryVideoStream;
                result.UploadedBitrate = (int)Math.Min(videoStream.BitRate, limits.VideoBitrate);
                result.UploadedResolution = GetLimitedResolution(
                    new(videoStream.Width, videoStream.Height),
                    limits.VideoResolution);
                result.UploadedBitrate = (int)Math.Min(videoStream.BitRate, limits.VideoBitrate);
                result.Size = Math.Min(data.size, limits.VideoSize);
                break;
            case MediaType.Audio:
                var audioStream = data.mediaAnalysis.PrimaryAudioStream;
                result.UploadedBitrate = (int)Math.Max(audioStream.BitRate, limits.AudioBitrate);
                result.Size = Math.Min(data.size, limits.AudioSize);
                break;
            case MediaType.Image:
                var imageStream = data.mediaAnalysis.PrimaryVideoStream;
                result.UploadedResolution = GetLimitedResolution(
                    new(imageStream.Width, imageStream.Height),
                    limits.ImageResolution
                );
                result.Size = Math.Min(data.size, limits.ImageSize);
                break;
            // _:
            // //Maybe should throw exception on unsupported
            // break;
        }
        //TODO: Calculate result.CompressionRatio from size
    }

    public async Task<Stream> RecodeMediaToLimits(Stream media, MediaLimitsData limitsData)
    {
        // Create a temporary output stream for the recoded media
        var outputStream = new MemoryStream();
        MediaType mediaType = limitsData.mediaType;
        media.Position=0;
        // Create FFmpeg arguments
        var ffmpegArguments = FFMpegArguments.FromPipeInput(new StreamPipeSource(media))
            .OutputToPipe(new StreamPipeSink(outputStream), options =>
            {
                // Handle video settings
                if (mediaType == MediaType.Video)
                {
                    // Set video bitrate if it's limited
                    if (limitsData.UploadedBitrate.HasValue)
                    {
                        options.WithVideoBitrate(limitsData.UploadedBitrate.Value);
                    }

                    // Set video resolution if it's limited
                    if (limitsData.UploadedResolution.HasValue)
                    {
                        options.Resize(limitsData.UploadedResolution.Value.Width, limitsData.UploadedResolution.Value.Height);
                    }

                    // Apply compression ratio if specified
                    if (limitsData.CompressionRatio.HasValue)
                    {
                        int crf = (int)(limitsData.CompressionRatio.Value * 51); // Scale to FFMpeg's CRF range (0-51)
                        options.WithVideoCodec("libx264").WithConstantRateFactor(crf);
                        //TODO: Check video compression
                    }
                }
                // Handle audio settings
                else if (mediaType == MediaType.Audio)
                {
                    // Set audio bitrate if it's limited
                    if (limitsData.UploadedBitrate.HasValue)
                    {
                        options.WithAudioBitrate(limitsData.UploadedBitrate.Value);
                    }
                    //TODO: Implement audio compression
                }

                options.OverwriteExisting();
            }
        );

        // Run FFmpeg and wait for the process to complete
        await ffmpegArguments.ProcessAsynchronously();

        // Reset the output stream position for reading
        outputStream.Position = 0;

        return outputStream;
    }

    public async Task<MediaLimitsData?> AnalyzeMediaUserLimits(IMediaAnalysis mediaAnalysis, long fileSize){
        UploadTier? userTier = await GetCurrentUserTier();
        if (userTier == null)
            return null;

        var limits = _tierLimits[userTier];
        MediaAnalysisData mediaData = new(){
            mediaType = DetermineMediaType(mediaAnalysis),
            mediaAnalysis = mediaAnalysis,
            size = fileSize
        };
        //TODO: MediaLimits data and MediaAnalysis data using looks like pipeline or builder.
        MediaLimitsData result = new();
        result.mediaType = mediaData.mediaType;
        result.Limited = IsLimited(mediaData, limits);
        SetLimits(mediaData, limits, ref result);
        return result;
    }
    
    public async Task<MediaLimitsData?> AnalyzeMediaUserLimits(Stream mediaStream){
        mediaStream.Position = 0;
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try{
            using(FileStream temp = File.Create(tempPath)){
                await mediaStream.CopyToAsync(temp);
            }
            IMediaAnalysis mediaAnalysis = await FFProbe.AnalyseAsync(tempPath);
            long length = mediaStream.Length;
            return await AnalyzeMediaUserLimits(mediaAnalysis, length);
        }
        //catch(Exception ex){throw ex}
        finally{
            File.Delete(tempPath);
        }
    }
}