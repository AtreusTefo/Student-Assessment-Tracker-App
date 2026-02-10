using FluentValidation;
using StudentAssessmentTracker.Models;

namespace StudentAssessmentTracker.Validators;

public class StudentValidator : AbstractValidator<Student>
{
    public StudentValidator()
    {
        RuleFor(s => s.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("First name is required")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        RuleFor(s => s.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Last name is required")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

        RuleFor(s => s.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MinimumLength(3).WithMessage("Email must be at least 3 characters long")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(s => s.Phone)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Phone is required")
            .Length(8).WithMessage("Phone must be exactly 8 digits")
            .Matches(@"^\d{8}$").WithMessage("Enter valid Phone number");

        RuleFor(s => s.Grade)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Grade is required")
            .MaximumLength(10).WithMessage("Grade cannot exceed 10 characters");

        RuleFor(s => s.Assessment1)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(0, 20).WithMessage("Assessment 1 must be between 0 and 20");

        RuleFor(s => s.Assessment2)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(0, 20).WithMessage("Assessment 2 must be between 0 and 20");

        RuleFor(s => s.Assessment3)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(0, 20).WithMessage("Assessment 3 must be between 0 and 20");
    }
}
