namespace Buratino.Models.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    sealed class TitleAttribute : Attribute
    {
        private string title;
        private string describtion;
        private string subject = null;
        private string handler;

        public TitleAttribute(string title)
        {
            Title = title;
            Description = title;
            Subject = null;
            Handler = null;
        }
        public TitleAttribute(string title, string describtion)
        {
            Title = title;
            Description = describtion;
            Subject = null;
            Handler = null;
        }
        public TitleAttribute(string title, string describtion, string subject, string handler)
        {
            Title = title;
            Description = describtion;
            Subject = subject;
            Handler = handler;
        }

        public string Title { get => title; set => title = value; }
        public string Description { get => describtion; set => describtion = value; }

        /// <summary>
        /// Таблица 2-ой сущности
        /// </summary>
        public string Subject { get => subject; set => subject = value; }

        /// <summary>
        /// Псевдо поле
        /// </summary>
        public string Handler { get => handler; set => handler = value; }
    }
}