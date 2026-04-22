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
            RuleFor(x => x.IdPassportNo)
                .NotEmpty().WithMessage("ID/Passport No. is required.")
                .Length(9).WithMessage("ID/Passport No. must be exactly 9 characters.")
                .Matches("^[a-zA-Z0-9\\-]+$").WithMessage("ID/Passport No. can only contain letters, numbers, and hyphens.");

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
                .Matches(@"^\d{8}$").WithMessage("Phone number must be exactly 8 digits.");

            RuleFor(x => x.SubjectId)
                .GreaterThan(0).WithMessage("A valid subject must be selected.");
        }
    }

    /// <summary>
    /// FluentValidation validator for <see cref="TeacherActivateDto"/>
    /// </summary>
    public class TeacherActivateValidator : AbstractValidator<TeacherActivateDto>
    {
        /// <summary>
        /// Initializes validation rules for teacher account activation
        /// </summary>
        public TeacherActivateValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Please confirm your password.")
                .Equal(x => x.Password).WithMessage("Passwords do not match.");
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
            RuleFor(x => x.IdPassportNo)
                .NotEmpty().WithMessage("ID/Passport No. is required.")
                .Length(9).WithMessage("ID/Passport No. must be exactly 9 characters.")
                .Matches("^[a-zA-Z0-9\\-]+$").WithMessage("ID/Passport No. can only contain letters, numbers, and hyphens.");

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
                .Matches(@"^\d{8}$").WithMessage("Phone number must be exactly 8 digits.");

            RuleFor(x => x.SubjectId)
                .GreaterThan(0).WithMessage("A valid subject must be selected.");
        }
    }
}
