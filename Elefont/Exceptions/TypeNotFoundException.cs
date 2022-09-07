using System;
namespace Elefont.Exceptions
{
    public class TypeNotFoundException : Exception
    {
        public TypeNotFoundException() : base() { }
        public TypeNotFoundException(string message) : base(message) { }
        public TypeNotFoundException(string message, Exception? innerException) : base(message, innerException) { }
    }
}

