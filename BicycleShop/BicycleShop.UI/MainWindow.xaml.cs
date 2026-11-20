using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BicycleShop.Core.Session;
using BicycleShop.UI.ViewModels;

namespace BicycleShop.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        DataContext = new MainViewModel();
        UserInfoText.Text = $"{UserSession.Fio} ({UserSession.RoleName})";
    }

    private void EditProduct_ClicK(object sender, MouseButtonEventArgs e)
    {
        // Проверяем, что клик был именно по карточке товара
        if (sender is FrameworkElement element && element.DataContext is ProductViewModel productVm)
        {
            // Проверяем, что текущий пользователь — Администратор
            if (UserSession.RoleName == "Администратор")
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.EditProductCommand.Execute(productVm);
                }
            }
        }
    }
}