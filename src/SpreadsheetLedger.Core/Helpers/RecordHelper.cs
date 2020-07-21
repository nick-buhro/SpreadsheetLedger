using SpreadsheetLedger.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpreadsheetLedger.Core.Helpers
{
    public static class RecordHelper
    {
        public static void Write<T>(object[,] header, object[,] data, IList<T> source)
        {
            Trace.Assert(header != null);
            Trace.Assert(header.GetLength(0) == 1);
            Trace.Assert(data != null);
            Trace.Assert(data.GetLength(0) == source.Count);

            if ((source == null) || (source.Count == 0))
                return;

            var index = Enumerable
                .Range(header.GetLowerBound(1), header.GetLength(1))
                .ToDictionary(i => header[header.GetLowerBound(0), i]);

            var t = typeof(T);
            foreach (var pi in t.GetProperties())
            {
                var columnName = pi.GetCustomAttributes(false).OfType<NameAttribute>().FirstOrDefault()?.Name ?? pi.Name;
                if (index.TryGetValue(columnName, out var columnIndex))
                {
                    for (var i = 0; i < source.Count; i++)
                    {
                        var value = pi.GetValue(source[i]);
                        data[data.GetLowerBound(0) + i, columnIndex] = value;
                    }
                }
                else
                {
                    throw new Exception($"{t.Name} table header doesn't contain '{columnName}' column.");
                }
            }
        }

        public static T[] Read<T>(object[,] header, object[,] data)
            where T : new()
        {
            Trace.Assert(header != null);
            Trace.Assert(header.GetLength(0) == 1);
            Trace.Assert(data != null);

            var t = typeof(T);
            var h = header.GetLowerBound(0);
            var firstRowIndex = data.GetLowerBound(0);
            var lastRowIndex = data.GetUpperBound(0);
            var result = new T[1 + lastRowIndex - firstRowIndex];
            for (var i = 0; i < result.Length; i++)
            {
                var record = new T();

                foreach (var pi in t.GetProperties())
                {
                    var columnName = pi.GetCustomAttributes(false).OfType<NameAttribute>().FirstOrDefault()?.Name ?? pi.Name;

                    for (var c = header.GetLowerBound(1); c <= header.GetUpperBound(1); c++)
                    {
                        if (columnName == (string)header[h, c])
                        {
                            if ((pi.PropertyType == typeof(decimal)) || (pi.PropertyType == typeof(decimal?)))
                            {
                                var value = data[i + firstRowIndex, c];
                                pi.SetValue(record, ToDecimal(value));                                
                            }
                            else if (pi.PropertyType == typeof(string))
                            {
                                var value = data[i + firstRowIndex, c];
                                pi.SetValue(record, value?.ToString());
                            }
                            else
                            {
                               pi.SetValue(record, data[i + firstRowIndex, c]);                               
                            }
                            break;
                        }
                    }

                    //throw new KeyNotFoundException($"Column '{columnName}' not found in the table.");
                }

                result[i] = record;
            }

            return result;
        }

        private static decimal? ToDecimal(object value)
        {
            if (value == null)
                return null;

            if ((value as string)?.Trim() == "")
                return null;

            return Convert.ToDecimal(value);
        }
    }
}
