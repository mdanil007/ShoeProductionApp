
namespace ShoeProductionApp.Authorization;

public static class AppRoles
{
    public const string Manager = "Руководитель";
    public const string Master = "Мастер";
    public const string Viewer = "Менеджер";
    public const string Administrator = "Администратор";

    public static readonly string[] All =
    [
        Manager,
        Master,
        Viewer,
        Administrator
    ];

    public const string CanManageOrders = Manager + "," + Master;
    public const string CanCancelOrders = Manager;
    public const string CanManageClients = Manager;
    public const string CanManageReferenceData = Manager + "," + Master;
    public const string CanManageUsers = Administrator;
}
