using Back.Requestes;
using FluentValidation;

namespace Back.Validators;

public class CreateAttendanceValidator : AbstractValidator<CreateAttendanceRequest>
{
    public CreateAttendanceValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("التاريخ مطلوب")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("لا يمكن تسجيل حضور لتاريخ مستقبلي");

        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("يجب تحديد الطالب");

        RuleFor(x => x.TeacherId)
            .GreaterThan(0).WithMessage("يجب تحديد المعلم");
    }
}

public class UpdateAttendanceValidator : AbstractValidator<UpdateAttendanceRequest>
{
    public UpdateAttendanceValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("الملاحظات يجب أن لا تتجاوز 500 حرف")
            .When(x => x.Notes != null);
    }
}
