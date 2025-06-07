using System;
using System.Collections.Generic;

namespace Jinqik.D365.Exceptions
{
    public class BusinessException : Exception
    {
        public string MessageCode { get; protected set; }
        public List<string> Parameters { get; protected set; } = new List<string>();

        public BusinessException(string messageCode, params string[] arguments) : base(messageCode)
        {
            this.MessageCode = messageCode;
            Parameters.AddRange(arguments);
        }
    }
}