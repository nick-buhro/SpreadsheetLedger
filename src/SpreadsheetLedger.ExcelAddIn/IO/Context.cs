using Microsoft.Office.Interop.Excel;
using SpreadsheetLedger.Core;
using SpreadsheetLedger.Core.Models;
using System.Collections.Generic;

namespace SpreadsheetLedger.ExcelAddIn.IO
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
            var wb = Globals.ThisAddIn.Application.ActiveWorkbook;

            GL = new RecordService<GLRecord>(FindListObject(wb, "GL"));
            Journal = new RecordService<JournalRecord>(FindListObject(wb, "Journal"));
            Prices = new RecordService<PriceRecord>(FindListObject(wb, "Price"));
            CoA = new RecordService<AccountRecord>(FindListObject(wb, "CoA"));
            CoPL = new RecordService<PLCategoryRecord>(FindListObject(wb, "CoPL"));
        }

        private static ListObject FindListObject(Workbook wb, string listName)
        {            
            foreach (Worksheet ws in wb.Worksheets)
            {
                foreach (ListObject lo in ws.ListObjects)
                {
                    if (lo.Name == listName)
                        return lo;
                }
            }
            throw new KeyNotFoundException($"Table '{listName}' not found in '{wb.Name}'.");
        }
    }
}
