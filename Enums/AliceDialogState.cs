namespace Bronya.Enums
{
    public enum AliceDialogState
    {
        /// <summary>
        /// Не понятно что он хочет
        /// </summary>
        None = 0,

        /// <summary>
        /// Хочет послушать помощь
        /// </summary>
        Help,

        /// <summary>
        /// Возможно хочет выйти
        /// </summary>
        Exit,

        /// <summary>
        /// Предлогаем забронировать стол
        /// </summary>
        AskIntent,

        /// <summary>
        /// Спрашиваем количество гостей
        /// </summary>
        AskSeatAmount,

        /// <summary>
        /// Спрашиваем время
        /// </summary>
        AskTime,

        /// <summary>
        /// Спрашиваем как его зовут
        /// </summary>
        AskName,

        /// <summary>
        /// Проверяем как его зовут
        /// </summary>
        CheckName,

        /// <summary>
        /// Проверяем время
        /// </summary>
        CheckTime,
    }
}