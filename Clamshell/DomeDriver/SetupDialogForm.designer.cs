using System.Drawing;
using System.Windows.Forms;

namespace ASCOM.Clamshell.Dome
{
    partial class SetupDialogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Root layout and buttons
        private TableLayoutPanel tlpRoot;
        private TableLayoutPanel tlpHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private PictureBox picASCOM;
        private TabControl tabWizard;
        private TabPage tabConnection;
        private TabPage tabCommands;
        private TabPage tabTiming;
        private TabPage tabOptions;
        private Label lblStatus;
        private FlowLayoutPanel pnlButtons;
        private Button btnBack;
        private Button btnNext;
        private Button btnOK;
        private Button btnCancel;

        // Tab 1: connection
        private TableLayoutPanel tlpConnection;
        private Label lblHost;
        private TextBox txtHost;
        private Label lblHostHint;
        private CheckBox chkHttps;
        private Label lblAuth;
        private ComboBox cmbAuth;
        private Label lblUser;
        private TextBox txtUser;
        private Label lblPass;
        private TableLayoutPanel tlpPass;
        private TextBox txtPass;
        private CheckBox chkShowPass;
        private Label lblPassHint;
        private Button btnTest;
        private Label lblTestResult;
        private Label lblTestNote;

        // Tab 2: commands
        private TableLayoutPanel tlpCommands;
        private GroupBox grpQuick;
        private TableLayoutPanel tlpQuick;
        private Label lblPath;
        private TextBox txtPath;
        private Label lblOpenRelays;
        private TextBox txtOpenRelays;
        private Label lblCloseRelays;
        private TextBox txtCloseRelays;
        private CheckBox chkParallel;
        private Label lblDelay;
        private NumericUpDown numDelay;
        private Button btnRelink;
        private Label lblLinked;
        private Label lblPreviewTitle;
        private TextBox txtPreview;
        private Button btnAdvanced;
        private GroupBox grpAdvanced;
        private TableLayoutPanel tlpAdvanced;
        private Label lblStepAction;
        private ComboBox cmbStepAction;
        private Label lblAdvHint;
        private DataGridView dgvSteps;

        // Tab 3: timing and status
        private TableLayoutPanel tlpTiming;
        private Label lblOpenTravel;
        private NumericUpDown numOpenTravel;
        private Label lblCloseTravel;
        private NumericUpDown numCloseTravel;
        private Label lblTimeout;
        private NumericUpDown numTimeout;
        private Label lblRegex;
        private TextBox txtRegex;
        private Label lblRegexHint;
        private Label lblAssumed;
        private ComboBox cmbAssumed;
        private Label lblExplain;

