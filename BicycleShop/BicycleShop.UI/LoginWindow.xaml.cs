using System.Windows;
using BicycleShop.UI.ViewModels;

namespace BicycleShop.UI;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();

        var vm = new LoginViewModel();
        DataContext = vm;

        vm.OnLoginSuccess += () =>
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        };
    }
}