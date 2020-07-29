using System;

namespace SpreadsheetLedger.Core.Logic
{
    public sealed class LedgerException: Exception
    {
        public LedgerException(string message)
            : base(message) { }

        public LedgerException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
