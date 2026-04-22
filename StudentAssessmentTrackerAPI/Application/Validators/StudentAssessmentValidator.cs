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
        /// <summary>Initialises validation rules for creating a student assessment.</summary>
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

            RuleFor(x => x.Instructions)
                .MaximumLength(2000).WithMessage("Instructions cannot exceed 2000 characters")
                .When(x => x.Instructions != null);
        }
    }

    /// <summary>
    /// Validation rules for updating an existing assessment record
    /// </summary>
    public class UpdateStudentAssessmentValidator : AbstractValidator<UpdateStudentAssessmentDto>
    {
        /// <summary>Initialises validation rules for updating a student assessment.</summary>
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

            RuleFor(x => x.Instructions)
                .MaximumLength(2000).WithMessage("Instructions cannot exceed 2000 characters")
                .When(x => x.Instructions != null);
        }
    }
}
