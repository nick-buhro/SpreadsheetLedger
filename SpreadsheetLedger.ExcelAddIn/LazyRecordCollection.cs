using SpreadsheetLedger.Core;
using System.Collections;
using System.Collections.Generic;

namespace SpreadsheetLedger.ExcelAddIn
{
    internal sealed class LazyRecordCollection<T> : IEnumerable<T>
        where T: new()
    {
        private readonly ISerializer _serializer;
        private readonly object[,] _header;
        private readonly object[,] _data;
        private T[] _deserialized;

        public LazyRecordCollection(object[,] header, object[,] data, ISerializer serializer)
        {
            _serializer = serializer;
            _header = header;
            _data = data;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_deserialized == null)
                _deserialized = _serializer.Read<T>(_header, _data);

            return ((IEnumerable<T>)_deserialized).GetEnumerator();            
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_deserialized == null)
                _deserialized = _serializer.Read<T>(_header, _data);

            return _deserialized.GetEnumerator();
        }
    }
}
