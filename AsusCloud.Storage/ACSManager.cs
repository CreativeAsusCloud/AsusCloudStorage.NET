using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.Xml;
using System.IO;
using aws;

namespace AsusCloud.Storage
{
    public class ACSObject
    {
        string ID;
        string parentID;
        string objName;
        string isGroupAware;
        string fileSize;
        string treeSize;
        string isBackup;
        string isInfected;
        string isShared;
        string headVersion;
        string folderOwner;
        string objCreatedTime;
        bool   isFileType;

        public string objID
        {
            set
            {
                this.ID = value;
            }

            get
            {
                return this.ID;
            }
        }
        
        public bool isFIle
        {
            get
            {
                return this.isFileType;
            }
            set
            {
                this.isFileType = value;
            }
        }
        public UInt64 FileSize
        {
            get 
            {
                return Convert.ToUInt64( fileSize );
            }

            set
            {
                this.fileSize = value.ToString();
            }
        }
        public string Name
        {
            get {
                return this.objName;
            }

            set
            {
                this.objName = value;
            }
        }

        public bool isGroup
        {
            get {
                bool isTrue;
                if ( this.isGroupAware == "1")
                    isTrue = true;
                else
                    isTrue = false;

                return isTrue;
            }
            set
            {
                if (value)
                    this.isGroupAware = "1";
                else
                    this.isGroupAware = "0";
            }
        }

        public bool isBackupType
        {
            get 
            {
               bool isTrue;
                if ( this.isBackup == "1")
                    isTrue = true;
                else
                    isTrue = false;

                return isTrue;
            }

            set
            {
                if (value)
                    this.isBackup = "1";
                else
                    this.isBackup = "0";
            }
        }

        public bool isInfectedByVirus
        {
            get 
            {
               bool isTrue;
                if ( this.isInfected == "1")
                    isTrue = true;
                else
                    isTrue = false;

                return isTrue;
            }
        }
        public bool isSharedToOthers
        {
            get 
            {
               bool isTrue;
                if ( this.isShared == "1")
                    isTrue = true;
                else
                    isTrue = false;

                return isTrue;
            }

            set
            {
                if (value)
                {
                    this.isShared = "1";
                }
                else
                {
                    this.isShared = "0";
                }
            }
        }

        public string Version
        {
            get 
            {
                return this.headVersion;
            }
        }

        public string CreatedTime
        {
            get 
            {
               return this.objCreatedTime;
            }

            set
            {
                this.objCreatedTime = value;
            }
        }

        public string FolderOwner
        {
            get
            {
                return this.folderOwner;
            }
        }

        public UInt64 FolderTreeSize
        {
            get
            {
                return Convert.ToUInt64(this.treeSize);
            }

            set
            {
                this.treeSize = value.ToString();
            }
        }

        
    }

    public enum ACS_ErrorCode
    {
        EXCEPTION_OCCURS = -1,
        OK = 0x0,
        USER_AUTHENTICATION_ERROR = 0x00000002,
        SID_OR_PROGKEY_AUTHENTICATE_ERROR = 0x00000101,
        OTP_AUTHENTICATE_ERROR = 0x000001F8,
        OTP_CREDENTIAL_ID_LOCKED = 0x000001F9,
        CAPTCHA_AUTHENTICATE_ERROR = 0x000001FC,
        DIRECTORY_IS_NOT_EXIST = 0x000000DA,
        SID_PROGKEY_IS_NULL = 0x00010001,
        USERNAME_PASSWORD_IS_NULL = 0x00010002,
        AUTHENTICATION_FAIL = 0x00001000,
        UNEXPECTED_ERROR = 0x00008000,
        ACQUIRE_TOKEN_FAIL = 0x00001001,
        POST_DATA_TO_SERVER_FAIL = 0x00002000,
        FILE_IS_EXIST = 0x000003FF,
        FILE_NAME_IS_EMPTY = 0x000000D3,
        FILE_NAME_TOO_LONG = 0x000000D5,
        FILE_NAME_IS_EXIST = 0x000000D6,
        FOLDER_NOT_EXIST_OR_DELETED = 0x000000DA,
        FILE_NOT_EXIST_OR_DELETED = 0x000000DB,
        GENERAL_FILE_ERROR = 0x000000DC,
        SINGLE_FILE_SIZE_OVER_LIMIT = 0x000000DD,
        USER_SPACE_NOT_ENOUGH = 0x000000E0,
        USER_ACCOUNT_FREEZE_OR_CLOSE = 0x000000E2,
        FILE_SIGNATURE_NOT_MATCH_WITH_CLOUD_RECORD = 0x000000FA,
        TRANSACTION_ID_NOT_EXIST = 0x0000FB, 
        TRANSACTION_ID_NOT_MATCH_FILE_ID = 0x000000FC
    }
    public enum ACS_LIST_OPTION
    {
        ALL = 0x0,
        FILE_ONLY = 0x1,
        DIRECTORY_ONLY = 0x10
    }

