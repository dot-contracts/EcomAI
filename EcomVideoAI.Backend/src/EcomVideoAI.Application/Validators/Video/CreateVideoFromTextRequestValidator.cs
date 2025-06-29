using EcomVideoAI.Application.DTOs.Requests.Video;
using FluentValidation;

namespace EcomVideoAI.Application.Validators.Video
{
    public class CreateVideoFromTextRequestValidator : AbstractValidator<CreateVideoFromTextRequest>
    {
        public CreateVideoFromTextRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required")
                .MaximumLength(200)
                .WithMessage("Title must not exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Description must not exceed 1000 characters");

            RuleFor(x => x.TextPrompt)
                .NotEmpty()
                .WithMessage("Text prompt is required")
                .MinimumLength(10)
                .WithMessage("Text prompt must be at least 10 characters")
                .MaximumLength(2000)
                .WithMessage("Text prompt must not exceed 2000 characters");

            RuleFor(x => x.NegativePrompt)
                .MaximumLength(1000)
                .WithMessage("Negative prompt must not exceed 1000 characters");

            RuleFor(x => x.Duration)
                .GreaterThan(0)
                .WithMessage("Duration must be greater than 0")
                .LessThanOrEqualTo(30)
                .WithMessage("Duration must not exceed 30 seconds");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.Resolution)
                .IsInEnum()
                .WithMessage("Invalid resolution value");
        }
    }
} 