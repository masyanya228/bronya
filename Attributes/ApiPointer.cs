namespace Bronya.Attributes
{
    [AttributeUsage(AttributeTargets.Method| AttributeTargets.Field)]
    public class ApiPointer : Attribute
    {
        public string[] Pointers { get => _pointers; set => _pointers = value; }
        private string[] _pointers;

        public ApiPointer(params string[] pointers)
        {
            _pointers = pointers;
        }
    }
}