using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AsusCloud.Storage;
using Newtonsoft.Json;

namespace AsusCloudStorage_Explorer_Sample
{
    public partial class frmMain : Form
    {
        private ACSDocumentManager acsDocMgr;
        ACSObject[] acsObjs;
        private string SID = "";
        private string progkey = "";
        private string username = "";
        private string password = "";

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnLogIn_Click(object sender, EventArgs e)
        {
            this.btnLogIn.Enabled = false;

            acsDocMgr = new ACSDocumentManager(SID, progkey, username, password);

            if (acsDocMgr.ErrorCode != (Int32)ACS_ErrorCode.OK) {
                //起始物件失敗，可抓取錯誤訊息
                lstAccountInfo.Items.Add("Login Error, status = "+acsDocMgr.ErrorCode);
                Int32 errorCode = acsDocMgr.ErrorCode;
                this.btnLogIn.Enabled = true;
                return;
            }
            lstAccountInfo.Items.Add("Login Successful.");

            this.btnLogIn.Enabled = true;
            //Todo 抓取帳戶資訊

            //清除list view的內容
            this.lstMySync.Items.Clear();

            this.tabExplorer.SelectedTab = tabMySync;
            //清楚列出檔案及資料夾

            getMySyncFolderList();
        }

        private void getMySyncFolderList()
        {
            acsObjs = null;
            acsObjs =  acsDocMgr.BrowseFolder("/MySync", ACS_LIST_OPTION.ALL);

            foreach (ACSObject obj in acsObjs)
            {
                this.lstMySync.Items.Add(obj.Name);
            }

        }

        private void lstMySync_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void lstAccountInfo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtWebStorageID_TextChanged(object sender, EventArgs e)
        {
            username = txtWebStorageID.Text;
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            password = txtPassword.Text;
        }

        private void txtSID_TextChanged(object sender, EventArgs e)
        {
            SID = txtSID.Text;
        }

        private void txtProgKey_TextChanged(object sender, EventArgs e)
        {
            progkey = txtProgKey.Text;
        }

       
    }
}
