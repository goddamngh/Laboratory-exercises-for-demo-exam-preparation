using System.Windows;
using RoleRouter.Data.Entities;

namespace RoleRouter.UI;

public partial class RoleWindow : Window
{
    public RoleWindow(User user)
    {
        InitializeComponent();
        WelcomeText.Text = $"Добро пожаловать, {user.FullName}!";
        RoleText.Text = $"Роль: {user.Role?.Name}";
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        new MainWindow().Show();
        Close();
    }
}