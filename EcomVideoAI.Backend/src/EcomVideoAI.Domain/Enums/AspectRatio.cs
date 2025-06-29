namespace EcomVideoAI.Domain.Enums
{
    public enum AspectRatio
    {
        Portrait_9_16 = 0,      // 9:16 (mobile/social)
        Landscape_16_9 = 1,     // 16:9 (traditional video)
        Square_1_1 = 2,         // 1:1 (square/Instagram)
        Portrait_3_4 = 3        // 3:4 (portrait)
    }

    public static class AspectRatioExtensions
    {
        public static string ToFreepikFormat(this AspectRatio aspectRatio)
        {
            return aspectRatio switch
            {
                AspectRatio.Portrait_9_16 => "portrait_9_16",
                AspectRatio.Landscape_16_9 => "widescreen_16_9",
                AspectRatio.Square_1_1 => "square_1_1",
                AspectRatio.Portrait_3_4 => "portrait_3_4",
                _ => "portrait_9_16"
            };
        }

        public static AspectRatio FromString(string aspectRatioString)
        {
            return aspectRatioString switch
            {
                "9:16" => AspectRatio.Portrait_9_16,
                "16:9" => AspectRatio.Landscape_16_9,
                "1:1" => AspectRatio.Square_1_1,
                "3:4" => AspectRatio.Portrait_3_4,
                _ => AspectRatio.Portrait_9_16
            };
        }

        public static (int width, int height) GetDimensions(this AspectRatio aspectRatio, VideoResolution resolution)
        {
            var baseSize = resolution switch
            {
                VideoResolution.SD_480p => 480,
                VideoResolution.HD_720p => 720,
                VideoResolution.FullHD_1080p => 1080,
                _ => 720
            };

            return aspectRatio switch
            {
                AspectRatio.Portrait_9_16 => (baseSize * 9 / 16, baseSize),
                AspectRatio.Landscape_16_9 => (baseSize * 16 / 9, baseSize),
                AspectRatio.Square_1_1 => (baseSize, baseSize),
                AspectRatio.Portrait_3_4 => (baseSize * 3 / 4, baseSize),
                _ => (baseSize * 9 / 16, baseSize)
            };
        }
    }
} 