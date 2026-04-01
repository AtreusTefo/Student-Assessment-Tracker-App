using FluentValidation;
using StudentAssessmentTracker.Application.DTOs;

namespace StudentAssessmentTracker.Application.Validators
{
    /// <summary>
    /// Validates the data required to create a new student.
    /// </summary>
    public class CreateStudentValidator : AbstractValidator<CreateStudentDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateStudentValidator"/> class.
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
            // TeacherId is no longer part of the request body — it is extracted from the authenticated teacher's JWT
        }
    }

    /// <summary>
    /// Validates the data required to update a student.
    /// </summary>
    public class UpdateStudentValidator : AbstractValidator<UpdateStudentDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateStudentValidator"/> class.
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

    /// <summary>
    /// Validates the data required to activate a student account.
    /// </summary>
    public class StudentActivateValidator : AbstractValidator<StudentActivateDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StudentActivateValidator"/> class.
        /// </summary>
        public StudentActivateValidator()
        {
            RuleFor(x => x.StudentUniqueId)
                .NotEmpty().WithMessage("Student ID is required.")
                .Matches(@"^STU-[A-Z0-9]{8}$").WithMessage("Student ID must be in the format STU-XXXXXXXX.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }

    /// <summary>
    /// Validates the data required for student login.
    /// </summary>
    public class StudentLoginValidator : AbstractValidator<StudentLoginDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StudentLoginValidator"/> class.
        /// </summary>
        public StudentLoginValidator()
        {
            RuleFor(x => x.StudentUniqueId)
                .NotEmpty().WithMessage("Student ID is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
