using System.Collections.Generic;

namespace SpreadsheetLedger.Core
{
    public interface ISerializer
    {
        T[] Read<T>(object[,] header, object[,] data)
            where T : new();

        void Write<T>(object[,] header, object[,] data, IList<T> values);
    }
}