    /// <summary>
    /// 在雲端上的根目錄統一為  /MySync, 如果要把一個檔案test.txt送上雲端的根目錄, 寫法就是 upload(test.txt, "/Mysync")
    /// 如果是要送到根目錄下的sub, 寫法就是 upload(test.txt, "/Mysync/sub")
    /// </summary>
    public class ACSDocumentManager
    {
        awsClient awsObj;
        Int32 lastError;
        string mySyncFolderID;
        string myBackFolderID;
        string myCollectionFolderID;

        public Int32 ErrorCode
        {
            get
            {
                return this.lastError;
            }
        }

        /// <summary>
        /// ERROR: POST_DATA_TO_SERVER_FAIL, ACQUIRE_TOKEN_FAIL, UNEXPECTED_ERROR
        /// </summary>
        /// <param name="SID"></param>
        /// <param name="ProgKey"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        public ACSDocumentManager(string SID, string ProgKey, string Username, string Password)
        {
            awsObj = null;
            lastError = 0;
            if ( (SID.Length == 0) || (ProgKey.Length == 0) )
            {
                lastError = (Int32)ACS_ErrorCode.SID_PROGKEY_IS_NULL;
                return;
            }

            if ( (Username.Length == 0) || ( Password.Length ==0 ) )
            {
                lastError = (Int32) ACS_ErrorCode.USERNAME_PASSWORD_IS_NULL;
                return;
            }

            try
            {
                this.awsObj = new awsClient(SID, ProgKey, Username, Password);
            }
            catch (ArgumentException ae)
            {
                this.lastError = (Int32)ACS_ErrorCode.POST_DATA_TO_SERVER_FAIL;
                return;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 0)
                {
                    string errorCode = e.Message.Substring(0,  e.Message.IndexOf(':'));
                    this.lastError = Convert.ToInt32(errorCode);
                    return;
                }
            }

            try
            {
                GetMyRootFoldersID();
            }
            catch (Exception e)
            {
                this.myBackFolderID = null;
                this.myCollectionFolderID = null;
                this.mySyncFolderID = null;
            }

            return;
        }
        public bool IsFileExist(string Path)
        {
            bool isExist = true;

            string[] paths = getPathList(Path);
            string folderid = getLastFolderID(paths);
            string fileid = awsObj.getFileID(paths[paths.Length - 1], folderid);

            if (fileid == null)
                isExist = false;
            else
                isExist = true;
            return isExist;
        }
        public bool IsDirectoryExist(string Path)
        {
            bool isExist = true;

            string[] paths = getPathList(Path);
            string folderid = getLastFolderID(paths);

            if (folderid == null)
                isExist = false;
            else
                isExist = true;
            return isExist;
        }
        //public bool CreateDirectory(string Path)
        //{
        //    bool result = true;

        //    return result;
        //}
        public bool DeleteDirectory(string Path)
        {
            bool result = true;


            string[] paths = getPathList(Path);
            string folderid = getLastFolderID(paths);

            string[] folders = new string[1];
            folders[0] = folderid;

            awsObj.removeFolder(folders);
            

            return result;           
        }
        //public bool RenameDirectory(string Old_Path_DirectoryName, string New_Path_DirectoryName)
        //{
        //    bool result = true;
            
        //    return result;
        //}
        //public bool MoveDirectory(string Source_Path, string Destination_Path)
        //{
        //    bool result = true;

        //    return result;
        //}
        public bool DeleteFile(string Path)
        {
            bool result = true;
            string[] paths = getPathList(Path);
            string folderid = getLastFolderID(paths);
            string fileid = awsObj.getFileID(paths[paths.Length - 1], folderid);

            string[] files = new string[1];
            files[0] = fileid;

            awsObj.removeFile(files);
            return result;
        }
        //public bool MoveFile(string Source_Path, string Destination_Path)
        //{
        //    bool result = true;
            
