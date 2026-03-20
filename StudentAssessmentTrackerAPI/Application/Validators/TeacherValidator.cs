using FluentValidation;
using StudentAssessmentTracker.Application.DTOs;

namespace StudentAssessmentTracker.Application.Validators
{
    /// <summary>
    /// FluentValidation validator for <see cref="TeacherRegisterDto"/>
    /// </summary>
    public class TeacherRegisterValidator : AbstractValidator<TeacherRegisterDto>
    {
        /// <summary>
        /// Initializes validation rules for teacher registration
        /// </summary>
        public TeacherRegisterValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(8).WithMessage("Phone number must not exceed 8 characters.");

            RuleFor(x => x.SubjectId)
                .GreaterThan(0).WithMessage("A valid subject must be selected.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }

    /// <summary>
    /// FluentValidation validator for <see cref="TeacherLoginDto"/>
    /// </summary>
    public class TeacherLoginValidator : AbstractValidator<TeacherLoginDto>
    {
        /// <summary>
        /// Initializes validation rules for teacher login
        /// </summary>
        public TeacherLoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }

    /// <summary>
    /// FluentValidation validator for <see cref="TeacherUpdateDto"/>
    /// </summary>
    public class TeacherUpdateValidator : AbstractValidator<TeacherUpdateDto>
    {
        /// <summary>
        /// Initializes validation rules for teacher updates
        /// </summary>
        public TeacherUpdateValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(8).WithMessage("Phone number must not exceed 8 characters.");

            RuleFor(x => x.SubjectId)
                .GreaterThan(0).WithMessage("A valid subject must be selected.");
        }
    }
}
