# RunwayML Gen-3 Alpha API Integration Guide

## 🎯 Why RunwayML Gen-3 Alpha is Excellent for EcomVideoAI

RunwayML Gen-3 Alpha offers premium video generation quality with excellent commercial licensing and competitive pricing.

### 💰 Cost Analysis
- **Direct Runway**: $0.25 per 5-second video (5 credits × $0.01)
- **AI/ML API**: ~40% cheaper than direct Runway pricing
- **Quality**: Premium, cinema-grade video generation
- **Commercial use**: Full commercial licensing included

### 🚀 Performance Benefits
- **Generation time**: 2-4 minutes average
- **Quality**: Excellent for product advertisements
- **Consistency**: Superior character and object consistency
- **Resolution**: Up to 1280x720, multiple aspect ratios

## 🔑 Getting API Access

### Option 1: Direct Runway API
1. Visit https://runwayml.com/api
2. Sign up and choose Build plan
3. Get API key from dashboard
4. Base URL: `https://api.runwayml.com`

### Option 2: AI/ML API (Recommended - Cheaper)
1. Visit https://aimlapi.com
2. Sign up and get API key
3. Access RunwayML models at lower cost
4. Base URL: `https://api.aimlapi.com`

## 🔧 API Integration

### 1. Client Setup
```csharp
public class RunwayAiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    
    public RunwayAiClient(string apiKey, string baseUrl = "https://api.aimlapi.com")
    {
        _apiKey = apiKey;
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }
}
```

### 2. Video Generation Models
```csharp
public class RunwayVideoRequest
{
    public string Prompt { get; set; }
    public string ImageUrl { get; set; } // Optional for image-to-video
    public string AspectRatio { get; set; } = "16:9"; // 16:9, 9:16, 4:3, 3:4, 1:1, 21:9
    public int Duration { get; set; } = 5; // 5 or 10 seconds
    public int? Seed { get; set; }
    public bool Watermark { get; set; } = false;
}

public class RunwayVideoResponse
{
    public string Id { get; set; }
    public string Status { get; set; }
    public string VideoUrl { get; set; }
    public string ThumbnailUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string ErrorMessage { get; set; }
    public int CreditsUsed { get; set; }
}
```

### 3. Generation Methods
```csharp
// Text-to-Video with Gen-3 Alpha Turbo
public async Task<RunwayVideoResponse> GenerateVideoFromTextAsync(RunwayVideoRequest request)
{
    var payload = new
    {
        model = "gen3a_turbo",
        prompt = request.Prompt,
        aspect_ratio = request.AspectRatio,
        duration = request.Duration,
        seed = request.Seed,
        watermark = request.Watermark
    };
    
    var response = await _httpClient.PostAsJsonAsync(
        "/runway/gen3a_turbo/text-to-video", 
        payload
    );
    
    return await response.Content.ReadFromJsonAsync<RunwayVideoResponse>();
}

// Image-to-Video with Gen-3 Alpha Turbo
public async Task<RunwayVideoResponse> GenerateVideoFromImageAsync(RunwayVideoRequest request)
{
    var payload = new
    {
        model = "gen3a_turbo",
        prompt = request.Prompt,
        image_url = request.ImageUrl,
        aspect_ratio = request.AspectRatio,
        duration = request.Duration,
        seed = request.Seed,
        watermark = request.Watermark
    };
    
    var response = await _httpClient.PostAsJsonAsync(
        "/runway/gen3a_turbo/image-to-video", 
        payload
    );
    
    return await response.Content.ReadFromJsonAsync<RunwayVideoResponse>();
}

// Status Checking
public async Task<RunwayVideoResponse> GetVideoStatusAsync(string generationId)
{
    var response = await _httpClient.GetAsync(
        $"/runway/generation/{generationId}"
    );
    
    return await response.Content.ReadFromJsonAsync<RunwayVideoResponse>();
}
```

## 🏗️ Service Implementation

### 1. RunwayML Service Interface
```csharp
public interface IRunwayVideoService
{
    Task<RunwayVideoResponse> GenerateVideoAsync(RunwayVideoRequest request);
    Task<RunwayVideoResponse> GetVideoStatusAsync(string generationId);
    Task<bool> IsVideoCompleteAsync(string generationId);
}
```

