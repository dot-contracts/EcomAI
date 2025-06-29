namespace EcomVideoAI.Application.DTOs.Responses.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiResponse() { }

        public ApiResponse(T data, string? message = null)
        {
            Success = true;
            Data = data;
            Message = message;
        }

        public ApiResponse(string error)
        {
            Success = false;
            Errors.Add(error);
        }

        public ApiResponse(List<string> errors)
        {
            Success = false;
            Errors = errors;
        }

        public static ApiResponse<T> SuccessResult(T data, string? message = null)
        {
            return new ApiResponse<T>(data, message);
        }

        public static ApiResponse<T> ErrorResult(string error)
        {
            return new ApiResponse<T>(error);
        }

        public static ApiResponse<T> ErrorResult(List<string> errors)
        {
            return new ApiResponse<T>(errors);
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse() : base() { }
        public ApiResponse(string message) : base()
        {
            Success = true;
            Message = message;
        }

        public static ApiResponse SuccessResult(string message)
        {
            return new ApiResponse(message);
        }

        public new static ApiResponse ErrorResult(string error)
        {
            return new ApiResponse { Success = false, Errors = [error] };
        }

        public new static ApiResponse ErrorResult(List<string> errors)
        {
            return new ApiResponse { Success = false, Errors = errors };
        }
    }
} 