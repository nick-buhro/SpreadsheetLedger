using System.Collections.Generic;

namespace SpreadsheetLedger.Core
{
    public interface IRecordService<T>
        where T: new()
    {
        /// <summary>
        /// Read all records from the source.
        /// </summary>
        T[] Read();

        /// <summary>
        /// Write records to the source.
        /// </summary>
        /// <param name="data">Records to write.</param>
        /// <param name="overwrite">Replace all source data if specified.
        /// Append to existing data otherwise.</param>
        void Write(IList<T> data, bool overwrite = false);        
    }
}
