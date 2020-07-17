using SpreadsheetLedger.Core;
using SpreadsheetLedger.Core.Models;

namespace SpreadsheetLedger.Helpers
{
    internal sealed class Context : IContext
    {
        public string BaseCommodity => "USD";

        public IRecordService<GLRecord> GL { get; }

        public IRecordService<JournalRecord> Journal { get; }

        public IRecordService<PriceRecord> Prices { get; }

        public IRecordService<AccountRecord> CoA { get; }

        public IRecordService<PLCategoryRecord> CoPL { get; }

        public Context()
        {
            GL = new RecordService<GLRecord>(Globals.GLSheet.ListObjects[1]);
            Journal = new RecordService<JournalRecord>(Globals.JournalSheet.ListObjects[1]);
            Prices = new RecordService<PriceRecord>(Globals.PriceSheet.ListObjects[1]);
            CoA = new RecordService<AccountRecord>(Globals.CoaSheet.ListObjects[1]);
            CoPL = new RecordService<PLCategoryRecord>(Globals.CoPLSheet.ListObjects[1]);
        }
    }
}
