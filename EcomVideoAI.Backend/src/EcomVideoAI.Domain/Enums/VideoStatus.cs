namespace EcomVideoAI.Domain.Enums
{
    public enum VideoStatus
    {
        Pending = 0,
        Processing = 1,
        GeneratingImage = 2,
        GeneratingVideo = 3,
        Completed = 4,
        Failed = 5,
        Cancelled = 6
    }
} 