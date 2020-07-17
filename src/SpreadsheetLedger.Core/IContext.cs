using SpreadsheetLedger.Core.Models;

namespace SpreadsheetLedger.Core
{
    public interface IContext
    {
        IRecordService<GLRecord> GL { get; }

        IRecordService<JournalRecord> Journal { get; }

        IRecordService<PriceRecord> Prices { get; }

        IRecordService<AccountRecord> CoA { get; }

        IRecordService<PLCategoryRecord> CoPL { get; }


        string BaseCommodity { get; }
    }
}
