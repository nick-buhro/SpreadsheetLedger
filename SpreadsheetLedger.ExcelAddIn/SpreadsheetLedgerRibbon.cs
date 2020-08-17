using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using SpreadsheetLedger.Core;
using SpreadsheetLedger.Core.Impl;
using SpreadsheetLedger.Core.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Forms = System.Windows.Forms;

// TODO:  Follow these steps to enable the Ribbon (XML) item:

// 1: Copy the following code block into the ThisAddin, ThisWorkbook, or ThisDocument class.

//  protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
//  {
//      return new SpreadsheetLedgerRibbon();
//  }

// 2. Create callback methods in the "Ribbon Callbacks" region of this class to handle user
//    actions, such as clicking a button. Note: if you have exported this Ribbon from the Ribbon designer,
//    move your code from the event handlers to the callback methods and modify the code to work with the
//    Ribbon extensibility (RibbonX) programming model.

// 3. Assign attributes to the control tags in the Ribbon XML file to identify the appropriate callback methods in your code.  

// For more information, see the Ribbon XML documentation in the Visual Studio Tools for Office Help.


namespace SpreadsheetLedger.ExcelAddIn
{
    [ComVisible(true)]
    public class SpreadsheetLedgerRibbon : IRibbonExtensibility
    {
        private IRibbonUI _ribbon;

        private IBuildGLStrategy _buildGLStrategy;
        private IPriceImportStrategy _priceImportStrategy;

        //public SpreadsheetLedgerRibbon() { }
        
        #region IRibbonExtensibility Members

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("SpreadsheetLedger.ExcelAddIn.SpreadsheetLedgerRibbon.xml");
        }

        #endregion

        #region Ribbon Callbacks
        //Create callback methods here. For more information about adding callback methods, visit https://go.microsoft.com/fwlink/?LinkID=271226

        public void Ribbon_Load(IRibbonUI ribbonUI)
        {
            _ribbon = ribbonUI;

            _buildGLStrategy = new BuildGLStrategy();
            _priceImportStrategy = new PriceImportStrategy();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public void OnPriceImport(IRibbonControl control)
        {
            ExecuteCommand((wb) =>
            {
                var conf = wb.FindListObject("Configuration")
                     .Read<ConfigurationRecord>()
                     .Where(r => !string.IsNullOrEmpty(r.Key))
                     .ToDictionary(r => r.Key, r => r.Value);
                
                var prices = wb.FindListObject("Price")
                    .Read<PriceRecord>();

                var newPrices = _priceImportStrategy
                    .Import(
                        wb.FindListObject("Configuration").ReadLazy<ConfigurationRecord>(),
                        wb.FindListObject("Price").ReadLazy<PriceRecord>())
                    .Result;
                
                if (newPrices.Count > 0)
                {
                    var lo = wb.FindListObject("Price");
                    lo.Write(newPrices);
                    lo.Sort.Apply();
                }
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public void OnGLRefresh(IRibbonControl control)
        {
            ExecuteCommand((wb) =>
            {
                var gl = _buildGLStrategy.Build(
                    wb.FindListObject("CoA").ReadLazy<AccountRecord>(),
                    wb.FindListObject("Journal").ReadLazy<JournalRecord>(),
                    wb.FindListObject("Currency").ReadLazy<CurrencyRecord>(),
                    wb.FindListObject("Price").ReadLazy<PriceRecord>());

                wb.FindListObject("GL")
                    .Write(gl, true);

                wb.RefreshAll();
            });
        }

        #endregion

        #region Helpers

        private static void ExecuteCommand(Action<Workbook> action)
        {
            try
            {
                Globals.ThisAddIn.Application.ScreenUpdating = false;
                action(Globals.ThisAddIn.Application.ActiveWorkbook);
            }
            catch(Exception ex)
            {
                Forms.MessageBox.Show(ex.ToString());
            }
            finally
            {
                Globals.ThisAddIn.Application.ScreenUpdating = true;
            }
        }

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
