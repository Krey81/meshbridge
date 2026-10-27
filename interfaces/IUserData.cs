namespace meshbridge.interfaces
{
    /// <summary>
    /// Интерфейс для пометки элементов, содержащих данные пользователя
    /// </summary>
    /// <remarks>используется для совмещения входных структур с именем ровера из топика mqtt</remarks>
    public interface IUserData
    {
        /// <summary>
        /// Логин, ник, наименование ровера
        /// </summary>
        string UserName { get; set; }
    }
}