        //    return result;
        //}
        //public bool CopyFile(string Source_Path, string Destination_Path)
        //{
        //    bool result = true;

        //    return result;
        //}
        //public bool RenameFile(string Old_Path_File_Name, string New_Path_File_Name)
        //{
        //    bool result = true;

        //    return result;
        //}
        public bool Upload(string Local_Path, string Cloud_Path, bool Overwrite=false)
        {
            bool result = true;
            string[] paths = getPathList(Cloud_Path);
            string folderid = getLastFolderID(paths);

            awsObj.uploadFile(Local_Path, folderid, Overwrite);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Cloud_Path">包含檔名的雲端路徑, 如/MySyn/test.txt</param>
        /// <param name="LocalPath">不包含檔名本機路徑，如C:\sub</param>
        /// <returns></returns>
        public bool Download(string Cloud_Path, string LocalPath)
        {
            bool result = true;
            string[] paths = getPathList(Cloud_Path);
            string folderid = getLastFolderID(paths);
            Directory.SetCurrentDirectory(LocalPath);

            awsObj.DownloadFile(paths[paths.Length-1], folderid);

            return result;
        }
        public ACSObject[] BrowseFolder(string Path, ACS_LIST_OPTION Option)
        {
            string result =  null;
            string [] paths = getPathList(Path);
            string folderid = getLastFolderID(paths);

            //用folderid 進行browsefolder
            string xmlResult = awsObj.browseFolder(folderid);
            //將傳回來的xml 打包成物件集合, 並利用ACS_LIST_OPTION 過濾掉不要的資料列
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResult);
            XmlNodeList statusNodes = doc.SelectNodes("/browse/status");

            if (statusNodes[0].InnerText != "0")
            {
                this.lastError = Convert.ToInt32(statusNodes[0].InnerText);
                return null;
            }

            XmlNodeList folderNodes = doc.SelectNodes("/browse/folder");
            XmlNodeList fileNodes = doc.SelectNodes("/browse/file");
            int total_rows = 0;
            switch (Option)
            {
                case ACS_LIST_OPTION.ALL:
                    total_rows = folderNodes.Count + fileNodes.Count;
                    break;
                case ACS_LIST_OPTION.DIRECTORY_ONLY:
                    total_rows = folderNodes.Count;
                    break;
                case ACS_LIST_OPTION.FILE_ONLY:
                    total_rows = fileNodes.Count;
                    break;
                default:
                    return null;
                    break;
            }

            ACSObject[] returnObjs = new ACSObject[total_rows];

            int index = 0;

            if (Option == ACS_LIST_OPTION.ALL || Option == ACS_LIST_OPTION.DIRECTORY_ONLY)
            {
                foreach (XmlNode folder in folderNodes)
                {
                    ACSObject acsObj = new ACSObject();

                    acsObj.objID = folder["id"].InnerText;
                    acsObj.FolderTreeSize = Convert.ToUInt64(folder["treesize"].InnerText);
                    acsObj.Name = folder["display"].InnerText;
                    if (folder["ispublic"].InnerText == "1")
                        acsObj.isSharedToOthers = true;
                    else
                        acsObj.isSharedToOthers = false;

                    if (folder["isbackup"].InnerText == "1")
                        acsObj.isBackupType = true;
                    else
                        acsObj.isBackupType = false;

                    acsObj.CreatedTime = folder["createdtime"].InnerText;

                    if (folder["isgroupaware"].InnerText == "1")
                        acsObj.isGroup = true;
                    else
                        acsObj.isGroup = false;

                    acsObj.isFIle = false;

                    returnObjs[index] = acsObj;
                    index++;
                }
            }

            if (Option == ACS_LIST_OPTION.ALL || Option == ACS_LIST_OPTION.FILE_ONLY)
            {
                foreach (XmlNode file in fileNodes)
                {
                    ACSObject acsObj = new ACSObject();

                    acsObj.objID = file["id"].InnerText;
                    acsObj.FileSize = Convert.ToUInt64(file["size"].InnerText);
                    acsObj.Name = file["display"].InnerText;
                    if (file["ispublic"].InnerText == "1")
                        acsObj.isSharedToOthers = true;
                    else
                        acsObj.isSharedToOthers = false;

                    if (file["isbackup"].InnerText == "1")
                        acsObj.isBackupType = true;
                    else
                        acsObj.isBackupType = false;

                    acsObj.CreatedTime = file["createdtime"].InnerText;

                    if (file["isgroupaware"].InnerText == "1")
                        acsObj.isGroup = true;
                    else
                        acsObj.isGroup = false;

                    acsObj.isFIle = true;
                    returnObjs[index] = acsObj;
                    index++;
                }
            }

            return returnObjs;
        }
        public string Share(string Path, bool isFile)
        {
            string shareCode = null;

            return shareCode;
        }
        public bool StopShare(string Path, bool isFile)
        {
            bool result = true;

            return result;
        }

