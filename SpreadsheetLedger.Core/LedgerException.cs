using System;

namespace SpreadsheetLedger.Core
{
    public sealed class LedgerException : Exception
    {
        public LedgerException(string message)
            : base(message) { }

        public LedgerException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
