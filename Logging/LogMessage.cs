using System;

#pragma warning disable IDE0079
#pragma warning disable CS8618

namespace BSS.Logging
{
    internal ref struct LogMessage
    {
        [Obsolete("Empty Constructor not supported.", true)]
        public LogMessage() { }

        internal LogMessage(String message, LogSeverity severity, String source)
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