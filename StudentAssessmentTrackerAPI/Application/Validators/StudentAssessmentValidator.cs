using FluentValidation;
using StudentAssessmentTracker.Application.DTOs;

namespace StudentAssessmentTracker.Application.Validators
{
    /// <summary>
    /// Validation rules for adding a new assessment to a student.
    /// Score is validated against MaxScore (not a hardcoded system limit)
    /// so teachers can use any marking scale they choose.
    /// </summary>
    public class CreateStudentAssessmentValidator : AbstractValidator<CreateStudentAssessmentDto>
    {
        public CreateStudentAssessmentValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Assessment name is required")
                .MaximumLength(100).WithMessage("Assessment name cannot exceed 100 characters");

            RuleFor(x => x.MaxScore)
                .GreaterThan(0).WithMessage("Max score must be greater than 0");

            RuleFor(x => x.Score)
                .GreaterThanOrEqualTo(0).WithMessage("Score cannot be negative")
                .Must((dto, score) => score <= dto.MaxScore)
                .WithMessage("Score cannot exceed the max score for this assessment");
        }
    }

    /// <summary>
    /// Validation rules for updating an existing assessment record
    /// </summary>
    public class UpdateStudentAssessmentValidator : AbstractValidator<UpdateStudentAssessmentDto>
    {
        public UpdateStudentAssessmentValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Assessment name is required")
                .MaximumLength(100).WithMessage("Assessment name cannot exceed 100 characters");

            RuleFor(x => x.MaxScore)
                .GreaterThan(0).WithMessage("Max score must be greater than 0");

            RuleFor(x => x.Score)
                .GreaterThanOrEqualTo(0).WithMessage("Score cannot be negative")
                .Must((dto, score) => score <= dto.MaxScore)
                .WithMessage("Score cannot exceed the max score for this assessment");
        }
    }
}
