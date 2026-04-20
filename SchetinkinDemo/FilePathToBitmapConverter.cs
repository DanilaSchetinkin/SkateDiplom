using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Globalization;
using System.IO;

namespace SchetinkinDemo
{
    /// <summary>
    /// Конвертирует путь к файлу (string) в Bitmap.
    /// Поддерживает абсолютные пути и имена файлов относительно папок Images/ProductImages рядом с exe.
    /// </summary>
    public class FilePathToBitmapConverter : IValueConverter
    {
        public static readonly FilePathToBitmapConverter Instance = new();

        private static readonly string BaseDir = AppContext.BaseDirectory;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string raw || string.IsNullOrWhiteSpace(raw))
                return null;

            var resolved = Resolve(raw);
            if (resolved == null)
                return null;

            try
            {
                return new Bitmap(resolved);
            }
            catch
            {
                return null;
            }
        }

        private static string? Resolve(string path)
        {
            // 1. Абсолютный путь — используем как есть
            if (Path.IsPathRooted(path) && File.Exists(path))
                return path;

            // Имя файла (или относительный путь) — ищем в нескольких папках
            var fileName = Path.GetFileName(path);

            // 2. Папки рядом с exe, которые могут содержать картинки
            string[] searchFolders =
            {
                Path.Combine(BaseDir, "Image"),
                Path.Combine(BaseDir, "Images"),
                Path.Combine(BaseDir, "ProductImages"),
                BaseDir,
            };

            foreach (var folder in searchFolders)
            {
                // Пробуем полный относительный путь
                var candidate1 = Path.Combine(BaseDir, path);
                if (File.Exists(candidate1)) return candidate1;

                // Пробуем только имя файла в папке
                var candidate2 = Path.Combine(folder, fileName);
                if (File.Exists(candidate2)) return candidate2;
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
