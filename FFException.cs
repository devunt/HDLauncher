using System;
using System.Windows.Controls;

namespace HDLauncher
{
    class FFException : Exception
    {
        public Control Cause { get; }

        public FFException(string message) : base(message) { }

        public FFException(string message, Control cause) : base(message)
        {
            Cause = cause;
        }
    }
}