        /* search api*/
        public void SqlQuery(bool is_folder, string keyword)
        {
            awsObj.search_sqlquery(is_folder,keyword);
        }

        /*
         * Private Function 
         */
        private void GetMyRootFoldersID()
        {
            this.mySyncFolderID = this.awsObj.browseFolder(SysFolder.MySync);
            this.myCollectionFolderID = this.awsObj.browseFolder(SysFolder.MyCollections);
            this.myBackFolderID = this.awsObj.browseFolder(SysFolder.MyBackup);
        }
        private string [] getPathList(string fullPath)
        {

            fullPath = fullPath.Replace('\\','/');
            string[] paths = fullPath.Split('/');
            return paths;

        }
        private string getLastFolderID(string[] paths)
        {
            Int64 startFolderID = 0;
            //取得起始的FolderID
            string lastFolderID = null;

            switch (paths[1].ToLower())
            {
                case "mysync":
                    lastFolderID = awsObj.getMySyncFolder();
                    break;
                case "mybackup":
                    lastFolderID = "-3";
                    break;
                case "mycollection":
                    lastFolderID = "0";
                    break;
                default:
                    return null;
                    break;
            }

            string result = null;

            if (paths.Length > 2 )
            {
                for (int i=2; i<paths.Length;i++)
                {
                    result = awsObj.browseFolder(lastFolderID);
                    lastFolderID =  getIDByName(paths[i].ToLower(), result);
                }
            }
            else
            {
                return lastFolderID.ToString();
            }

            return lastFolderID;
        }

        private string getIDByName(string name, string xmlResult)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlResult);

                XmlNodeList statusNodes = doc.SelectNodes("/browse/status");
                XmlNodeList folderNodes = doc.SelectNodes("/browse/folder");

                if (statusNodes[0].InnerText != "0")
                {
                    return null;
                }
                else
                {

                    foreach (XmlNode folder in folderNodes)
                    {
                        string displayName = folder["display"].InnerText.ToLower();
                        if (displayName == name)
                        {
                            return folder["id"].InnerText;
                        }
                    }
                }

                return null;

            }
            catch (Exception ex)
            {
                return null;
            }

           return null;

        }
    }
    public class ACSDataManager
    {
        awsClient awsObj;
        Int32 lastError;
        public ACSDataManager(string SID, string ProgKey, string Username, string Password)
        {
            awsObj = null;
            lastError = 0;
            if ((SID.Length == 0) || (ProgKey.Length == 0))
            {
                lastError = (Int32)ACS_ErrorCode.SID_PROGKEY_IS_NULL;
                return;
            }

            if ((Username.Length == 0) || (Password.Length == 0))
            {
                lastError = (Int32)ACS_ErrorCode.USERNAME_PASSWORD_IS_NULL;
                return;
            }

            try
            {
                this.awsObj = new awsClient(SID, ProgKey, Username, Password);
            }
            catch (Exception e)
            {
                if (awsObj.lastErrorMessage.Length > 0)
                {
                    if (awsObj.lastErrorMessage.IndexOf("acquireToken():", 0) >= 0)
                    {
                        this.lastError = (Int32)ACS_ErrorCode.ACQUIRE_TOKEN_FAIL;
                    }
                    else
                        if (awsObj.lastErrorMessage.IndexOf("postData():", 0) >= 0)
                        {
                            this.lastError = (Int32)ACS_ErrorCode.POST_DATA_TO_SERVER_FAIL;
                        }
                        else
                        {
                            this.lastError = (Int32)ACS_ErrorCode.UNEXPECTED_ERROR;
                        }
                }
                else
                {
                    this.lastError = (Int32)ACS_ErrorCode.UNEXPECTED_ERROR;
                }

                awsObj = null;
            }



            return;
        }
        public bool Write(string UrlOfFunction, string Data)
        {
            bool result = true;

            return result;
        }
        public String Query(string Function_Url, string QueryString)
        {
            string queryResult = null;

            return queryResult;
        }
    }
}
