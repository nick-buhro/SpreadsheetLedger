using Microsoft.Office.Interop.Excel;
using System.Collections.Generic;

namespace SpreadsheetLedger.ExcelAddIn
{
    public static class WorkbookExtensions
    {
        public static ListObject FindListObject(this Workbook wb, string listName)
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
