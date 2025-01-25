using System;

namespace Buratino.Models.Attributes
{

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class PermissionAttribute : Attribute
    {
        private string permission;

        public PermissionAttribute(string permission)
        {
            Permission = permission;
        }

        public string Permission { get => permission; set => permission = value; }
    }
}