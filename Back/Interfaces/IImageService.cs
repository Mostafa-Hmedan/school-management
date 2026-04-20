namespace Back.Services.Interfaces;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile image, string folderName);
    bool DeleteImage(string imagePath);
    bool IsValidImage(IFormFile file);
    string GetFullPath(string imagePath);
}