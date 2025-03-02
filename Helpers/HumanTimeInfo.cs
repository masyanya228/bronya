namespace Bronya.Helpers
{
    public class HumanTimeInfo
    {
        /// <summary>
        /// i-1 короткие 3-х или 4-х буквенные
        /// </summary>
        public static string[] _Months = new string[] { "янв", "фев", "март", "апр", "май", "июнь", "июль", "авг", "сен", "окт", "ноя", "дек" };
        /// <summary>
        /// i-1 полные инфинитив
        /// </summary>
        public static string[] _MonthsFull = "январь,февраль,март,апрель,май,июнь,июль,август,сентябрь,октябрь,ноябрь,декабрь".Split(',');
        /// <summary>
        /// i-1 полные родительный
        /// </summary>
        public static string[] _MonthsFull2 = "января,февраля,марта,апреля,мая,июня,июля,августа,сентября,октября,ноября,декабря".Split(',');
        public string Title { get; set; }
    }
}