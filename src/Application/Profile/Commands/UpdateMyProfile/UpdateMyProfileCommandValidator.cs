namespace IgnaCheck.Application.Profile.Commands.UpdateMyProfile;

/// <summary>
/// Validator for UpdateMyProfileCommand.
/// </summary>
public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.")
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.")
            .When(x => x.LastName != null);

        RuleFor(x => x.JobTitle)
            .MaximumLength(200).WithMessage("Job title must not exceed 200 characters.")
            .When(x => x.JobTitle != null);

        RuleFor(x => x.Department)
            .MaximumLength(200).WithMessage("Department must not exceed 200 characters.")
            .When(x => x.Department != null);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .When(x => x.PhoneNumber != null);

        RuleFor(x => x.TimeZone)
            .MaximumLength(100).WithMessage("Time zone must not exceed 100 characters.")
            .When(x => x.TimeZone != null);

        RuleFor(x => x.PreferredLanguage)
            .MaximumLength(10).WithMessage("Preferred language must not exceed 10 characters.")
            .When(x => x.PreferredLanguage != null);
    }
}
