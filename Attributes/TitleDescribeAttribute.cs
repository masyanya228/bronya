namespace Bronya.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    sealed class TitleAttribute : Attribute
    {
        private string title;
        private string describtion;

        public TitleAttribute(string title)
        {
            Title = title;
            Description = title;
        }
        public TitleAttribute(string title, string describtion)
        {
            Title = title;
            Description = describtion;
        }

        public string Title { get => title; set => title = value; }
        public string Description { get => describtion; set => describtion = value; }
    }
}