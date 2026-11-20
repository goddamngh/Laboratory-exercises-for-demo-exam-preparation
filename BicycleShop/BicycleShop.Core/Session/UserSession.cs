namespace BicycleShop.Core.Session;

public static class UserSession
{
    public static int? UserId { get; set; }
    public static string? Fio { get; set; } = string.Empty;
    public static string RoleName { get; set; } = "Гость";

    public static void SetUser(int id, string fio, string name)
    {
        UserId = id;
        Fio = fio;
        RoleName = name;
    }

    public static void ClearUser()
    {
        UserId = null;
        Fio = string.Empty;
        RoleName = "Гость";
    }
}