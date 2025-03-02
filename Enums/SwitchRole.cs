using Bronya.Attributes;

namespace Bronya.Enums
{
    /// <summary>
    /// Для переключения ролей в режиме отладки
    /// </summary>
    public enum RoleType
    {
        [Title("Гость")]
        Costumer = 0,

        [Title("Хостес")]
        Hostes,

        [Title("Администратор")]
        Administrator,
    }
}
