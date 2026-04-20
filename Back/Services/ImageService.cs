

using Back.Services.Interfaces;

namespace Back.Services.Image;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _environment;

    public ImageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveImageAsync(IFormFile image, string folderName)
    {
        // التحقق من صحة الصورة
        if (!IsValidImage(image))
            throw new ArgumentException("الصورة غير صالحة أو حجمها كبير");

        // إنشاء مجلد التخزين
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folderName);
        Directory.CreateDirectory(uploadsFolder);

        // اسم الملف
        var fileName = image.FileName;
        var filePath = Path.Combine(uploadsFolder, fileName);

        // إذا كانت موجودة، أضيف timestamp
        if (File.Exists(filePath))
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            fileName = $"{fileNameWithoutExtension}_{DateTime.Now.Ticks}{extension}";
            filePath = Path.Combine(uploadsFolder, fileName);
        }

        // حفظ الملف
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        // إرجاع المسار النسبي
        return Path.Combine("uploads", folderName, fileName).Replace("\\", "/");
    }

    public bool DeleteImage(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return false;

        var fullPath = Path.Combine(_environment.WebRootPath, imagePath);

        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    public bool IsValidImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        // الحجم الأقصى 5MB
        if (file.Length > 5 * 1024 * 1024)
            return false;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        return allowedExtensions.Contains(extension);
    }

    public string GetFullPath(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return string.Empty;

        return Path.Combine(_environment.WebRootPath, imagePath);
    }
}