### 2. Service Implementation
```csharp
public class RunwayVideoService : IRunwayVideoService
{
    private readonly RunwayAiClient _client;
    private readonly ILogger<RunwayVideoService> _logger;
    
    public RunwayVideoService(RunwayAiClient client, ILogger<RunwayVideoService> logger)
    {
        _client = client;
        _logger = logger;
    }
    
    public async Task<RunwayVideoResponse> GenerateVideoAsync(RunwayVideoRequest request)
    {
        try
        {
            _logger.LogInformation($"Starting Runway video generation with prompt: {request.Prompt}");
            
            var result = string.IsNullOrEmpty(request.ImageUrl) 
                ? await _client.GenerateVideoFromTextAsync(request)
                : await _client.GenerateVideoFromImageAsync(request);
                
            _logger.LogInformation($"Runway generation started with ID: {result.Id}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video with Runway");
            throw;
        }
    }
    
    public async Task<RunwayVideoResponse> GetVideoStatusAsync(string generationId)
    {
        return await _client.GetVideoStatusAsync(generationId);
    }
    
    public async Task<bool> IsVideoCompleteAsync(string generationId)
    {
        var status = await GetVideoStatusAsync(generationId);
        return status.Status == "COMPLETED";
    }
}
```

## ⚙️ Configuration

### appsettings.json
```json
{
  "VideoGeneration": {
    "DefaultProvider": "Runway",
    "Providers": {
      "Runway": {
        "ApiKey": "your-runway-api-key",
        "BaseUrl": "https://api.aimlapi.com",
        "CostPerSecond": 0.05,
        "MaxDuration": 10,
        "Model": "gen3a_turbo"
      },
      "Freepik": {
        "ApiKey": "your-freepik-key",
        "BaseUrl": "https://api.freepik.com",
        "CostPerSecond": 0.08,
        "MaxDuration": 10
      }
    }
  }
}
```

### Dependency Injection
```csharp
// In Program.cs
builder.Services.AddScoped<RunwayAiClient>(provider =>
{
    var config = provider.GetService<IConfiguration>();
    var apiKey = config["VideoGeneration:Providers:Runway:ApiKey"];
    var baseUrl = config["VideoGeneration:Providers:Runway:BaseUrl"];
    return new RunwayAiClient(apiKey, baseUrl);
});

builder.Services.AddScoped<IRunwayVideoService, RunwayVideoService>();
```

## 🧪 Testing the API

### Simple Test Script
```csharp
public async Task TestRunwayApiAsync()
{
    var client = new RunwayAiClient("your-api-key");
    
    var request = new RunwayVideoRequest
    {
        Prompt = "A sleek product advertisement showing a modern smartphone rotating on a clean white background with soft lighting",
        AspectRatio = "16:9",
        Duration = 5,
        Watermark = false
    };
    
    // Start generation
    var result = await client.GenerateVideoFromTextAsync(request);
    Console.WriteLine($"Generation started: {result.Id}");
    
    // Poll for completion
    while (true)
    {
        var status = await client.GetVideoStatusAsync(result.Id);
        Console.WriteLine($"Status: {status.Status}");
        
        if (status.Status == "COMPLETED")
        {
            Console.WriteLine($"Video URL: {status.VideoUrl}");
            break;
        }
        
        if (status.Status == "FAILED")
        {
            Console.WriteLine($"Error: {status.ErrorMessage}");
            break;
        }
        
        await Task.Delay(10000); // Wait 10 seconds
    }
}
```

## 📊 Cost Comparison with Current Setup

### Current Freepik Costs
- **5-second video**: ~$0.40
- **10-second video**: ~$0.80
- **1000 videos/month**: ~$400-800

### RunwayML Gen-3 Alpha Costs
- **5-second video**: ~$0.25 (direct) / ~$0.15 (AI/ML API)
- **10-second video**: ~$0.50 (direct) / ~$0.30 (AI/ML API)
- **1000 videos/month**: ~$150-300 (AI/ML API)

### Potential Savings
- **Direct Runway**: 30-40% savings
- **AI/ML API**: 60-70% savings
- **Annual savings**: $2,400-5,600 for 1000 videos/month

## 🚀 Getting Started Steps

### 1. Get API Access (Recommended: AI/ML API)
```bash
# Visit https://aimlapi.com
# Sign up and get API key
# Test with curl:
curl -X POST "https://api.aimlapi.com/runway/gen3a_turbo/text-to-video" \
  -H "Authorization: Bearer YOUR_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "A professional product advertisement",
    "aspect_ratio": "16:9",
    "duration": 5,
    "watermark": false
  }'
```

### 2. Implement Service
- Add RunwayVideoService to your project
- Configure API key in appsettings.json
- Update video generation flow

### 3. Test Integration
- Generate test videos
- Compare quality with Freepik
- Measure cost savings

## 🎯 Why RunwayML Gen-3 Alpha is Excellent

1. **Premium Quality**: Cinema-grade video generation
2. **Commercial Ready**: Full commercial licensing
3. **Cost Effective**: 30-70% cheaper than current setup
4. **Fast Generation**: 2-4 minute average
5. **Excellent Consistency**: Superior for product videos
6. **Multiple Formats**: Support for various aspect ratios
7. **Reliable API**: Well-documented and stable

This integration will provide your users with premium video quality while reducing costs significantly compared to your current Freepik implementation. 