using System;

#pragma warning disable CS8618
#pragma warning disable IDE0079

namespace BSS.Logging
{
    internal ref struct LOG_MESSAGE
    {
        [Obsolete("Empty Constructor not supported.", true)]
        public LOG_MESSAGE() { }

        internal LOG_MESSAGE(String message, LogSeverity severity, String source)
        {
            Message = message;
            Severity = severity;
            Source = source;
        }

        internal LogSeverity Severity;

        internal String Source;
        internal String Message;
    }
}