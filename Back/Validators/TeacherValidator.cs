using Back.Requestes;
using FluentValidation;

namespace Back.Validators;

public class CreateTeacherValidator : AbstractValidator<CreateTeacherRequest>
{
    public CreateTeacherValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("الاسم الأول مطلوب")
            .MaximumLength(100).WithMessage("الاسم الأول يجب أن لا يتجاوز 100 حرف");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("اسم العائلة مطلوب")
            .MaximumLength(100).WithMessage("اسم العائلة يجب أن لا يتجاوز 100 حرف");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("البريد الإلكتروني غير صالح");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة")
            .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل")
            .Matches("[A-Z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف كبير")
            .Matches("[0-9]").WithMessage("كلمة المرور يجب أن تحتوي على رقم");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("رقم الهاتف يجب أن لا يتجاوز 20 رقم")
            .When(x => x.Phone != null);

        RuleFor(x => x.SubjectId)
            .GreaterThan(0).WithMessage("يجب اختيار المادة");

        RuleFor(x => x.ClassId)
            .GreaterThan(0).WithMessage("يجب اختيار الصف");

        RuleFor(x => x.Image)
            .Must(img => img == null || img.Length <= 5 * 1024 * 1024)
            .WithMessage("حجم الصورة يجب أن لا يتجاوز 5 ميجابايت")
            .Must(img => img == null || new[] { ".jpg", ".jpeg", ".png", ".webp" }
                .Contains(Path.GetExtension(img.FileName).ToLower()))
            .WithMessage("صيغة الصورة يجب أن تكون jpg أو png أو webp");
    }
}

public class UpdateTeacherValidator : AbstractValidator<UpdateTeacherRequest>
{
    public UpdateTeacherValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("الاسم الأول يجب أن لا يتجاوز 100 حرف")
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("اسم العائلة يجب أن لا يتجاوز 100 حرف")
            .When(x => x.LastName != null);

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("رقم الهاتف يجب أن لا يتجاوز 20 رقم")
            .When(x => x.Phone != null);

        RuleFor(x => x.SubjectId)
            .GreaterThan(0).WithMessage("المادة غير صالحة")
            .When(x => x.SubjectId.HasValue);

        RuleFor(x => x.ClassId)
            .GreaterThan(0).WithMessage("الصف غير صالح")
            .When(x => x.ClassId.HasValue);

        RuleFor(x => x.Image)
            .Must(img => img == null || img.Length <= 5 * 1024 * 1024)
            .WithMessage("حجم الصورة يجب أن لا يتجاوز 5 ميجابايت")
            .Must(img => img == null || new[] { ".jpg", ".jpeg", ".png", ".webp" }
                .Contains(Path.GetExtension(img.FileName).ToLower()))
            .WithMessage("صيغة الصورة يجب أن تكون jpg أو png أو webp");
    }
}
