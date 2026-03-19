using FluentValidation;
using StudentAssessmentTracker.Application.DTOs;

namespace StudentAssessmentTracker.Application.Validators
{
    /// <summary>
    /// Validator for CreateStudentDto
    /// Enforces business rules for student creation
    /// </summary>
    public class CreateStudentValidator : AbstractValidator<CreateStudentDto>
    {
        /// <summary>
        /// Initializes validation rules for creating students
        /// </summary>
        public CreateStudentValidator()
        {
            RuleFor(x => x.IdPassportNo)
                .NotEmpty().WithMessage("ID/Passport No. is required")
                .Length(9).WithMessage("ID/Passport No. must be exactly 9 characters")
                .Matches(@"^[a-zA-Z0-9\-]+$").WithMessage("ID/Passport No. can only contain letters, numbers, and hyphens");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(2, 50).WithMessage("First name must be 2-50 characters")
                .Matches(@"^[a-zA-Z\s\-]+$").WithMessage("First name can only contain letters, spaces, and hyphens");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(2, 50).WithMessage("Last name must be 2-50 characters")
                .Matches(@"^[a-zA-Z\s\-]+$").WithMessage("Last name can only contain letters, spaces, and hyphens");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^\d{8}$").WithMessage("Phone must be exactly 8 digits");

            RuleFor(x => x.GradeId)
                .GreaterThan(0).WithMessage("A valid grade must be selected");

            RuleFor(x => x.TeacherId)
                .GreaterThan(0).WithMessage("Teacher ID is required");
        }
    }

    /// <summary>
    /// Validator for UpdateStudentDto
    /// Enforces business rules for student updates
    /// </summary>
    public class UpdateStudentValidator : AbstractValidator<UpdateStudentDto>
    {
        /// <summary>
        /// Initializes validation rules for updating students
        /// </summary>
        public UpdateStudentValidator()
        {
            RuleFor(x => x.IdPassportNo)
                .NotEmpty().WithMessage("ID/Passport No. is required")
                .Length(9).WithMessage("ID/Passport No. must be exactly 9 characters")
                .Matches(@"^[a-zA-Z0-9\-]+$").WithMessage("ID/Passport No. can only contain letters, numbers, and hyphens");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(2, 50).WithMessage("First name must be 2-50 characters")
                .Matches(@"^[a-zA-Z\s\-]+$").WithMessage("First name can only contain letters, spaces, and hyphens");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(2, 50).WithMessage("Last name must be 2-50 characters")
                .Matches(@"^[a-zA-Z\s\-]+$").WithMessage("Last name can only contain letters, spaces, and hyphens");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^\d{8}$").WithMessage("Phone must be exactly 8 digits");

            RuleFor(x => x.GradeId)
                .GreaterThan(0).WithMessage("A valid grade must be selected");
        }
    }
}
