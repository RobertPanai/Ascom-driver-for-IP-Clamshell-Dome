using ASCOM.Utilities;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ASCOM.Clamshell.Dome
{
    [ComVisible(false)] // Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        const string NO_PORTS_MESSAGE = "No COM ports found";
        TraceLogger tl; // Holder for a reference to the driver's trace logger

        public SetupDialogForm(TraceLogger tlDriver)
        {
            InitializeComponent();

            // Save the provided trace logger for use within the setup dialogue
            tl = tlDriver;

            // Initialise current values of user settings from the ASCOM Profile
            InitUI();
        }

        private void CmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            // Place any validation constraint checks here and update the state variables with results from the dialogue
            DomeHardware.comPort = IPAddress.Text;
            DomeHardware.Password = textbox_password.Text;
            DomeHardware.Username = textbox_username.Text;
            DomeHardware.Relè_NU = textbox_nu.Text;
            DomeHardware.Relè_SU = textbox_su.Text;
            DomeHardware.Relè_ND = textbox_nd.Text;
            DomeHardware.Relè_SD = textbox_sd.Text;

            DomeHardware.WriteProfile();
            //tl.Enabled = chkTrace.Checked;

            // Update the COM port variable if one has been selected
            //if (comboBoxComPort.SelectedItem is null) // No COM port selected
            //{
            //    tl.LogMessage("Setup OK", $"New configuration values - COM Port: Not selected");
            //}
            //else if (comboBoxComPort.SelectedItem.ToString() == NO_PORTS_MESSAGE)
            //{
            //    tl.LogMessage("Setup OK", $"New configuration values - NO COM ports detected on this PC.");
            //}
            //else // A valid COM port has been selected
            //{
            //    DomeHardware.comPort = (string)comboBoxComPort.SelectedItem;
            //    tl.LogMessage("Setup OK", $"New configuration values - COM Port: {comboBoxComPort.SelectedItem}");
            //}
        }

        private void CmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            try
            {
                System.Diagnostics.Process.Start("https://ascom-standards.org/");
            }
            catch (Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void InitUI()
        {

            IPAddress.Text = DomeHardware.comPort;
            textbox_password.Text = DomeHardware.Password;
            textbox_username.Text = DomeHardware.Username;
            textbox_nu.Text = DomeHardware.Relè_NU;
            textbox_su.Text = DomeHardware.Relè_SU;
            textbox_nd.Text = DomeHardware.Relè_ND;
            textbox_sd.Text = DomeHardware.Relè_SD;
        }

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
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textbox_sd_TextChanged(object sender, EventArgs e)
        {

        }


        private void advancedbutton_Click(object sender, EventArgs e)
        {
            advancedpanel.Visible = !advancedpanel.Visible;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}