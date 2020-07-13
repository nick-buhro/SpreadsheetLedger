using System;

namespace SpreadsheetLedger.Core.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal sealed class MandatoryAttribute : Attribute
    {
        public bool Key { get; }

        public MandatoryAttribute(bool keyAttribute = false)
        {
            Key = keyAttribute;
        }
    }
}
