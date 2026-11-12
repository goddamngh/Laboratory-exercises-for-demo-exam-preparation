namespace BicycleShop.Core.Session;

public static class UserSession
{
    public static int? UserId { get; set; }
    public static string Fio { get; set; } = "Гость";
    public static string Name { get; set; } = "Гость";

    public static void SetUser(int id, string fio, string name)
    {
        UserId = id;
        Fio = fio;
        Name = name;
    }

    public static void ClearUser()
    {
        UserId = null;
        Fio = "Гость";
        Name = "Гость";
    }
}