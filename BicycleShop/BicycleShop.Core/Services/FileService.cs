using System;
using System.IO;

namespace BicycleShop.Core.Services;

public static class FileService
{
    // Возвращает путь к папке Images в корне проекта (для сохранения исходников)
    public static string GetProjectImagesPath()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        // Поднимаемся из bin/Debug/net10.0-windows в корень проекта BicycleShop.UI
        string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\"));
        string imagesPath = Path.Combine(projectRoot, "Images");

        // Если папки нет (например, при запуске .exe вне Rider), используем папку рядом с .exe
        if (!Directory.Exists(imagesPath))
        {
            imagesPath = Path.Combine(baseDir, "Images");
            Directory.CreateDirectory(imagesPath);
        }
        return imagesPath;
    }

    // Возвращает путь к папке Images рядом с .exe (для отображения в UI)
    public static string GetBinImagesPath()
    {
        string binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
        if (!Directory.Exists(binPath))
            Directory.CreateDirectory(binPath);
        return binPath;
    }

    // Удаляет файл из обеих папок (корень и bin)
    public static void DeleteImage(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return;

        string projectPath = Path.Combine(GetProjectImagesPath(), fileName);
        string binPath = Path.Combine(GetBinImagesPath(), fileName);

        try
        {
            if (File.Exists(projectPath)) File.Delete(projectPath);
            if (File.Exists(binPath)) File.Delete(binPath);
        }
        catch (IOException ex)
        {
            // Игнорируем ошибку, если файл занят (например, открыт в просмотрщике)
            Console.WriteLine($"Не удалось удалить файл {fileName}: {ex.Message}");
        }
    }

    // Копирует выбранный файл в обе папки и возвращает новое имя файла
    public static string SaveImage(FileInfo sourceFile, string newFileName)
    {
        string projectPath = Path.Combine(GetProjectImagesPath(), newFileName);
        string binPath = Path.Combine(GetBinImagesPath(), newFileName);

        // Копируем в корень проекта (для сохранения в исходниках)
        File.Copy(sourceFile.FullName, projectPath, true);
        // Копируем в bin (чтобы приложение сразу увидело изменения без пересборки)
        File.Copy(sourceFile.FullName, binPath, true);

        return newFileName;
    }
}