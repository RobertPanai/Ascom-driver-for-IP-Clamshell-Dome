using ASCOM.Clamshell.Core;
using ASCOM.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASCOM.Clamshell.Dome
{
    /// <summary>
    /// Setup dialogue of the Clamshell dome driver. Edits a private clone of the <see cref="DomeConfig"/>;
    /// on OK the clone (with the password re-protected if the password box was changed) is exposed in <see cref="Result"/>.
    /// </summary>
    [ComVisible(false)] // Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        private const uint EM_SETCUEBANNER = 0x1501;

        private readonly TraceLogger tl;
        private readonly ISecretProtector protector;
        private readonly BindingList<StepRow> gridRows = new BindingList<StepRow>();
        private DomeConfig cfg;
        private bool loading;
        private bool linked;
        private bool firstRun;

        /// <summary>
        /// Configuration edited by the user, set when OK is pressed. Null if the dialogue was cancelled.
        /// </summary>
        public DomeConfig Result { get; private set; }

        /// <summary>
        /// Creates the setup dialogue.
        /// </summary>
        /// <param name="tl">Trace logger of the driver (may be null).</param>
        /// <param name="config">Current configuration; it is cloned, never modified in place.</param>
        /// <param name="protector">Protector used to re-protect the password on OK.</param>
        public SetupDialogForm(TraceLogger tl, DomeConfig config, ISecretProtector protector)
        {
            this.tl = tl;
            this.protector = protector;
            cfg = config != null ? config.Clone() : new DomeConfig();
            EnsureActions();

            InitializeComponent();

            dgvSteps.DataSource = gridRows;
            gridRows.ListChanged += gridRows_ListChanged;

            LoadUiFromConfig();
            List<string> problems;
            firstRun = !ValidateConfig(out problems) && !cfg.SimulationMode;
            UpdateAll();
        }

        /// <summary>
        /// One row of the advanced steps grid. Public because the DataGridView binds to its properties.
        /// </summary>
        public sealed class StepRow : INotifyPropertyChanged
        {
            private string urlTemplate = "";
            private int delayAfterMs;

            /// <summary>Raised when a property changes (needed for grid edits to reach the configuration).</summary>
            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>URL template of the step, with {scheme} and {host} placeholders.</summary>
            public string UrlTemplate
            {
                get { return urlTemplate; }
                set
                {
                    if (urlTemplate == value) return;
                    urlTemplate = value;
                    Raise("UrlTemplate");
                }
            }

            /// <summary>Pause after this step, in milliseconds.</summary>
            public int DelayAfterMs
            {
                get { return delayAfterMs; }
                set
                {
                    if (delayAfterMs == value) return;
                    delayAfterMs = value;
                    Raise("DelayAfterMs");
                }
            }

            private void Raise(string name)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(name));
            }
        }

        // ------------------------------------------------------------------ startup and layout

        private void SetupDialogForm_Load(object sender, EventArgs e)
        {
            // Bring the setup dialogue to the front of the screen
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            else
            {
                TopMost = true;
                Focus();
                BringToFront();
                TopMost = false;
            }

            SetCueBanner(txtHost, Strings.Get("cue.host"));
            SetCueBanner(txtPath, Strings.Get("cue.path"));
            SetCueBanner(txtOpenRelays, Strings.Get("cue.relays"));
            SetCueBanner(txtCloseRelays, Strings.Get("cue.relays"));

            if (firstRun)
                btnNext.Focus();
        }

        private void SetupDialogForm_Resize(object sender, EventArgs e)
        {
            // Wrap the explanatory labels at the width of the window.
            int tabWidth = Math.Max(200, tabWizard.ClientSize.Width - 60);
            int fullWidth = Math.Max(200, ClientSize.Width - 24);
            Label[] wrapped = { lblHostHint, lblTestNote, lblRegexHint, lblExplain, lblAdvHint, lblSimWarn, lblLinked };
            foreach (Label label in wrapped)
                label.MaximumSize = new Size(tabWidth, 0);
            lblStatus.MaximumSize = new Size(fullWidth, 0);
            lblSubtitle.MaximumSize = new Size(Math.Max(200, ClientSize.Width - 140), 0);
        }

        private static void SetCueBanner(TextBox box, string text)
        {
            IntPtr handle = box.Handle; // forces handle creation
            SendMessage(handle, EM_SETCUEBANNER, (IntPtr)1, text);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);

        /// <summary>Version of the driver assembly as shown in the header.</summary>
        internal static string DriverVersionText()
        {
            try
            {
                return typeof(SetupDialogForm).Assembly.GetName().Version.ToString(3);
            }
            catch (Exception)
            {
                return "?";
            }
        }

        // ------------------------------------------------------------------ wizard navigation

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (tabWizard.SelectedIndex > 0)
                tabWizard.SelectedIndex--;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (tabWizard.SelectedIndex < tabWizard.TabCount - 1)
                tabWizard.SelectedIndex++;
        }

        private void tabWizard_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAll();
        }

        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            grpAdvanced.Visible = !grpAdvanced.Visible;
            bool show = grpAdvanced.Visible;
            tlpCommands.RowStyles[2] = show ? new RowStyle(SizeType.Percent, 40F) : new RowStyle(SizeType.Percent, 100F);
            tlpCommands.RowStyles[4] = show ? new RowStyle(SizeType.Percent, 60F) : new RowStyle(SizeType.Absolute, 0F);
        }

        // ------------------------------------------------------------------ UI <-> configuration

        private void LoadUiFromConfig()
        {
            bool was = loading;
            loading = true;
            try
            {
                EnsureActions();

                txtHost.Text = cfg.Host ?? "";
                chkHttps.Checked = cfg.UseHttps;
                cmbAuth.SelectedIndex = cfg.Auth == AuthMode.Basic ? 1 : 0;
                txtUser.Text = cfg.Username ?? "";
                txtPass.Text = "";
                lblPassHint.Text = Strings.Get(cfg.PasswordProtected.Length > 0 ? "conn.pass_saved" : "conn.pass_none");

                txtPath.Text = cfg.QuickPathTemplate ?? "";
                txtOpenRelays.Text = cfg.QuickOpenRelays ?? "";
                txtCloseRelays.Text = cfg.QuickCloseRelays ?? "";
                chkParallel.Checked = cfg.OpenAction.Parallel;
                numDelay.Value = ClampValue(FirstStepDelay(), 0, 60000);

                numOpenTravel.Value = ClampValue(cfg.OpenTravelSeconds, 0, 900);
                numCloseTravel.Value = ClampValue(cfg.CloseTravelSeconds, 0, 900);
                numTimeout.Value = ClampValue(cfg.RequestTimeoutMs, 0, 60000);
                txtRegex.Text = cfg.ExpectedResponseRegex ?? "";
                cmbAssumed.SelectedIndex = cfg.AssumedInitialState == ShutterPhase.Open ? 1 : 0;

                chkSimulation.Checked = cfg.SimulationMode;
                chkTrace.Checked = cfg.TraceEnabled;

                cmbStepAction.SelectedIndex = 0;
                LoadGridFromConfig();
                linked = DetectLinked();
            }
            finally
            {
                loading = was;
            }
            UpdateAll();
        }

        /// <summary>Copies the simple fields from the controls into the private configuration (not the password, not the steps).</summary>
        private void PullFieldsToConfig()
        {
            EnsureActions();
            cfg.Host = txtHost.Text.Trim();
            cfg.UseHttps = chkHttps.Checked;
            cfg.Auth = cmbAuth.SelectedIndex == 1 ? AuthMode.Basic : AuthMode.None;
            cfg.Username = txtUser.Text.Trim();
            cfg.QuickPathTemplate = txtPath.Text.Trim();
            cfg.QuickOpenRelays = txtOpenRelays.Text.Trim();
            cfg.QuickCloseRelays = txtCloseRelays.Text.Trim();
            cfg.RequestTimeoutMs = (int)numTimeout.Value;
            cfg.ExpectedResponseRegex = txtRegex.Text.Trim();
            cfg.OpenTravelSeconds = (int)numOpenTravel.Value;
            cfg.CloseTravelSeconds = (int)numCloseTravel.Value;
            cfg.AssumedInitialState = cmbAssumed.SelectedIndex == 1 ? ShutterPhase.Open : ShutterPhase.Closed;
            cfg.SimulationMode = chkSimulation.Checked;
            cfg.TraceEnabled = chkTrace.Checked;
            cfg.OpenAction.Parallel = chkParallel.Checked;
            cfg.CloseAction.Parallel = chkParallel.Checked;
        }

        private void EnsureActions()
        {
            if (cfg.OpenAction == null) cfg.OpenAction = new DomeAction();
            if (cfg.CloseAction == null) cfg.CloseAction = new DomeAction();
            if (cfg.OpenAction.Steps == null) cfg.OpenAction.Steps = new List<CommandStep>();
            if (cfg.CloseAction.Steps == null) cfg.CloseAction.Steps = new List<CommandStep>();
        }

        private DomeAction ActiveAction()
        {
            return cmbStepAction.SelectedIndex == 1 ? cfg.CloseAction : cfg.OpenAction;
        }

        private int FirstStepDelay()
        {
            DomeAction action = cfg.OpenAction.Steps.Count > 0 ? cfg.OpenAction : cfg.CloseAction;
            return action.Steps.Count > 0 ? action.Steps[0].DelayAfterMs : 0;
        }

        private static decimal ClampValue(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        // ------------------------------------------------------------------ quick builder and steps grid

        /// <summary>Builds one action from the quick builder fields, or an empty action if path or relays are missing.</summary>
        private DomeAction BuildQuickAction(string relays, bool parallel, int delay)
        {
            string path = txtPath.Text.Trim();
            if (path.Length == 0 || (relays ?? "").Trim().Length == 0)
                return new DomeAction { Parallel = parallel };
            return DomeAction.FromTemplate(path, relays, parallel, delay);
        }

        /// <summary>True when the current steps are exactly what the quick builder would generate.</summary>
        private bool DetectLinked()
        {
            int delay = FirstStepDelay();
            bool parallel = cfg.OpenAction.Parallel;
            return SameAction(cfg.OpenAction, BuildQuickAction(txtOpenRelays.Text, parallel, delay))
                && SameAction(cfg.CloseAction, BuildQuickAction(txtCloseRelays.Text, parallel, delay));
        }

        private static bool SameAction(DomeAction a, DomeAction b)
        {
            if (a.Parallel != b.Parallel || a.Steps.Count != b.Steps.Count)
                return false;
            for (int i = 0; i < a.Steps.Count; i++)
            {
                if (a.Steps[i].UrlTemplate != b.Steps[i].UrlTemplate || a.Steps[i].DelayAfterMs != b.Steps[i].DelayAfterMs)
                    return false;
            }
            return true;
        }

        /// <summary>Regenerates both actions from the quick builder fields and refreshes the grid.</summary>
        private void RegenerateFromQuick()
        {
            int delay = (int)numDelay.Value;
            bool parallel = chkParallel.Checked;
            cfg.OpenAction = BuildQuickAction(txtOpenRelays.Text, parallel, delay);
            cfg.CloseAction = BuildQuickAction(txtCloseRelays.Text, parallel, delay);
            LoadGridFromConfig();
        }

        private void LoadGridFromConfig()
        {
            bool was = loading;
            loading = true;
            try
            {
                gridRows.Clear();
                foreach (CommandStep step in ActiveAction().Steps)
                {
                    if (step == null) continue;
                    gridRows.Add(new StepRow { UrlTemplate = step.UrlTemplate ?? "", DelayAfterMs = step.DelayAfterMs });
                }
            }
            finally
            {
                loading = was;
            }
        }

        private void SyncGridToConfig()
        {
            List<CommandStep> steps = new List<CommandStep>();
            foreach (StepRow row in gridRows)
                steps.Add(new CommandStep { UrlTemplate = row.UrlTemplate ?? "", DelayAfterMs = row.DelayAfterMs });
            ActiveAction().Steps = steps;
        }

        private void Quick_Changed(object sender, EventArgs e)
        {
            if (loading) return;
            if (linked)
                RegenerateFromQuick();
            UpdateAll();
        }

        private void btnRelink_Click(object sender, EventArgs e)
        {
            if (!linked)
            {
                DialogResult answer = MessageBox.Show(this, Strings.Get("cmd.relink_confirm"), Strings.Get("title.form"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (answer != DialogResult.Yes)
                    return;
            }
            linked = true;
            RegenerateFromQuick();
            UpdateAll();
        }

        private void cmbStepAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading) return;
            LoadGridFromConfig();
        }

        private void gridRows_ListChanged(object sender, ListChangedEventArgs e)
        {
            // Only user edits reach this point: programmatic reloads run with 'loading' set.
            if (loading) return;
            linked = false;
            SyncGridToConfig();
            UpdateAll();
        }

        private void dgvSteps_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // A non numeric delay is rejected by the grid: keep the previous value instead of throwing.
            e.ThrowException = false;
        }

        // ------------------------------------------------------------------ connection tab

        private void Field_Changed(object sender, EventArgs e)
        {
            if (loading) return;
            UpdateAll();
        }

        private void txtPass_TextChanged(object sender, EventArgs e)
        {
            if (loading) return;
            if (txtPass.Text.Length > 0)
                lblPassHint.Text = Strings.Get("conn.pass_will_change");
            else
                lblPassHint.Text = Strings.Get(cfg.PasswordProtected.Length > 0 ? "conn.pass_saved" : "conn.pass_none");
        }

        private void chkShowPass_CheckedChanged(object sender, EventArgs e)
        {
            txtPass.PasswordChar = chkShowPass.Checked ? '\0' : '*';
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            string host = txtHost.Text.Trim();
            if (host.Length == 0)
            {
                SetTestResult(Strings.Get("conn.test_need_host"), Color.Firebrick);
                return;
            }

            bool https = chkHttps.Checked;
            const int timeoutMs = 5000;
            TcpCheck check = new TcpCheck();

            btnTest.Enabled = false;
            SetTestResult(Strings.Get("conn.test_running"), SystemColors.GrayText);
            try
            {
                // Plain TCP connect on a worker thread: no HTTP request, no command is sent.
                await Task.Run(() =>
                {
                    string error;
                    check.Ok = Reachability.TryTcpConnect(host, https, timeoutMs, out error);
                    check.Error = error;
                });
            }
            catch (Exception ex)
            {
                check.Ok = false;
                check.Error = ex.Message;
            }

            if (IsDisposed)
                return;
            btnTest.Enabled = true;
            if (check.Ok)
                SetTestResult(Strings.Get("conn.test_ok"), Color.DarkGreen);
            else
                SetTestResult(Strings.Format("conn.test_fail", check.Error ?? ""), Color.Firebrick);
        }

        private void SetTestResult(string text, Color color)
        {
            lblTestResult.Text = text;
            lblTestResult.ForeColor = color;
        }

        private sealed class TcpCheck
        {
            public bool Ok;
            public string Error;
        }

        // ------------------------------------------------------------------ validation, preview, buttons

        private bool ValidateConfig(out List<string> problems)
        {
            try
            {
                problems = new List<string>();
                return cfg.Validate(out problems) && problems.Count == 0;
            }
            catch (Exception ex)
            {
                problems = new List<string> { ex.Message };
                return false;
            }
        }

        /// <summary>Refreshes everything that depends on the configuration: status, preview, buttons.</summary>
        private void UpdateAll()
        {
            if (loading) return;
            PullFieldsToConfig();

            List<string> problems;
            bool valid = ValidateConfig(out problems);

            // status strip
            string text;
            Color color;
            if (valid && cfg.SimulationMode)
            {
                text = Strings.Get("status.sim_ready");
                color = Color.DarkOrange;
            }
            else if (valid)
            {
                text = Strings.Get("status.ready");
                color = Color.DarkGreen;
            }
            else
            {
                text = Strings.Format("status.invalid", string.Join("; ", problems));
                color = Color.Firebrick;
            }
            if (firstRun && !valid)
                text = Strings.Get("status.first_run") + Environment.NewLine + text;
            lblStatus.Text = text;
            lblStatus.ForeColor = color;

            // buttons: OK waits for a valid configuration, except in simulation mode
            btnOK.Enabled = valid || cfg.SimulationMode;
            btnBack.Enabled = tabWizard.SelectedIndex > 0;
            btnNext.Enabled = tabWizard.SelectedIndex < tabWizard.TabCount - 1;
            AcceptButton = (firstRun && !valid && btnNext.Enabled) ? btnNext : btnOK;

            lblLinked.Text = Strings.Get(linked ? "cmd.linked" : "cmd.unlinked");
            UpdatePreview();
        }

        /// <summary>Shows the exact URLs that would be called, built with the host currently typed. Nothing is sent.</summary>
        private void UpdatePreview()
        {
            if (cfg.Host.Length == 0)
            {
                txtPreview.Text = Strings.Get("cmd.preview_nohost");
                return;
            }

            StringBuilder sb = new StringBuilder();
            AppendPreview(sb, "cmd.open_label", linked ? BuildQuickAction(txtOpenRelays.Text, chkParallel.Checked, (int)numDelay.Value) : cfg.OpenAction);
            sb.AppendLine();
            AppendPreview(sb, "cmd.close_label", linked ? BuildQuickAction(txtCloseRelays.Text, chkParallel.Checked, (int)numDelay.Value) : cfg.CloseAction);
            txtPreview.Text = sb.ToString();
        }

        private void AppendPreview(StringBuilder sb, string labelKey, DomeAction action)
        {
            string mode = Strings.Get(action.Parallel ? "cmd.mode_parallel" : "cmd.mode_sequential");
            sb.AppendLine(Strings.Get(labelKey) + " (" + mode + ")");
            if (action.Steps.Count == 0)
            {
                sb.AppendLine("   " + Strings.Get("cmd.preview_empty"));
                return;
            }
            for (int i = 0; i < action.Steps.Count; i++)
            {
                string url;
                try
                {
                    url = cfg.ExpandUrl(action.Steps[i].UrlTemplate ?? "");
                }
                catch (Exception)
                {
                    url = action.Steps[i].UrlTemplate;
                }
                sb.AppendLine("   " + Strings.Format("cmd.preview_line", i + 1, url));
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            PullFieldsToConfig();
            List<string> problems;
            bool valid = ValidateConfig(out problems);
            if (!valid && !cfg.SimulationMode)
            {
                UpdateAll();
                return;
            }

            // Re-protect the password only when the box was changed; an empty box keeps the stored one.
            if (txtPass.Text.Length > 0)
            {
                try
                {
                    cfg.SetPassword(txtPass.Text, protector);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, Strings.Format("msg.error_detail", ex.Message), Strings.Get("msg.error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            txtPass.Text = "";

            Result = cfg.Clone();
            if (tl != null)
                tl.LogMessage("Setup", "Configuration updated from the setup dialogue (password not logged)");
            DialogResult = DialogResult.OK;
        }

        // ------------------------------------------------------------------ options tab: import / export

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = Strings.Get("opt.filter");
                dlg.CheckFileExists = true;
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    DomeConfig imported = ConfigSerializer.Import(dlg.FileName) ?? new DomeConfig();
                    cfg = imported;
                    EnsureActions();
                    LoadUiFromConfig();
                    MessageBox.Show(this, Strings.Get("opt.import_done"), Strings.Get("title.form"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, Strings.Format("opt.import_fail", ex.Message), Strings.Get("msg.error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = Strings.Get("opt.filter");
                dlg.FileName = "clamshell-dome.json";
                dlg.DefaultExt = "json";
                dlg.AddExtension = true;
                dlg.OverwritePrompt = true;
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                // Default is No: the encrypted password stays out of the file unless explicitly asked.
                DialogResult answer = MessageBox.Show(this, Strings.Get("opt.export_pass_q"), Strings.Get("title.form"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                bool includePassword = answer == DialogResult.Yes;

                try
                {
                    PullFieldsToConfig();
                    ConfigSerializer.Export(cfg, dlg.FileName, includePassword);
                    MessageBox.Show(this, Strings.Get("opt.export_done"), Strings.Get("title.form"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, Strings.Format("opt.export_fail", ex.Message), Strings.Get("msg.error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void lnkDocs_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkDocs.LinkVisited = true;
            OpenUrl(Strings.DocsUrl);
        }

        // ------------------------------------------------------------------ header

        private void BrowseToAscom(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            OpenUrl(Strings.AscomUrl);
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, Strings.Format("msg.ascom_fail", ex.Message), Strings.Get("msg.error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
