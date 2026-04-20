using Back.Requestes;
using FluentValidation;

namespace Back.Validators;

public class CreateGradeValidator : AbstractValidator<CreateGradeRequest>
{
    public CreateGradeValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(0, 100).WithMessage("الدرجة يجب أن تكون بين 0 و 100");

        RuleFor(x => x.GradeType)
            .NotEmpty().WithMessage("نوع الاختبار مطلوب")
            .MaximumLength(50).WithMessage("نوع الاختبار يجب أن لا يتجاوز 50 حرف");

        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("يجب تحديد الطالب");

        RuleFor(x => x.TeacherId)
            .GreaterThan(0).WithMessage("يجب تحديد المعلم");

        RuleFor(x => x.SubjectId)
            .GreaterThan(0).WithMessage("يجب تحديد المادة");
    }
}

public class UpdateGradeValidator : AbstractValidator<UpdateGradeRequest>
{
    public UpdateGradeValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(0, 100).WithMessage("الدرجة يجب أن تكون بين 0 و 100")
            .When(x => x.Score.HasValue);

        RuleFor(x => x.GradeType)
            .MaximumLength(50).WithMessage("نوع الاختبار يجب أن لا يتجاوز 50 حرف")
            .When(x => x.GradeType != null);
    }
}
