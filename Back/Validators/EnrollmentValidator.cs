using Back.Requestes;
using FluentValidation;

namespace Back.Validators;

public class CreateEnrollmentValidator : AbstractValidator<CreateEnrollmentRequest>
{
    public CreateEnrollmentValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("يجب تحديد الطالب");

        RuleFor(x => x.SubjectId)
            .GreaterThan(0).WithMessage("يجب تحديد المادة");

        RuleFor(x => x.ClassId)
            .GreaterThan(0).WithMessage("يجب تحديد الصف");
    }
}
