using Back.Requestes;
using FluentValidation;

namespace Back.Validators;

public class CreateSubjectValidator : AbstractValidator<CreateSubjectRequest>
{
    public CreateSubjectValidator()
    {
        RuleFor(x => x.SubjectName)
            .NotEmpty().WithMessage("اسم المادة مطلوب")
            .MaximumLength(100).WithMessage("اسم المادة يجب أن لا يتجاوز 100 حرف");
    }
}

public class UpdateSubjectValidator : AbstractValidator<UpdateSubjectRequest>
{
    public UpdateSubjectValidator()
    {
        RuleFor(x => x.SubjectName)
            .MaximumLength(100).WithMessage("اسم المادة يجب أن لا يتجاوز 100 حرف")
            .When(x => x.SubjectName != null);
    }
}
