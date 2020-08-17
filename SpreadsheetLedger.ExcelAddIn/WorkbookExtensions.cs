using Microsoft.Office.Interop.Excel;
using System;
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

        public static IEnumerable<ListObject> FindListObjects(this Workbook wb, Predicate<string> nameFilter)
        {
            foreach (Worksheet ws in wb.Worksheets)
            {
                foreach (ListObject lo in ws.ListObjects)
                {
                    if (nameFilter(lo.Name))
                        yield return lo;
                }
            }
        }
    }
}
