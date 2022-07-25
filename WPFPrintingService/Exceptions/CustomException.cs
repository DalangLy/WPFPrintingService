using System;

namespace WPFPrintingService
{
    internal class CustomException : Exception
    {
        public CustomException(String message) : base(message)
        {

        }
    }
}
