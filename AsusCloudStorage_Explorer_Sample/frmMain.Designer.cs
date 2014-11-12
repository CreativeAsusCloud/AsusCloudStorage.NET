namespace AsusCloudStorage_Explorer_Sample
{
    partial class frmMain
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.lstAccountInfo = new System.Windows.Forms.ListBox();
            this.btnLogIn = new System.Windows.Forms.Button();
            this.txtProgKey = new System.Windows.Forms.TextBox();
            this.txtSID = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtWebStorageID = new System.Windows.Forms.TextBox();
            this.lblProgKey = new System.Windows.Forms.Label();
            this.lblSID = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblID = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tabExplorer = new System.Windows.Forms.TabControl();
            this.lstMySync = new System.Windows.Forms.ListBox();
            this.tabMySync = new System.Windows.Forms.TabPage();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabExplorer.SuspendLayout();
            this.tabMySync.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Highlight;
            this.panel1.Controls.Add(this.lstAccountInfo);
            this.panel1.Controls.Add(this.btnLogIn);
            this.panel1.Controls.Add(this.txtProgKey);
            this.panel1.Controls.Add(this.txtSID);
            this.panel1.Controls.Add(this.txtPassword);
            this.panel1.Controls.Add(this.txtWebStorageID);
            this.panel1.Controls.Add(this.lblProgKey);
            this.panel1.Controls.Add(this.lblSID);
            this.panel1.Controls.Add(this.lblPassword);
            this.panel1.Controls.Add(this.lblID);
            this.panel1.Location = new System.Drawing.Point(-1, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(247, 423);
            this.panel1.TabIndex = 0;
            // 
            // lstAccountInfo
            // 
            this.lstAccountInfo.FormattingEnabled = true;
            this.lstAccountInfo.ItemHeight = 12;
            this.lstAccountInfo.Location = new System.Drawing.Point(16, 231);
            this.lstAccountInfo.MultiColumn = true;
            this.lstAccountInfo.Name = "lstAccountInfo";
            this.lstAccountInfo.Size = new System.Drawing.Size(215, 172);
            this.lstAccountInfo.TabIndex = 9;
            this.lstAccountInfo.SelectedIndexChanged += new System.EventHandler(this.lstAccountInfo_SelectedIndexChanged);
            // 
            // btnLogIn
            // 
            this.btnLogIn.Location = new System.Drawing.Point(95, 178);
            this.btnLogIn.Name = "btnLogIn";
            this.btnLogIn.Size = new System.Drawing.Size(75, 23);
            this.btnLogIn.TabIndex = 8;
            this.btnLogIn.Text = "Log In";
            this.btnLogIn.UseVisualStyleBackColor = true;
            this.btnLogIn.Click += new System.EventHandler(this.btnLogIn_Click);
            // 
            // txtProgKey
            // 
            this.txtProgKey.Location = new System.Drawing.Point(95, 131);
            this.txtProgKey.Name = "txtProgKey";
            this.txtProgKey.Size = new System.Drawing.Size(100, 22);
            this.txtProgKey.TabIndex = 7;
            this.txtProgKey.TextChanged += new System.EventHandler(this.txtProgKey_TextChanged);
            // 
            // txtSID
            // 
            this.txtSID.Location = new System.Drawing.Point(95, 97);
            this.txtSID.Name = "txtSID";
            this.txtSID.Size = new System.Drawing.Size(100, 22);
            this.txtSID.TabIndex = 6;
            this.txtSID.TextChanged += new System.EventHandler(this.txtSID_TextChanged);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(95, 60);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(100, 22);
            this.txtPassword.TabIndex = 5;
            this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
            // 
            // txtWebStorageID
            // 
            this.txtWebStorageID.Location = new System.Drawing.Point(95, 27);
            this.txtWebStorageID.Name = "txtWebStorageID";
            this.txtWebStorageID.Size = new System.Drawing.Size(100, 22);
            this.txtWebStorageID.TabIndex = 4;
            this.txtWebStorageID.TextChanged += new System.EventHandler(this.txtWebStorageID_TextChanged);
            // 
            // lblProgKey
            // 
            this.lblProgKey.AutoSize = true;
            this.lblProgKey.ForeColor = System.Drawing.Color.White;
            this.lblProgKey.Location = new System.Drawing.Point(13, 135);
            this.lblProgKey.Name = "lblProgKey";
            this.lblProgKey.Size = new System.Drawing.Size(46, 12);
            this.lblProgKey.TabIndex = 3;
            this.lblProgKey.Text = "ProgKey";
            // 
            // lblSID
            // 
            this.lblSID.AutoSize = true;
            this.lblSID.ForeColor = System.Drawing.Color.White;
            this.lblSID.Location = new System.Drawing.Point(14, 102);
            this.lblSID.Name = "lblSID";
            this.lblSID.Size = new System.Drawing.Size(23, 12);
            this.lblSID.TabIndex = 2;
            this.lblSID.Text = "SID";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.ForeColor = System.Drawing.Color.White;
            this.lblPassword.Location = new System.Drawing.Point(14, 64);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(48, 12);
            this.lblPassword.TabIndex = 1;
            this.lblPassword.Text = "Password";
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.ForeColor = System.Drawing.Color.White;
            this.lblID.Location = new System.Drawing.Point(14, 32);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(74, 12);
            this.lblID.TabIndex = 0;
            this.lblID.Text = "WebStorageID";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.tabExplorer);
            this.panel2.Location = new System.Drawing.Point(245, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(463, 423);
            this.panel2.TabIndex = 1;
            // 
            // tabExplorer
            // 
            this.tabExplorer.Controls.Add(this.tabMySync);
            this.tabExplorer.ImeMode = System.Windows.Forms.ImeMode.On;
            this.tabExplorer.Location = new System.Drawing.Point(0, 0);
            this.tabExplorer.Name = "tabExplorer";
            this.tabExplorer.SelectedIndex = 0;
            this.tabExplorer.Size = new System.Drawing.Size(460, 423);
            this.tabExplorer.TabIndex = 0;
            // 
            // lstMySync
            // 
            this.lstMySync.FormattingEnabled = true;
            this.lstMySync.ItemHeight = 12;
            this.lstMySync.Location = new System.Drawing.Point(6, 13);
            this.lstMySync.Name = "lstMySync";
            this.lstMySync.Size = new System.Drawing.Size(439, 376);
            this.lstMySync.TabIndex = 0;
            this.lstMySync.SelectedIndexChanged += new System.EventHandler(this.lstMySync_SelectedIndexChanged);
            // 
            // tabMySync
            // 
            this.tabMySync.Controls.Add(this.lstMySync);
            this.tabMySync.Location = new System.Drawing.Point(4, 22);
            this.tabMySync.Name = "tabMySync";
            this.tabMySync.Padding = new System.Windows.Forms.Padding(3);
            this.tabMySync.Size = new System.Drawing.Size(452, 397);
            this.tabMySync.TabIndex = 0;
            this.tabMySync.Text = "MySyncFolder";
            this.tabMySync.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(696, 411);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(712, 450);
            this.MinimumSize = new System.Drawing.Size(712, 450);
            this.Name = "frmMain";
            this.Text = "AsusCloudStorage_Explorer_Sample";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tabExplorer.ResumeLayout(false);
            this.tabMySync.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnLogIn;
        private System.Windows.Forms.TextBox txtProgKey;
        private System.Windows.Forms.TextBox txtSID;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtWebStorageID;
        private System.Windows.Forms.Label lblProgKey;
        private System.Windows.Forms.Label lblSID;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.ListBox lstAccountInfo;
        private System.Windows.Forms.TabControl tabExplorer;
        private System.Windows.Forms.TabPage tabMySync;
        private System.Windows.Forms.ListBox lstMySync;
        
    }
}

