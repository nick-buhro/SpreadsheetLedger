using System;
using System.Windows.Forms;
using Microsoft.Office.Tools.Ribbon;
using SpreadsheetLedger.Core;
using SpreadsheetLedger.Core.Commands;
using SpreadsheetLedger.Helpers;

namespace SpreadsheetLedger
{
    public partial class SpreadsheetLedgerRibbon
    {
        private IContext _context;
        private ICommand _glRefreshCommand;

        private void SpreadsheetLedgerRibbon_Load(object sender, RibbonUIEventArgs e)
        {
            _context = new Context();
            _glRefreshCommand = new GLRefreshCommand(_context);
        }

        private void buttonGLRefresh_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                _glRefreshCommand.Execute();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