        // Tab 4: options
        private TableLayoutPanel tlpOptions;
        private CheckBox chkSimulation;
        private Label lblSimWarn;
        private CheckBox chkTrace;
        private FlowLayoutPanel flpIO;
        private Button btnImport;
        private Button btnExport;
        private LinkLabel lnkDocs;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupDialogForm));
            this.tlpRoot = new TableLayoutPanel();
            this.tlpHeader = new TableLayoutPanel();
            this.lblTitle = new Label();
            this.lblSubtitle = new Label();
            this.picASCOM = new PictureBox();
            this.tabWizard = new TabControl();
            this.tabConnection = new TabPage();
            this.tabCommands = new TabPage();
            this.tabTiming = new TabPage();
            this.tabOptions = new TabPage();
            this.lblStatus = new Label();
            this.pnlButtons = new FlowLayoutPanel();
            this.btnBack = new Button();
            this.btnNext = new Button();
            this.btnOK = new Button();
            this.btnCancel = new Button();

            this.tlpConnection = new TableLayoutPanel();
            this.lblHost = new Label();
            this.txtHost = new TextBox();
            this.lblHostHint = new Label();
            this.chkHttps = new CheckBox();
            this.lblAuth = new Label();
            this.cmbAuth = new ComboBox();
            this.lblUser = new Label();
            this.txtUser = new TextBox();
            this.lblPass = new Label();
            this.tlpPass = new TableLayoutPanel();
            this.txtPass = new TextBox();
            this.chkShowPass = new CheckBox();
            this.lblPassHint = new Label();
            this.btnTest = new Button();
            this.lblTestResult = new Label();
            this.lblTestNote = new Label();

            this.tlpCommands = new TableLayoutPanel();
            this.grpQuick = new GroupBox();
            this.tlpQuick = new TableLayoutPanel();
            this.lblPath = new Label();
            this.txtPath = new TextBox();
            this.lblOpenRelays = new Label();
            this.txtOpenRelays = new TextBox();
            this.lblCloseRelays = new Label();
            this.txtCloseRelays = new TextBox();
            this.chkParallel = new CheckBox();
            this.lblDelay = new Label();
            this.numDelay = new NumericUpDown();
            this.btnRelink = new Button();
            this.lblLinked = new Label();
            this.lblPreviewTitle = new Label();
            this.txtPreview = new TextBox();
            this.btnAdvanced = new Button();
            this.grpAdvanced = new GroupBox();
            this.tlpAdvanced = new TableLayoutPanel();
            this.lblStepAction = new Label();
            this.cmbStepAction = new ComboBox();
            this.lblAdvHint = new Label();
            this.dgvSteps = new DataGridView();

            this.tlpTiming = new TableLayoutPanel();
            this.lblOpenTravel = new Label();
            this.numOpenTravel = new NumericUpDown();
            this.lblCloseTravel = new Label();
            this.numCloseTravel = new NumericUpDown();
            this.lblTimeout = new Label();
            this.numTimeout = new NumericUpDown();
            this.lblRegex = new Label();
            this.txtRegex = new TextBox();
            this.lblRegexHint = new Label();
            this.lblAssumed = new Label();
            this.cmbAssumed = new ComboBox();
            this.lblExplain = new Label();

            this.tlpOptions = new TableLayoutPanel();
            this.chkSimulation = new CheckBox();
            this.lblSimWarn = new Label();
            this.chkTrace = new CheckBox();
            this.flpIO = new FlowLayoutPanel();
            this.btnImport = new Button();
            this.btnExport = new Button();
            this.lnkDocs = new LinkLabel();

            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOpenTravel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCloseTravel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSteps)).BeginInit();
            this.tlpRoot.SuspendLayout();
            this.tlpHeader.SuspendLayout();
            this.tabWizard.SuspendLayout();
            this.tabConnection.SuspendLayout();
            this.tabCommands.SuspendLayout();
            this.tabTiming.SuspendLayout();
            this.tabOptions.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.tlpConnection.SuspendLayout();
            this.tlpPass.SuspendLayout();
            this.tlpCommands.SuspendLayout();
            this.grpQuick.SuspendLayout();
            this.tlpQuick.SuspendLayout();
            this.grpAdvanced.SuspendLayout();
            this.tlpAdvanced.SuspendLayout();
            this.tlpTiming.SuspendLayout();
            this.tlpOptions.SuspendLayout();
            this.flpIO.SuspendLayout();
            this.SuspendLayout();

            // ---------------- root ----------------
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tlpRoot.RowCount = 4;
            this.tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpRoot.Controls.Add(this.tlpHeader, 0, 0);
            this.tlpRoot.Controls.Add(this.tabWizard, 0, 1);
            this.tlpRoot.Controls.Add(this.lblStatus, 0, 2);
            this.tlpRoot.Controls.Add(this.pnlButtons, 0, 3);
            this.tlpRoot.Dock = DockStyle.Fill;
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.Padding = new Padding(0);

            // ---------------- header ----------------
            this.tlpHeader.ColumnCount = 2;
            this.tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            this.tlpHeader.RowCount = 2;
            this.tlpHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpHeader.Controls.Add(this.lblTitle, 0, 0);
            this.tlpHeader.Controls.Add(this.lblSubtitle, 0, 1);
            this.tlpHeader.Controls.Add(this.picASCOM, 1, 0);
            this.tlpHeader.SetRowSpan(this.picASCOM, 2);
            this.tlpHeader.Dock = DockStyle.Top;
            this.tlpHeader.Padding = new Padding(12, 10, 12, 6);
            this.tlpHeader.Name = "tlpHeader";
            this.tlpHeader.BackColor = SystemColors.Control;

            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = Strings.Get("header.title");

            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Text = Strings.Format("header.version", DriverVersionText()) + "  -  " + Strings.Get("header.author");

            this.picASCOM.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.picASCOM.Cursor = Cursors.Hand;
            this.picASCOM.Image = ((Image)(resources.GetObject("picASCOM.Image")));
            this.picASCOM.Name = "picASCOM";
            this.picASCOM.SizeMode = PictureBoxSizeMode.AutoSize;
            this.picASCOM.TabStop = false;
            this.picASCOM.Click += new System.EventHandler(this.BrowseToAscom);
            this.picASCOM.DoubleClick += new System.EventHandler(this.BrowseToAscom);

            // ---------------- wizard tabs ----------------
            this.tabWizard.Controls.Add(this.tabConnection);
            this.tabWizard.Controls.Add(this.tabCommands);
            this.tabWizard.Controls.Add(this.tabTiming);
            this.tabWizard.Controls.Add(this.tabOptions);
            this.tabWizard.Dock = DockStyle.Fill;
            this.tabWizard.Name = "tabWizard";
            this.tabWizard.Padding = new Point(12, 4);
            this.tabWizard.SelectedIndexChanged += new System.EventHandler(this.tabWizard_SelectedIndexChanged);

            this.tabConnection.Controls.Add(this.tlpConnection);
            this.tabConnection.Name = "tabConnection";
            this.tabConnection.Padding = new Padding(3);
            this.tabConnection.Text = Strings.Get("tab.connection");
            this.tabCommands.Controls.Add(this.tlpCommands);
            this.tabCommands.Name = "tabCommands";
            this.tabCommands.Padding = new Padding(3);
            this.tabCommands.Text = Strings.Get("tab.commands");
            this.tabTiming.Controls.Add(this.tlpTiming);
            this.tabTiming.Name = "tabTiming";
            this.tabTiming.Padding = new Padding(3);
            this.tabTiming.Text = Strings.Get("tab.timing");
            this.tabOptions.Controls.Add(this.tlpOptions);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.Padding = new Padding(3);
            this.tabOptions.Text = Strings.Get("tab.options");

            // ---------------- status strip ----------------
            this.lblStatus.AutoSize = true;
            this.lblStatus.Dock = DockStyle.Fill;
            this.lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Padding = new Padding(12, 6, 12, 6);
            this.lblStatus.Text = "";

            // ---------------- buttons ----------------
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Controls.Add(this.btnOK);
            this.pnlButtons.Controls.Add(this.btnNext);
            this.pnlButtons.Controls.Add(this.btnBack);
            this.pnlButtons.AutoSize = true;
            this.pnlButtons.Dock = DockStyle.Fill;
            this.pnlButtons.FlowDirection = FlowDirection.RightToLeft;
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new Padding(9, 6, 9, 9);
            this.pnlButtons.WrapContents = false;

            this.btnCancel.Name = "btnCancel";
            this.btnCancel.AutoSize = true;
            this.btnCancel.MinimumSize = new Size(84, 27);
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Text = Strings.Get("btn.cancel");
            this.btnCancel.UseVisualStyleBackColor = true;

            this.btnOK.Name = "btnOK";
            this.btnOK.AutoSize = true;
            this.btnOK.MinimumSize = new Size(84, 27);
            this.btnOK.Text = Strings.Get("btn.ok");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            this.btnNext.Name = "btnNext";
            this.btnNext.AutoSize = true;
            this.btnNext.MinimumSize = new Size(84, 27);
            this.btnNext.Text = Strings.Get("btn.next");
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);

            this.btnBack.Name = "btnBack";
            this.btnBack.AutoSize = true;
            this.btnBack.MinimumSize = new Size(84, 27);
            this.btnBack.Text = Strings.Get("btn.back");
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);

            // ---------------- tab 1: connection ----------------
            this.tlpConnection.ColumnCount = 2;
            this.tlpConnection.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            this.tlpConnection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tlpConnection.RowCount = 10;
            for (int i = 0; i < 9; i++)
                this.tlpConnection.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpConnection.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tlpConnection.Dock = DockStyle.Fill;
            this.tlpConnection.Name = "tlpConnection";
            this.tlpConnection.Padding = new Padding(9);
            this.tlpConnection.Controls.Add(this.lblHost, 0, 0);
            this.tlpConnection.Controls.Add(this.txtHost, 1, 0);
            this.tlpConnection.Controls.Add(this.lblHostHint, 1, 1);
            this.tlpConnection.Controls.Add(this.chkHttps, 0, 2);
            this.tlpConnection.SetColumnSpan(this.chkHttps, 2);
            this.tlpConnection.Controls.Add(this.lblAuth, 0, 3);
            this.tlpConnection.Controls.Add(this.cmbAuth, 1, 3);
            this.tlpConnection.Controls.Add(this.lblUser, 0, 4);
            this.tlpConnection.Controls.Add(this.txtUser, 1, 4);
            this.tlpConnection.Controls.Add(this.lblPass, 0, 5);
            this.tlpConnection.Controls.Add(this.tlpPass, 1, 5);
            this.tlpConnection.Controls.Add(this.lblPassHint, 1, 6);
            this.tlpConnection.Controls.Add(this.btnTest, 0, 7);
            this.tlpConnection.Controls.Add(this.lblTestResult, 1, 7);
            this.tlpConnection.Controls.Add(this.lblTestNote, 0, 8);
            this.tlpConnection.SetColumnSpan(this.lblTestNote, 2);

            this.lblHost.AutoSize = true;
            this.lblHost.Anchor = AnchorStyles.Left;
            this.lblHost.Name = "lblHost";
            this.lblHost.Text = Strings.Get("conn.host");
            this.lblHost.Margin = new Padding(3, 6, 3, 3);
            this.txtHost.Dock = DockStyle.Fill;
            this.txtHost.Name = "txtHost";
            this.txtHost.TextChanged += new System.EventHandler(this.Field_Changed);
            this.lblHostHint.AutoSize = true;
            this.lblHostHint.ForeColor = SystemColors.GrayText;
            this.lblHostHint.Name = "lblHostHint";
            this.lblHostHint.Text = Strings.Get("conn.host_hint");
            this.chkHttps.AutoSize = true;
            this.chkHttps.Name = "chkHttps";
            this.chkHttps.Text = Strings.Get("conn.https");
            this.chkHttps.CheckedChanged += new System.EventHandler(this.Field_Changed);
            this.lblAuth.AutoSize = true;
            this.lblAuth.Name = "lblAuth";
            this.lblAuth.Text = Strings.Get("conn.auth");
            this.lblAuth.Margin = new Padding(3, 6, 3, 3);
            this.cmbAuth.Dock = DockStyle.Fill;
            this.cmbAuth.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbAuth.Items.AddRange(new object[] { Strings.Get("auth.none"), Strings.Get("auth.basic") });
            this.cmbAuth.Name = "cmbAuth";
            this.cmbAuth.SelectedIndexChanged += new System.EventHandler(this.Field_Changed);
            this.lblUser.AutoSize = true;
            this.lblUser.Name = "lblUser";
            this.lblUser.Text = Strings.Get("conn.user");
            this.lblUser.Margin = new Padding(3, 6, 3, 3);
            this.txtUser.Dock = DockStyle.Fill;
            this.txtUser.Name = "txtUser";
            this.txtUser.TextChanged += new System.EventHandler(this.Field_Changed);
            this.lblPass.AutoSize = true;
            this.lblPass.Name = "lblPass";
            this.lblPass.Text = Strings.Get("conn.pass");
            this.lblPass.Margin = new Padding(3, 6, 3, 3);
            this.tlpPass.ColumnCount = 2;
            this.tlpPass.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tlpPass.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            this.tlpPass.RowCount = 1;
            this.tlpPass.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpPass.Controls.Add(this.txtPass, 0, 0);
            this.tlpPass.Controls.Add(this.chkShowPass, 1, 0);
            this.tlpPass.Dock = DockStyle.Fill;
            this.tlpPass.Margin = new Padding(0);
            this.tlpPass.Name = "tlpPass";
            this.txtPass.Dock = DockStyle.Fill;
            this.txtPass.Name = "txtPass";
            this.txtPass.PasswordChar = '*';
            this.txtPass.TextChanged += new System.EventHandler(this.txtPass_TextChanged);
            this.chkShowPass.AutoSize = true;
            this.chkShowPass.Anchor = AnchorStyles.Left;
            this.chkShowPass.Name = "chkShowPass";
            this.chkShowPass.Text = Strings.Get("conn.show_pass");
            this.chkShowPass.CheckedChanged += new System.EventHandler(this.chkShowPass_CheckedChanged);
            this.lblPassHint.AutoSize = true;
            this.lblPassHint.ForeColor = SystemColors.GrayText;
            this.lblPassHint.Name = "lblPassHint";
            this.lblPassHint.Text = Strings.Get("conn.pass_none");
            this.btnTest.AutoSize = true;
            this.btnTest.MinimumSize = new Size(120, 27);
            this.btnTest.Name = "btnTest";
            this.btnTest.Text = Strings.Get("conn.test");
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            this.lblTestResult.AutoSize = true;
            this.lblTestResult.Anchor = AnchorStyles.Left;
            this.lblTestResult.Name = "lblTestResult";
            this.lblTestResult.Text = "";
            this.lblTestResult.Margin = new Padding(3, 6, 3, 3);
            this.lblTestNote.AutoSize = true;
            this.lblTestNote.ForeColor = SystemColors.GrayText;
            this.lblTestNote.Name = "lblTestNote";
            this.lblTestNote.Text = Strings.Get("conn.test_note");

            // ---------------- tab 2: commands ----------------
            this.tlpCommands.ColumnCount = 1;
            this.tlpCommands.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tlpCommands.RowCount = 5;
            this.tlpCommands.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpCommands.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpCommands.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tlpCommands.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpCommands.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
            this.tlpCommands.Controls.Add(this.grpQuick, 0, 0);
            this.tlpCommands.Controls.Add(this.lblPreviewTitle, 0, 1);
            this.tlpCommands.Controls.Add(this.txtPreview, 0, 2);
            this.tlpCommands.Controls.Add(this.btnAdvanced, 0, 3);
            this.tlpCommands.Controls.Add(this.grpAdvanced, 0, 4);
            this.tlpCommands.Dock = DockStyle.Fill;
            this.tlpCommands.Name = "tlpCommands";
            this.tlpCommands.Padding = new Padding(9);

            this.grpQuick.AutoSize = true;
            this.grpQuick.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.grpQuick.Controls.Add(this.tlpQuick);
            this.grpQuick.Dock = DockStyle.Fill;
            this.grpQuick.Name = "grpQuick";
            this.grpQuick.Padding = new Padding(6);
            this.grpQuick.Text = Strings.Get("cmd.quick");

            this.tlpQuick.ColumnCount = 2;
            this.tlpQuick.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            this.tlpQuick.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tlpQuick.RowCount = 7;
            for (int i = 0; i < 7; i++)
                this.tlpQuick.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpQuick.Controls.Add(this.lblPath, 0, 0);
            this.tlpQuick.Controls.Add(this.txtPath, 1, 0);
            this.tlpQuick.Controls.Add(this.lblOpenRelays, 0, 1);
            this.tlpQuick.Controls.Add(this.txtOpenRelays, 1, 1);
            this.tlpQuick.Controls.Add(this.lblCloseRelays, 0, 2);
            this.tlpQuick.Controls.Add(this.txtCloseRelays, 1, 2);
            this.tlpQuick.Controls.Add(this.chkParallel, 0, 3);
            this.tlpQuick.SetColumnSpan(this.chkParallel, 2);
            this.tlpQuick.Controls.Add(this.lblDelay, 0, 4);
            this.tlpQuick.Controls.Add(this.numDelay, 1, 4);
            this.tlpQuick.Controls.Add(this.btnRelink, 0, 5);
            this.tlpQuick.SetColumnSpan(this.btnRelink, 2);
            this.tlpQuick.Controls.Add(this.lblLinked, 0, 6);
            this.tlpQuick.SetColumnSpan(this.lblLinked, 2);
            this.tlpQuick.Dock = DockStyle.Fill;
            this.tlpQuick.Name = "tlpQuick";

            this.lblPath.AutoSize = true;
            this.lblPath.Name = "lblPath";
            this.lblPath.Text = Strings.Get("cmd.path");
            this.lblPath.Margin = new Padding(3, 6, 3, 3);
            this.txtPath.Dock = DockStyle.Fill;
            this.txtPath.Name = "txtPath";
            this.txtPath.TextChanged += new System.EventHandler(this.Quick_Changed);
            this.lblOpenRelays.AutoSize = true;
            this.lblOpenRelays.Name = "lblOpenRelays";
            this.lblOpenRelays.Text = Strings.Get("cmd.open_relays");
            this.lblOpenRelays.Margin = new Padding(3, 6, 3, 3);
            this.txtOpenRelays.Dock = DockStyle.Fill;
            this.txtOpenRelays.Name = "txtOpenRelays";
            this.txtOpenRelays.TextChanged += new System.EventHandler(this.Quick_Changed);
            this.lblCloseRelays.AutoSize = true;
            this.lblCloseRelays.Name = "lblCloseRelays";
            this.lblCloseRelays.Text = Strings.Get("cmd.close_relays");
            this.lblCloseRelays.Margin = new Padding(3, 6, 3, 3);
            this.txtCloseRelays.Dock = DockStyle.Fill;
            this.txtCloseRelays.Name = "txtCloseRelays";
            this.txtCloseRelays.TextChanged += new System.EventHandler(this.Quick_Changed);
            this.chkParallel.AutoSize = true;
            this.chkParallel.Name = "chkParallel";
            this.chkParallel.Text = Strings.Get("cmd.parallel");
            this.chkParallel.CheckedChanged += new System.EventHandler(this.Quick_Changed);
            this.lblDelay.AutoSize = true;
            this.lblDelay.Name = "lblDelay";
            this.lblDelay.Text = Strings.Get("cmd.delay");
            this.lblDelay.Margin = new Padding(3, 6, 3, 3);
            this.numDelay.Name = "numDelay";
            this.numDelay.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            this.numDelay.Dock = DockStyle.Left;
            this.numDelay.Width = 110;
            this.numDelay.ValueChanged += new System.EventHandler(this.Quick_Changed);
            this.btnRelink.AutoSize = true;
            this.btnRelink.Name = "btnRelink";
            this.btnRelink.Text = Strings.Get("cmd.relink");
            this.btnRelink.UseVisualStyleBackColor = true;
            this.btnRelink.Click += new System.EventHandler(this.btnRelink_Click);
            this.lblLinked.AutoSize = true;
            this.lblLinked.Name = "lblLinked";
            this.lblLinked.Text = "";

            this.lblPreviewTitle.AutoSize = true;
            this.lblPreviewTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.lblPreviewTitle.Name = "lblPreviewTitle";
            this.lblPreviewTitle.Text = Strings.Get("cmd.preview_title");
            this.lblPreviewTitle.Margin = new Padding(3, 9, 3, 3);
            this.txtPreview.Dock = DockStyle.Fill;
            this.txtPreview.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.txtPreview.Multiline = true;
            this.txtPreview.ReadOnly = true;
            this.txtPreview.ScrollBars = ScrollBars.Both;
            this.txtPreview.WordWrap = false;
            this.txtPreview.Name = "txtPreview";
            this.txtPreview.TabStop = false;
            this.btnAdvanced.AutoSize = true;
            this.btnAdvanced.Anchor = AnchorStyles.Left;
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.Text = Strings.Get("cmd.advanced");
            this.btnAdvanced.UseVisualStyleBackColor = true;
            this.btnAdvanced.Margin = new Padding(3, 6, 3, 3);
            this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);

            this.grpAdvanced.Controls.Add(this.tlpAdvanced);
            this.grpAdvanced.Dock = DockStyle.Fill;
            this.grpAdvanced.Name = "grpAdvanced";
            this.grpAdvanced.Padding = new Padding(6);
            this.grpAdvanced.Text = Strings.Get("cmd.advanced");
            this.grpAdvanced.Visible = false;

            this.tlpAdvanced.ColumnCount = 2;
            this.tlpAdvanced.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            this.tlpAdvanced.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tlpAdvanced.RowCount = 3;
            this.tlpAdvanced.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpAdvanced.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpAdvanced.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tlpAdvanced.Controls.Add(this.lblStepAction, 0, 0);
            this.tlpAdvanced.Controls.Add(this.cmbStepAction, 1, 0);
            this.tlpAdvanced.Controls.Add(this.lblAdvHint, 0, 1);
            this.tlpAdvanced.SetColumnSpan(this.lblAdvHint, 2);
            this.tlpAdvanced.Controls.Add(this.dgvSteps, 0, 2);
            this.tlpAdvanced.SetColumnSpan(this.dgvSteps, 2);
            this.tlpAdvanced.Dock = DockStyle.Fill;
            this.tlpAdvanced.Name = "tlpAdvanced";

            this.lblStepAction.AutoSize = true;
            this.lblStepAction.Name = "lblStepAction";
            this.lblStepAction.Text = Strings.Get("cmd.action");
            this.lblStepAction.Margin = new Padding(3, 6, 3, 3);
            this.cmbStepAction.Dock = DockStyle.Left;
            this.cmbStepAction.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbStepAction.Items.AddRange(new object[] { Strings.Get("cmd.action_open"), Strings.Get("cmd.action_close") });
            this.cmbStepAction.Name = "cmbStepAction";
            this.cmbStepAction.Width = 160;
            this.cmbStepAction.SelectedIndexChanged += new System.EventHandler(this.cmbStepAction_SelectedIndexChanged);
            this.lblAdvHint.AutoSize = true;
            this.lblAdvHint.ForeColor = SystemColors.GrayText;
            this.lblAdvHint.Name = "lblAdvHint";
            this.lblAdvHint.Text = Strings.Get("cmd.adv_hint");

            this.dgvSteps.AllowUserToResizeRows = false;
            this.dgvSteps.AutoGenerateColumns = false;
            this.dgvSteps.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            this.dgvSteps.BackgroundColor = SystemColors.Window;
            this.dgvSteps.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSteps.Dock = DockStyle.Fill;
            this.dgvSteps.MultiSelect = false;
            this.dgvSteps.Name = "dgvSteps";
            this.dgvSteps.RowHeadersVisible = false;
            this.dgvSteps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvSteps.DataError += new DataGridViewDataErrorEventHandler(this.dgvSteps_DataError);
            DataGridViewTextBoxColumn colUrl = new DataGridViewTextBoxColumn();
            colUrl.DataPropertyName = "UrlTemplate";
            colUrl.HeaderText = Strings.Get("cmd.col_url");
            colUrl.Name = "colUrl";
            colUrl.SortMode = DataGridViewColumnSortMode.NotSortable;
            colUrl.Width = 330;
            DataGridViewTextBoxColumn colDelay = new DataGridViewTextBoxColumn();
            colDelay.DataPropertyName = "DelayAfterMs";
            colDelay.HeaderText = Strings.Get("cmd.col_delay");
            colDelay.Name = "colDelay";
            colDelay.SortMode = DataGridViewColumnSortMode.NotSortable;
            colDelay.Width = 110;
            this.dgvSteps.Columns.AddRange(new DataGridViewColumn[] { colUrl, colDelay });

            // ---------------- tab 3: timing and status ----------------
            this.tlpTiming.ColumnCount = 2;
            this.tlpTiming.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            this.tlpTiming.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tlpTiming.RowCount = 8;
            for (int i = 0; i < 7; i++)
                this.tlpTiming.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpTiming.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tlpTiming.Controls.Add(this.lblOpenTravel, 0, 0);
            this.tlpTiming.Controls.Add(this.numOpenTravel, 1, 0);
            this.tlpTiming.Controls.Add(this.lblCloseTravel, 0, 1);
            this.tlpTiming.Controls.Add(this.numCloseTravel, 1, 1);
            this.tlpTiming.Controls.Add(this.lblTimeout, 0, 2);
            this.tlpTiming.Controls.Add(this.numTimeout, 1, 2);
            this.tlpTiming.Controls.Add(this.lblRegex, 0, 3);
            this.tlpTiming.Controls.Add(this.txtRegex, 1, 3);
            this.tlpTiming.Controls.Add(this.lblRegexHint, 1, 4);
            this.tlpTiming.Controls.Add(this.lblAssumed, 0, 5);
            this.tlpTiming.Controls.Add(this.cmbAssumed, 1, 5);
            this.tlpTiming.Controls.Add(this.lblExplain, 0, 6);
            this.tlpTiming.SetColumnSpan(this.lblExplain, 2);
            this.tlpTiming.Dock = DockStyle.Fill;
            this.tlpTiming.Name = "tlpTiming";
            this.tlpTiming.Padding = new Padding(9);

            this.lblOpenTravel.AutoSize = true;
            this.lblOpenTravel.Name = "lblOpenTravel";
            this.lblOpenTravel.Text = Strings.Get("time.open");
            this.lblOpenTravel.Margin = new Padding(3, 6, 3, 3);
            this.numOpenTravel.Name = "numOpenTravel";
            this.numOpenTravel.Maximum = new decimal(new int[] { 900, 0, 0, 0 });
            this.numOpenTravel.Dock = DockStyle.Left;
            this.numOpenTravel.Width = 110;
            this.numOpenTravel.ValueChanged += new System.EventHandler(this.Field_Changed);
            this.lblCloseTravel.AutoSize = true;
            this.lblCloseTravel.Name = "lblCloseTravel";
            this.lblCloseTravel.Text = Strings.Get("time.close");
            this.lblCloseTravel.Margin = new Padding(3, 6, 3, 3);
            this.numCloseTravel.Name = "numCloseTravel";
            this.numCloseTravel.Maximum = new decimal(new int[] { 900, 0, 0, 0 });
            this.numCloseTravel.Dock = DockStyle.Left;
            this.numCloseTravel.Width = 110;
            this.numCloseTravel.ValueChanged += new System.EventHandler(this.Field_Changed);
            this.lblTimeout.AutoSize = true;
            this.lblTimeout.Name = "lblTimeout";
            this.lblTimeout.Text = Strings.Get("time.timeout");
            this.lblTimeout.Margin = new Padding(3, 6, 3, 3);
            this.numTimeout.Name = "numTimeout";
            this.numTimeout.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            this.numTimeout.Increment = new decimal(new int[] { 500, 0, 0, 0 });
            this.numTimeout.Dock = DockStyle.Left;
            this.numTimeout.Width = 110;
            this.numTimeout.ValueChanged += new System.EventHandler(this.Field_Changed);
            this.lblRegex.AutoSize = true;
            this.lblRegex.Name = "lblRegex";
            this.lblRegex.Text = Strings.Get("time.regex");
            this.lblRegex.Margin = new Padding(3, 6, 3, 3);
            this.txtRegex.Dock = DockStyle.Fill;
            this.txtRegex.Name = "txtRegex";
            this.txtRegex.TextChanged += new System.EventHandler(this.Field_Changed);
            this.lblRegexHint.AutoSize = true;
            this.lblRegexHint.ForeColor = SystemColors.GrayText;
            this.lblRegexHint.Name = "lblRegexHint";
            this.lblRegexHint.Text = Strings.Get("cue.regex");
            this.lblAssumed.AutoSize = true;
            this.lblAssumed.Name = "lblAssumed";
            this.lblAssumed.Text = Strings.Get("time.assumed");
            this.lblAssumed.Margin = new Padding(3, 6, 3, 3);
            this.cmbAssumed.Dock = DockStyle.Left;
            this.cmbAssumed.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbAssumed.Items.AddRange(new object[] { Strings.Get("time.state_closed"), Strings.Get("time.state_open") });
            this.cmbAssumed.Name = "cmbAssumed";
            this.cmbAssumed.Width = 160;
            this.cmbAssumed.SelectedIndexChanged += new System.EventHandler(this.Field_Changed);
            this.lblExplain.AutoSize = true;
            this.lblExplain.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point, 0);
            this.lblExplain.Name = "lblExplain";
            this.lblExplain.Text = Strings.Get("time.explain");
            this.lblExplain.Margin = new Padding(3, 12, 3, 3);

            // ---------------- tab 4: options ----------------
            this.tlpOptions.ColumnCount = 2;
            this.tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            this.tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tlpOptions.RowCount = 6;
            for (int i = 0; i < 5; i++)
                this.tlpOptions.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tlpOptions.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tlpOptions.Controls.Add(this.chkSimulation, 0, 0);
            this.tlpOptions.SetColumnSpan(this.chkSimulation, 2);
            this.tlpOptions.Controls.Add(this.lblSimWarn, 0, 1);
            this.tlpOptions.SetColumnSpan(this.lblSimWarn, 2);
            this.tlpOptions.Controls.Add(this.chkTrace, 0, 2);
            this.tlpOptions.SetColumnSpan(this.chkTrace, 2);
            this.tlpOptions.Controls.Add(this.flpIO, 0, 3);
            this.tlpOptions.SetColumnSpan(this.flpIO, 2);
            this.tlpOptions.Controls.Add(this.lnkDocs, 0, 4);
            this.tlpOptions.SetColumnSpan(this.lnkDocs, 2);
            this.tlpOptions.Dock = DockStyle.Fill;
            this.tlpOptions.Name = "tlpOptions";
            this.tlpOptions.Padding = new Padding(9);

            this.chkSimulation.AutoSize = true;
            this.chkSimulation.Name = "chkSimulation";
            this.chkSimulation.Text = Strings.Get("opt.sim");
            this.chkSimulation.Margin = new Padding(3, 3, 3, 6);
            this.chkSimulation.CheckedChanged += new System.EventHandler(this.Field_Changed);
            this.lblSimWarn.AutoSize = true;
            this.lblSimWarn.ForeColor = Color.Firebrick;
            this.lblSimWarn.Name = "lblSimWarn";
            this.lblSimWarn.Text = Strings.Get("opt.sim_warn");
            this.lblSimWarn.Margin = new Padding(22, 0, 3, 9);
            this.chkTrace.AutoSize = true;
            this.chkTrace.Name = "chkTrace";
            this.chkTrace.Text = Strings.Get("opt.trace");
            this.chkTrace.Margin = new Padding(3, 3, 3, 9);
            this.chkTrace.CheckedChanged += new System.EventHandler(this.Field_Changed);
            this.flpIO.AutoSize = true;
            this.flpIO.Controls.Add(this.btnImport);
            this.flpIO.Controls.Add(this.btnExport);
            this.flpIO.Name = "flpIO";
            this.flpIO.Margin = new Padding(0);
            this.flpIO.WrapContents = false;
            this.btnImport.AutoSize = true;
            this.btnImport.MinimumSize = new Size(120, 27);
            this.btnImport.Name = "btnImport";
            this.btnImport.Text = Strings.Get("opt.import");
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            this.btnExport.AutoSize = true;
            this.btnExport.MinimumSize = new Size(120, 27);
            this.btnExport.Name = "btnExport";
            this.btnExport.Text = Strings.Get("opt.export");
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            this.lnkDocs.AutoSize = true;
            this.lnkDocs.Name = "lnkDocs";
            this.lnkDocs.Text = Strings.Get("opt.docs");
            this.lnkDocs.Margin = new Padding(3, 12, 3, 3);
            this.lnkDocs.LinkClicked += new LinkLabelLinkClickedEventHandler(this.lnkDocs_LinkClicked);

            // ---------------- form ----------------
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.ClientSize = new Size(640, 560);
            this.MinimumSize = new Size(560, 520);
            this.Controls.Add(this.tlpRoot);
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupDialogForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = Strings.Get("title.form");
            this.Load += new System.EventHandler(this.SetupDialogForm_Load);
            this.Resize += new System.EventHandler(this.SetupDialogForm_Resize);

            ((System.ComponentModel.ISupportInitialize)(this.dgvSteps)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCloseTravel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOpenTravel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).EndInit();
            this.flpIO.ResumeLayout(false);
            this.tlpOptions.ResumeLayout(false);
            this.tlpOptions.PerformLayout();
            this.tlpTiming.ResumeLayout(false);
            this.tlpTiming.PerformLayout();
            this.tlpAdvanced.ResumeLayout(false);
            this.tlpAdvanced.PerformLayout();
            this.grpAdvanced.ResumeLayout(false);
            this.tlpQuick.ResumeLayout(false);
            this.tlpQuick.PerformLayout();
            this.grpQuick.ResumeLayout(false);
            this.grpQuick.PerformLayout();
            this.tlpCommands.ResumeLayout(false);
            this.tlpCommands.PerformLayout();
            this.tlpPass.ResumeLayout(false);
            this.tlpPass.PerformLayout();
            this.tlpConnection.ResumeLayout(false);
            this.tlpConnection.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.pnlButtons.PerformLayout();
            this.tabOptions.ResumeLayout(false);
            this.tabTiming.ResumeLayout(false);
            this.tabCommands.ResumeLayout(false);
            this.tabConnection.ResumeLayout(false);
            this.tabWizard.ResumeLayout(false);
            this.tlpHeader.ResumeLayout(false);
            this.tlpHeader.PerformLayout();
            this.tlpRoot.ResumeLayout(false);
            this.tlpRoot.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
