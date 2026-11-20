using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using BicycleShop.UI.ViewModels;
using Microsoft.Win32;

namespace BicycleShop.UI;

public partial class ProductEditWindow : Window
{
    public ProductEditWindow(ProductEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void SelectPhoto_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
            CheckPathExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            // Проверка размера (300x200)
            var bitmap = new BitmapImage(new Uri(dialog.FileName));
            if (bitmap.PixelWidth > 300 || bitmap.PixelHeight > 200)
            {
                MessageBox.Show("Размер изображения не должен превышать 300x200 пикселей!", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (DataContext is ProductEditViewModel vm)
            {
                vm.SetImage(new FileInfo(dialog.FileName));
            }
        }
    }
}