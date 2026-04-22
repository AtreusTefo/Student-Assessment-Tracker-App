using FluentValidation;
using StudentAssessmentTracker.Application.DTOs;

namespace StudentAssessmentTracker.Application.Validators
{
    /// <summary>
    /// FluentValidation validator for <see cref="CreateClassGroupDto"/>.
    /// </summary>
    public class CreateClassGroupValidator : AbstractValidator<CreateClassGroupDto>
    {
        /// <summary>Initializes validation rules for class group creation.</summary>
        public CreateClassGroupValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Class group name is required.")
                .MaximumLength(100).WithMessage("Class group name must not exceed 100 characters.");

            RuleFor(x => x.SubjectId)
                .GreaterThan(0).WithMessage("A valid subject must be selected.");

            RuleFor(x => x.GradeId)
                .GreaterThan(0).WithMessage("A valid grade must be selected.");
        }
    }

    /// <summary>
    /// FluentValidation validator for <see cref="UpdateClassGroupDto"/>.
    /// </summary>
    public class UpdateClassGroupValidator : AbstractValidator<UpdateClassGroupDto>
    {
        /// <summary>Initializes validation rules for class group updates.</summary>
        public UpdateClassGroupValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Class group name is required.")
                .MinimumLength(2).WithMessage("Class group name must be at least 2 characters.")
                .MaximumLength(100).WithMessage("Class group name must not exceed 100 characters.");
        }
    }
}
