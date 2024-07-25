
namespace ASCOM.Clamshell.Dome
{
    partial class SetupDialogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.picASCOM = new System.Windows.Forms.PictureBox();
            this.IPAddress = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textbox_password = new System.Windows.Forms.TextBox();
            this.textbox_username = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textbox_su = new System.Windows.Forms.TextBox();
            this.textbox_nu = new System.Windows.Forms.TextBox();
            this.textbox_sd = new System.Windows.Forms.TextBox();
            this.textbox_nd = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.advancedpanel = new System.Windows.Forms.Panel();
            this.advancedbutton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).BeginInit();
            this.advancedpanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(305, 380);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(59, 24);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.CmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(305, 410);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(59, 25);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.CmdCancel_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(24, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 31);
            this.label1.TabIndex = 2;
            this.label1.Text = "Clamshell Driver v0.2 by Robert Panai";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // picASCOM
            // 
            this.picASCOM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picASCOM.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picASCOM.Image = ((System.Drawing.Image)(resources.GetObject("picASCOM.Image")));
            this.picASCOM.Location = new System.Drawing.Point(316, 9);
            this.picASCOM.Name = "picASCOM";
            this.picASCOM.Size = new System.Drawing.Size(48, 56);
            this.picASCOM.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picASCOM.TabIndex = 3;
            this.picASCOM.TabStop = false;
            this.picASCOM.Click += new System.EventHandler(this.BrowseToAscom);
            this.picASCOM.DoubleClick += new System.EventHandler(this.BrowseToAscom);
            // 
            // IPAddress
            // 
            this.IPAddress.Location = new System.Drawing.Point(117, 63);
            this.IPAddress.Name = "IPAddress";
            this.IPAddress.Size = new System.Drawing.Size(100, 20);
            this.IPAddress.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "IP Address";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // textbox_password
            // 
            this.textbox_password.Location = new System.Drawing.Point(117, 142);
            this.textbox_password.Name = "textbox_password";
            this.textbox_password.PasswordChar = '*';
            this.textbox_password.Size = new System.Drawing.Size(100, 20);
            this.textbox_password.TabIndex = 8;
            // 
            // textbox_username
            // 
            this.textbox_username.Location = new System.Drawing.Point(117, 103);
            this.textbox_username.Name = "textbox_username";
            this.textbox_username.Size = new System.Drawing.Size(100, 20);
            this.textbox_username.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "username";
            this.label3.Click += new System.EventHandler(this.label3_Click_1);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 145);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "password";
            // 
            // textbox_su
            // 
            this.textbox_su.Location = new System.Drawing.Point(108, 39);
            this.textbox_su.Name = "textbox_su";
            this.textbox_su.Size = new System.Drawing.Size(27, 20);
            this.textbox_su.TabIndex = 12;
            // 
            // textbox_nu
            // 
            this.textbox_nu.Location = new System.Drawing.Point(108, 13);
            this.textbox_nu.Name = "textbox_nu";
            this.textbox_nu.Size = new System.Drawing.Size(27, 20);
            this.textbox_nu.TabIndex = 13;
            // 
            // textbox_sd
            // 
            this.textbox_sd.Location = new System.Drawing.Point(108, 91);
            this.textbox_sd.Name = "textbox_sd";
            this.textbox_sd.Size = new System.Drawing.Size(27, 20);
            this.textbox_sd.TabIndex = 14;
            this.textbox_sd.TextChanged += new System.EventHandler(this.textbox_sd_TextChanged);
            // 
            // textbox_nd
            // 
            this.textbox_nd.Location = new System.Drawing.Point(108, 65);
            this.textbox_nd.Name = "textbox_nd";
            this.textbox_nd.Size = new System.Drawing.Size(27, 20);
            this.textbox_nd.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Nord Up Relè";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 39);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Sud Up Relè";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 65);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(86, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Nord Down Relè";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 96);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(82, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "Sud Down Relè";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // advancedpanel
            // 
            this.advancedpanel.Controls.Add(this.textbox_nu);
            this.advancedpanel.Controls.Add(this.label8);
            this.advancedpanel.Controls.Add(this.textbox_su);
            this.advancedpanel.Controls.Add(this.label7);
            this.advancedpanel.Controls.Add(this.textbox_nd);
            this.advancedpanel.Controls.Add(this.label6);
            this.advancedpanel.Controls.Add(this.textbox_sd);
            this.advancedpanel.Controls.Add(this.label5);
            this.advancedpanel.Location = new System.Drawing.Point(27, 243);
            this.advancedpanel.Name = "advancedpanel";
            this.advancedpanel.Size = new System.Drawing.Size(165, 170);
            this.advancedpanel.TabIndex = 20;
            this.advancedpanel.Visible = false;
            // 
            // advancedbutton
            // 
            this.advancedbutton.Location = new System.Drawing.Point(26, 201);
            this.advancedbutton.Name = "advancedbutton";
            this.advancedbutton.Size = new System.Drawing.Size(75, 23);
            this.advancedbutton.TabIndex = 21;
            this.advancedbutton.Text = "Advanced ";
            this.advancedbutton.UseVisualStyleBackColor = true;
            this.advancedbutton.Click += new System.EventHandler(this.advancedbutton_Click);
            // 
            // SetupDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 443);
            this.Controls.Add(this.advancedbutton);
            this.Controls.Add(this.advancedpanel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textbox_username);
            this.Controls.Add(this.textbox_password);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.IPAddress);
            this.Controls.Add(this.picASCOM);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupDialogForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Clamshell Setup";
            this.Load += new System.EventHandler(this.SetupDialogForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).EndInit();
            this.advancedpanel.ResumeLayout(false);
            this.advancedpanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox picASCOM;
        private System.Windows.Forms.TextBox IPAddress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textbox_password;
        private System.Windows.Forms.TextBox textbox_username;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textbox_su;
        private System.Windows.Forms.TextBox textbox_nu;
        private System.Windows.Forms.TextBox textbox_sd;
        private System.Windows.Forms.TextBox textbox_nd;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel advancedpanel;
        private System.Windows.Forms.Button advancedbutton;
    }
}