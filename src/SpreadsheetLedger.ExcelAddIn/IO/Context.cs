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

        public Context()
        {
            var wb = Globals.ThisAddIn.Application.ActiveWorkbook;

            GL = new RecordService<GLRecord>(FindListObject(wb, "GL"));
            Journal = new RecordService<JournalRecord>(FindListObject(wb, "JournalTable"));
            Prices = new RecordService<PriceRecord>(FindListObject(wb, "PriceTable"));
            CoA = new RecordService<AccountRecord>(FindListObject(wb, "CoBATable"));
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
