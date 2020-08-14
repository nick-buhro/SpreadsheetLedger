using System;

namespace SpreadsheetLedger.Core.Models.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    internal sealed class NameAttribute : Attribute
    {
        public string Name { get; }

        public NameAttribute(string name)
        {
            Name = name;
        }
    }
}
