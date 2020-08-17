﻿using Microsoft.Office.Interop.Excel;
using SpreadsheetLedger.Core;
using SpreadsheetLedger.Core.Impl;
using System.Collections.Generic;

namespace SpreadsheetLedger.ExcelAddIn
{
    public static class ListObjectExtensions
    {
        private static readonly ISerializer _serializer = new Serializer();

        public static T[] Read<T>(this ListObject lo)
            where T: new()
        {
            if (lo.DataBodyRange == null)
                return new T[0];

            return _serializer.Read<T>(
                lo.HeaderRowRange.Value,
                lo.DataBodyRange.Value);
        }

        public static IEnumerable<T> ReadLazy<T>(this ListObject lo)
            where T: new()
        {
            if (lo.DataBodyRange == null)
                return new T[0];

            return new LazyRecordCollection<T>(
                lo.HeaderRowRange.Value,
                lo.DataBodyRange.Value,
                _serializer);
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
                    _serializer.Write(lo.HeaderRowRange.Value, value, data);
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
                _serializer.Write(lo.HeaderRowRange.Value, value, data);
                appendRange.Value = value;
            }
        }
    }
}
