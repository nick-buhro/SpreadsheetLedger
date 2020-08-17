using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Core;

namespace SpreadsheetLedger.ExcelAddIn
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {            
            this.Application.WorkbookActivate += Application_ActiveWorkbookChanged;
            this.Application.WorkbookDeactivate += Application_ActiveWorkbookChanged;
        }
        
        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            this.Application.WorkbookActivate -= Application_ActiveWorkbookChanged;
            this.Application.WorkbookDeactivate -= Application_ActiveWorkbookChanged;
        }

        private void Application_ActiveWorkbookChanged(Excel.Workbook Wb)
        {
            // TODO: Check ribbon buttons visibility
        }


        protected override IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new SpreadsheetLedgerRibbon();
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
