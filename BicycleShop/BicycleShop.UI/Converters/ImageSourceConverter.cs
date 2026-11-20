using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BicycleShop.UI.Converters;

public class ImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string? fileName = value as string;
        
        // Если путь пустой, возвращаем заглушку
        if (string.IsNullOrWhiteSpace(fileName))
            return new BitmapImage(new Uri("pack://application:,,,/Images/picture.png"));

        // Формируем абсолютный путь к файлу в папке bin
        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", fileName);

        if (!File.Exists(fullPath))
            return new BitmapImage(new Uri("pack://application:,,,/Images/picture.png"));

        try
        {
            // Создаем BitmapImage с опцией OnLoad, чтобы файл не блокировался на диске
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.EndInit();
            return bitmap;
        }
        catch
        {
            return new BitmapImage(new Uri("pack://application:,,,/Images/picture.png"));
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}