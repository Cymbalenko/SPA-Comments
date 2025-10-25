using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helper;

public static class FileHelper
{
    private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif"
    };

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif"
    };

    private static readonly HashSet<string> AllowedTextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt"
    };
    private const long MaxTextFileSizeBytes = 100 * 1024; // 100 KB

    /// <summary>
    /// Проверяет, является ли файл текстовым (.txt).
    /// </summary>
    public static bool IsTextFile(IFormFile file)
    {
        string extension = Path.GetExtension(file.FileName);
        return AllowedTextExtensions.Contains(extension);
    }

    /// <summary>
    /// Проверяет, превышает ли размер текстового файла лимит 100 KB.
    /// </summary>
    public static bool IsTextFileTooLarge(IFormFile file)
    {
        if (!IsTextFile(file))
            return false;

        return file.Length > MaxTextFileSizeBytes;
    }

    /// <summary>
    /// Универсальная проверка — можно ли принимать файл (JPG, PNG, GIF, TXT ≤ 100KB).
    /// </summary>
    public static bool IsValidFile(IFormFile file, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (file == null || file.Length == 0)
        {
            errorMessage = "File is empty.";
            return false;
        }

        var (name, ext, type, isImage) = GetFileInfo(file);

        // Проверка картинки
        if (isImage)
            return true;

        // Проверка текстового файла
        if (IsTextFile(file))
        {
            if (IsTextFileTooLarge(file))
            {
                errorMessage = "Text file size must not exceed 100 KB.";
                return false;
            }

            return true;
        }

        errorMessage = "Unsupported file format. Allowed formats: JPG, PNG, GIF, TXT.";
        return false;
    }


    public static (string FileName, string Extension, string ContentType, bool IsImage) GetFileInfo(IFormFile file)
    {
        string fileName = Path.GetFileName(file.FileName);
        string extension = Path.GetExtension(fileName);
        string contentType = file.ContentType;

        bool isImage = AllowedImageContentTypes.Contains(contentType)
                       && AllowedImageExtensions.Contains(extension);

        return (fileName, extension, contentType, isImage);
    }
}

