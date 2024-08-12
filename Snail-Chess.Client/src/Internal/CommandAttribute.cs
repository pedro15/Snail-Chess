using System;

namespace SnailChess.Client.Internal
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        public readonly string name;
        public readonly string description;
        
        public CommandAttribute(string _name)
        {
            name = _name;
            description = string.Empty;
        }

        public CommandAttribute(string _name , string _description)
        {
            name = _name;
            description = _description;
        }
    }
}