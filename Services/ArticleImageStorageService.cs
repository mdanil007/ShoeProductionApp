using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace ShoeProductionApp.Services;

public class ArticleImageStorageService(IWebHostEnvironment environment)
{
    private const long MaxFileSize = 5 * 1024 * 1024;
    private readonly string _directory = Path.Combine(environment.ContentRootPath, "App_Data", "Uploads", "Articles");

    public async Task<string> SaveAsync(IFormFile file)
    {
        if (file.Length <= 0 || file.Length > MaxFileSize)
            throw new ArgumentException("Размер фотографии должен быть от 1 байта до 5 МБ.");

        await using var input = file.OpenReadStream();
        var header = new byte[12];
        var read = await input.ReadAsync(header);
        var extension = DetectExtension(header, read);
        if (extension is null)
            throw new ArgumentException("Допустимые форматы фотографий: JPEG, PNG и WebP.");

        Directory.CreateDirectory(_directory);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(_directory, fileName);
        input.Position = 0;
        await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await input.CopyToAsync(output);
        return fileName;
    }

    public void Delete(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName)) return;
        var path = Path.Combine(_directory, fileName);
        if (File.Exists(path)) File.Delete(path);
    }

    private static string? DetectExtension(byte[] h, int n)
    {
        if (n >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return ".jpg";
        if (n >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47 && h[4] == 0x0D && h[5] == 0x0A && h[6] == 0x1A && h[7] == 0x0A) return ".png";
        if (n >= 12 && h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46 && h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50) return ".webp";
        return null;
    }
}
