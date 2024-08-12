namespace AllUp.Extensions;



public static partial class Extension
{
    public static bool IsImage(this IFormFile formFile) => formFile.ContentType.Contains("image");
    public static bool DoesSizeExceed(this IFormFile formFile, int size) => formFile.Length / 1024 > size;
    public static async Task<string> SaveFileAsync(this IFormFile formFile, string? rootFolder = null)
    {
        string filename = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
        string filePath;
        if (rootFolder == null)
        {
            filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "images", filename);
        }
        else
        {
            filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "images", rootFolder, filename);
        }
        using FileStream fileStream = new(filePath, FileMode.Create);
        await formFile.CopyToAsync(fileStream);
        return filename;
    }
}
