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

            RuleFor(x => x.Grade)
                .NotEmpty().WithMessage("Grade is required")
                .Length(1, 10).WithMessage("Grade must be 1-10 characters");

            RuleFor(x => x.Assessment1)
                .InclusiveBetween(0, 20).WithMessage("Assessment 1 must be between 0-20");

            RuleFor(x => x.Assessment2)
                .InclusiveBetween(0, 20).WithMessage("Assessment 2 must be between 0-20");

            RuleFor(x => x.Assessment3)
                .InclusiveBetween(0, 20).WithMessage("Assessment 3 must be between 0-20");
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

            RuleFor(x => x.Grade)
                .NotEmpty().WithMessage("Grade is required")
                .Length(1, 10).WithMessage("Grade must be 1-10 characters");

            RuleFor(x => x.Assessment1)
                .InclusiveBetween(0, 20).WithMessage("Assessment 1 must be between 0-20");

            RuleFor(x => x.Assessment2)
                .InclusiveBetween(0, 20).WithMessage("Assessment 2 must be between 0-20");

            RuleFor(x => x.Assessment3)
                .InclusiveBetween(0, 20).WithMessage("Assessment 3 must be between 0-20");
        }
    }
}
