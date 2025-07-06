namespace EcomVideoAI.Domain.Enums
{
    public enum AspectRatio
    {
        Portrait916 = 0,    // 9:16 - Default mobile format
        Landscape169 = 1,   // 16:9 - Standard widescreen
        Square11 = 2,       // 1:1 - Square format
        Portrait45 = 3,     // 4:5 - Instagram portrait
        Portrait23 = 4      // 2:3 - Classic portrait
    }

    public static class AspectRatioExtensions
    {
        public static string ToDisplayString(this AspectRatio ratio)
        {
            return ratio switch
            {
                AspectRatio.Portrait916 => "9:16",
                AspectRatio.Landscape169 => "16:9",
                AspectRatio.Square11 => "1:1",
                AspectRatio.Portrait45 => "4:5",
                AspectRatio.Portrait23 => "2:3",
                _ => "9:16"
            };
        }

        public static AspectRatio FromString(string ratio)
        {
            return ratio?.ToLower() switch
            {
                "9:16" => AspectRatio.Portrait916,
                "16:9" => AspectRatio.Landscape169,
                "1:1" => AspectRatio.Square11,
                "4:5" => AspectRatio.Portrait45,
                "2:3" => AspectRatio.Portrait23,
                _ => AspectRatio.Portrait916
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
                AspectRatio.Portrait916 => (baseSize * 9 / 16, baseSize),
                AspectRatio.Landscape169 => (baseSize * 16 / 9, baseSize),
                AspectRatio.Square11 => (baseSize, baseSize),
                AspectRatio.Portrait45 => (baseSize * 4 / 5, baseSize),
                AspectRatio.Portrait23 => (baseSize * 2 / 3, baseSize),
                _ => (baseSize * 9 / 16, baseSize)
            };
        }
    }
} 