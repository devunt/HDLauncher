﻿using System;
using System.Windows.Controls;

namespace HDLauncher
{
    class FFException : Exception
    {
        public string Description { get; }
        public Control Cause { get; }

        public FFException(string message) : base(message) { }

        public FFException(string message, Control cause) : this(message)
        {
            Cause = cause;
        }

        public FFException(string message, string description) : this(message)
        {
            Description = description;
        }

        public FFException(string message, string description, Control cause) : this(message, description)
        {
            Cause = cause;
        }
    }
}
