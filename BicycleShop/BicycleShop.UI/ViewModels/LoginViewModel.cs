using BicycleShop.Core.Session;
using BicycleShop.Data.Context;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BicycleShop.UI.ViewModels;

public partial class LoginViewModel: ObservableObject
{
    [ObservableProperty]
    private string _login = string.Empty;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public event Action? OnLoginSuccess;

    [ObservableProperty]
    private string _password = string.Empty;

    [RelayCommand]
    private void LoginUser() // Убрали параметр
    {
        ErrorMessage = string.Empty;

        // Теперь проверяем свойство Password
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите логин и пароль!";
            return;
        }

        try
        {
            using var context = new BicycleShopDbContext();
        
            var user = context.Users.FirstOrDefault(u => 
                u.Password == Password && // Используем свойство
                u.Login == Login);
            
            if (user == null)
            {
                ErrorMessage = "Неверный логин или пароль!";
                return;
            }

            var role = context.Roles.FirstOrDefault(r => r.Id == user.RoleId)?.RoleName ?? "Гость";
        
            UserSession.SetUser(user.Id, user.Fio, role);
            OnLoginSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}"; 
        }
    }

    [RelayCommand]
    private void LoginAsGuest()
    {
        UserSession.ClearUser();
        OnLoginSuccess?.Invoke();
    }
    
}