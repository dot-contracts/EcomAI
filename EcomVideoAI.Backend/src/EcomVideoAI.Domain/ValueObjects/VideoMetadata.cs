namespace EcomVideoAI.Domain.ValueObjects
{
    public class VideoMetadata
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public double FrameRate { get; private set; }
        public string Format { get; private set; }
        public string Codec { get; private set; }
        public int Bitrate { get; private set; }
        public Dictionary<string, object> AdditionalProperties { get; private set; }

        public VideoMetadata(
            int width,
            int height,
            double frameRate,
            string format,
            string codec,
            int bitrate,
            Dictionary<string, object>? additionalProperties = null)
        {
            Width = width > 0 ? width : throw new ArgumentException("Width must be positive", nameof(width));
            Height = height > 0 ? height : throw new ArgumentException("Height must be positive", nameof(height));
            FrameRate = frameRate > 0 ? frameRate : throw new ArgumentException("Frame rate must be positive", nameof(frameRate));
            Format = format ?? throw new ArgumentNullException(nameof(format));
            Codec = codec ?? throw new ArgumentNullException(nameof(codec));
            Bitrate = bitrate > 0 ? bitrate : throw new ArgumentException("Bitrate must be positive", nameof(bitrate));
            AdditionalProperties = additionalProperties ?? new Dictionary<string, object>();
        }

        public string GetAspectRatio()
        {
            var gcd = CalculateGCD(Width, Height);
            var aspectWidth = Width / gcd;
            var aspectHeight = Height / gcd;
            return $"{aspectWidth}:{aspectHeight}";
        }

        private static int CalculateGCD(int a, int b)
        {
            while (b != 0)
            {
                var temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public override bool Equals(object? obj)
        {
            return obj is VideoMetadata other &&
                   Width == other.Width &&
                   Height == other.Height &&
                   Math.Abs(FrameRate - other.FrameRate) < 0.001 &&
                   Format == other.Format &&
                   Codec == other.Codec &&
                   Bitrate == other.Bitrate;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Width, Height, FrameRate, Format, Codec, Bitrate);
        }
    }
} 