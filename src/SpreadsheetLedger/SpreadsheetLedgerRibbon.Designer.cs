namespace SpreadsheetLedger
{
    partial class SpreadsheetLedgerRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public SpreadsheetLedgerRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SLedgerTab = this.Factory.CreateRibbonTab();
            this.groupGL = this.Factory.CreateRibbonGroup();
            this.buttonGLRefresh = this.Factory.CreateRibbonButton();
            this.SLedgerTab.SuspendLayout();
            this.groupGL.SuspendLayout();
            this.SuspendLayout();
            // 
            // SLedgerTab
            // 
            this.SLedgerTab.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.SLedgerTab.Groups.Add(this.groupGL);
            this.SLedgerTab.Label = "Ledger";
            this.SLedgerTab.Name = "SLedgerTab";
            // 
            // groupGL
            // 
            this.groupGL.Items.Add(this.buttonGLRefresh);
            this.groupGL.Label = "GL";
            this.groupGL.Name = "groupGL";
            // 
            // buttonGLRefresh
            // 
            this.buttonGLRefresh.Label = "Refresh";
            this.buttonGLRefresh.Name = "buttonGLRefresh";
            this.buttonGLRefresh.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonGLRefresh_Click);
            // 
            // SpreadsheetLedgerRibbon
            // 
            this.Name = "SpreadsheetLedgerRibbon";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.SLedgerTab);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.SpreadsheetLedgerRibbon_Load);
            this.SLedgerTab.ResumeLayout(false);
            this.SLedgerTab.PerformLayout();
            this.groupGL.ResumeLayout(false);
            this.groupGL.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab SLedgerTab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupGL;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonGLRefresh;
    }

    partial class ThisRibbonCollection
    {
        internal SpreadsheetLedgerRibbon SpreadsheetLedgerRibbon
        {
            get { return this.GetRibbon<SpreadsheetLedgerRibbon>(); }
        }
    }
}
