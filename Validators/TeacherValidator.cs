using FluentValidation;
using StudentAssessmentTracker.Models;

namespace StudentAssessmentTracker.Validators;

public class TeacherValidator : AbstractValidator<Teacher>
{
    public TeacherValidator()
    {
        RuleFor(t => t.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("First name is required")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        RuleFor(t => t.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Last name is required")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

        RuleFor(t => t.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(t => t.Phone)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Phone is required")
            .Length(8).WithMessage("Phone must be exactly 8 digits")
            .Matches(@"^\d{8}$").WithMessage("Enter valid Phone number");

        RuleFor(t => t.Subject)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(100).WithMessage("Subject cannot exceed 100 characters");

        RuleFor(t => t.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(20).WithMessage("Password cannot exceed 20 characters");

    }
}
