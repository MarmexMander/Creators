using System.Drawing;
using FFMpegCore.Enums;

namespace Creators.Models;
public enum UploadTierEnum
{
    Tier1 = 1,
    Tier2,
    Tier3,
    Tier4,
    Tier5,
}

public class UploadTier
{
    public class MediaLimitations{
        public int AudioBitrate { get; set; }
        public long AudioSize { get; set; }
        public int VideoBitrate { get; set; }
        public long VideoSize { get; set; }
        public Size VideoResolution { get; set; }
        public Size ImageResolution { get; set; }
        public long ImageSize { get; set; }
        public bool postNSFW { get; set; } = false;
    }
    public UploadTier(UploadTierEnum categoryEnum)
    {
        Id = (int)categoryEnum;
    }

    public UploadTier(){}

    public int Id { get; set;}

    public static implicit operator UploadTier (UploadTierEnum categoryEnum)
     => new UploadTier (categoryEnum);
    public static implicit operator UploadTierEnum (UploadTier category)
     => (UploadTierEnum) category.Id;
}