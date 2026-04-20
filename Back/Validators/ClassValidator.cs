using Back.Requestes;
using FluentValidation;

namespace Back.Validators;

public class CreateClassValidator : AbstractValidator<CreateClassRequest>
{
    public CreateClassValidator()
    {
        RuleFor(x => x.ClassNumber)
            .NotEmpty().WithMessage("رقم الصف مطلوب")
            .MaximumLength(50).WithMessage("رقم الصف يجب أن لا يتجاوز 50 حرف");

        RuleFor(x => x.StudentStep)
            .IsInEnum().WithMessage("المرحلة الدراسية غير صالحة");
    }
}

public class UpdateClassValidator : AbstractValidator<UpdateClassRequest>
{
    public UpdateClassValidator()
    {
        RuleFor(x => x.ClassNumber)
            .MaximumLength(50).WithMessage("رقم الصف يجب أن لا يتجاوز 50 حرف")
            .When(x => x.ClassNumber != null);

        RuleFor(x => x.StudentStep)
            .IsInEnum().WithMessage("المرحلة الدراسية غير صالحة")
            .When(x => x.StudentStep.HasValue);
    }
}
