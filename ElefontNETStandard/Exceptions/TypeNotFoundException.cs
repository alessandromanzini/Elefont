using System;
namespace ElefontNETStandard.Exceptions
{
    public class TypeNotFoundException : Exception
    {
        public TypeNotFoundException() : base() { }
        public TypeNotFoundException(string message) : base(message) { }
    }
}

