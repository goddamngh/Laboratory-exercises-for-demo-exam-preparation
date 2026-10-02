using System.Windows;
using Microsoft.EntityFrameworkCore;
using RoleRouter.Data;

namespace RoleRouter.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var login = LoginInput.Text;
        var password = PasswordInput.Password;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Заполните все поля", "Ошибка входа",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var context = new RoleRouterDbContext();

        var user = context.Users
            .Include(u => u.Role)
            .FirstOrDefault(u => u.Login == login && u.Password == password);

        if (user is null)
        {
            MessageBox.Show("Пользователь с введёнными данными не найден",
                "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var roleWindow = new RoleWindow(user);
        roleWindow.Show();
        Close();
    }
}