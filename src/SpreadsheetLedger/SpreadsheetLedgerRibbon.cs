using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Tools.Ribbon;
using SpreadsheetLedger.Core;
using SpreadsheetLedger.Core.Models;

namespace SpreadsheetLedger
{
    public partial class SpreadsheetLedgerRibbon
    {
        private void SpreadsheetLedgerRibbon_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private void buttonGLRefresh_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                var lo = Globals.JournalSheet.ListObjects[1];
                var data = RecordManager.Read<JournalRecord>(
                    lo.HeaderRowRange.Value,
                    lo.DataBodyRange.Value);

                MessageBox.Show($"{data.Count} rows read.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
