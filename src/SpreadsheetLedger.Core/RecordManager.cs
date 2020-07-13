using SpreadsheetLedger.Core.Models;
using SpreadsheetLedger.Core.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpreadsheetLedger.Core
{
    public static class RecordManager
    {
        public static void Write<T>(this IList<T> source, object[,] header, object[,] data)
            where T: Record
        {
            Trace.Assert(header != null);
            Trace.Assert(header.GetLength(0) == 1);
            Trace.Assert(data != null);
            Trace.Assert(data.GetLength(0) == source.Count);

            throw new NotImplementedException();
        }

        public static List<T> Read<T>(object[,] header, object[,] data)
            where T : Record, new()
        {
            Trace.Assert(header != null);
            Trace.Assert(header.GetLength(0) == 1);
            Trace.Assert(data != null);

            var t = typeof(T);
            var h = header.GetLowerBound(0);
            var lastRowIndex = data.GetUpperBound(0);
            var result = new List<T>(lastRowIndex);
            for (var rowIndex = data.GetLowerBound(0); rowIndex <= lastRowIndex; rowIndex++)
            {
                var record = new T();

                foreach (var pi in t.GetProperties())
                {
                    var columnName = pi.GetCustomAttributes(false).OfType<NameAttribute>().FirstOrDefault()?.Name ?? pi.Name;

                    for (var c = header.GetLowerBound(1); c <= header.GetUpperBound(1); c++)
                    {
                        if (columnName == (string)header[h, c])
                        {
                            pi.SetValue(record, data[rowIndex, c]);
                            break;
                        }
                    }

                    //throw new KeyNotFoundException($"Column '{columnName}' not found in the table.");
                }

                result.Add(record);
            }

            return result;
        }
    }
}
