using Microsoft.Office.Interop.Excel;
using SpreadsheetLedger.Core.Helpers;
using SpreadsheetLedger.Core.Logic;
using System.Collections.Generic;

namespace SpreadsheetLedger.ExcelAddIn
{
    public static class ListObjectExtensions
    {
        public static T[] Read<T>(this ListObject lo)
            where T: new()
        {
            return Serializer.Read<T>(
                lo.HeaderRowRange.Value,
                lo.DataBodyRange.Value);
        }

        public static void Write<T>(this ListObject lo, IList<T> data, bool overwrite = false)
        {
            if (lo.ShowTotals)
                throw new LedgerException($"List '{lo.DisplayName}' contains total row (not supported).");

            if (overwrite || (lo.DataBodyRange == null))
            {
                //var ws = (Worksheet)_list.Parent;
                //ws.ShowAllData();

                if (lo.DataBodyRange == null)
                    lo.ListRows.AddEx();
                else
                    lo.DataBodyRange.ClearContents();

                var newDataRange = lo
                    .Range
                    .Resize[(data.Count == 0) ? 2 : (1 + data.Count), lo.DataBodyRange.Columns.Count];

                lo.Resize(newDataRange);


                if (data.Count > 0)
                {
                    newDataRange = newDataRange
                        .Offset[1, 0]
                        .Resize[data.Count, lo.DataBodyRange.Columns.Count];

                    var value = lo.DataBodyRange.Value;
                    Serializer.Write(lo.HeaderRowRange.Value, value, data);
                    newDataRange.Value = value;
                }
            }
            else if (data.Count > 0)
            {
                var existingSize = lo.DataBodyRange.Rows.Count;
                var newDataRange = lo
                    .Range
                    .Resize[1 + existingSize + data.Count, lo.DataBodyRange.Columns.Count];

                lo.Resize(newDataRange);

                var appendRange = lo
                    .DataBodyRange
                    .Offset[existingSize, 0]
                    .Resize[data.Count, lo.DataBodyRange.Columns.Count];

                var value = appendRange.Value;
                Serializer.Write(lo.HeaderRowRange.Value, value, data);
                appendRange.Value = value;
            }
        }
    }
}
