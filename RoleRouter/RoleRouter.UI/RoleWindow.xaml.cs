using System.Windows;
using RoleRouter.Data.Entities;

namespace RoleRouter.UI;

/// <summary>
/// Окно личного кабинета.
/// Отображает приветствие и текущую роль пользователя.
/// Одно окно для всех ролей — содержимое зависит от переданного <see cref="User"/>.
/// </summary>
public partial class RoleWindow : Window
{
    public RoleWindow(User user)
    {
        InitializeComponent();
        WelcomeText.Text = $"Добро пожаловать, {user.FullName}!";
        RoleText.Text = $"Вы вошли как: {user.Role?.Name ?? "неизвестная роль"}";
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        new MainWindow().Show();
        Close();
    }
}