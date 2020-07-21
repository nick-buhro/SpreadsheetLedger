using Microsoft.Office.Interop.Excel;
using SpreadsheetLedger.Core;
using SpreadsheetLedger.Core.Helpers;
using System;
using System.Collections.Generic;

namespace SpreadsheetLedger.ExcelAddIn.IO
{
    internal sealed class RecordService<T> : IRecordService<T>
        where T : new()
    {
        private readonly ListObject _list;

        public RecordService(ListObject list)
        {
            _list = list;
        }

        public T[] Read()
        {
            return RecordHelper.Read<T>(
                _list.HeaderRowRange.Value,
                _list.DataBodyRange.Value);
        }

        public void Write(IList<T> data, bool overwrite = false)
        {
            if (_list.ShowTotals)
                throw new Exception($"List '{_list.DisplayName}' contains total row (not supported).");

            if (overwrite || (_list.DataBodyRange == null))
            {
                //var ws = (Worksheet)_list.Parent;
                //ws.ShowAllData();

                if (_list.DataBodyRange == null)
                    _list.ListRows.AddEx();
                else
                    _list.DataBodyRange.ClearContents();

                var newDataRange = _list
                    .Range
                    .Resize[(data.Count == 0) ? 2 : (1 + data.Count), _list.DataBodyRange.Columns.Count];

                _list.Resize(newDataRange);


                if (data.Count > 0)
                {
                    newDataRange = newDataRange
                        .Offset[1, 0]
                        .Resize[data.Count, _list.DataBodyRange.Columns.Count];

                    var value = _list.DataBodyRange.Value;
                    RecordHelper.Write(_list.HeaderRowRange.Value, value, data);
                    newDataRange.Value = value;
                }
            }
            else if (data.Count > 0)
            {
                var existingSize = _list.DataBodyRange.Rows.Count;
                var newDataRange = _list
                    .Range
                    .Resize[1 + existingSize + data.Count, _list.DataBodyRange.Columns.Count];

                _list.Resize(newDataRange);

                var appendRange = _list
                    .DataBodyRange
                    .Offset[existingSize, 0]
                    .Resize[data.Count, _list.DataBodyRange.Columns.Count];

                var value = appendRange.Value;
                RecordHelper.Write(_list.HeaderRowRange.Value, value, data);
                appendRange.Value = value;
            }
        }
    }
}
