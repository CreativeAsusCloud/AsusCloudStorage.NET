using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Web;
using System.Collections;

namespace aws
{
    public enum SysFolder
    {
        MySync = -5,
        MyBackup = -3,
        MyCollections = 0
    }

    public enum system 
    {
        folder = 0,
        file = 1,
        unknown = 2
    }
    public class SimpleBase64
    {
        //------------------------
        //編碼
        //------------------------
        public static string Encode(string WEStr)
        {
            string functionReturnValue = null;

            byte[] weba1;
            try
            {
                weba1 = Encoding.UTF8.GetBytes(WEStr);
                functionReturnValue = Convert.ToBase64String(weba1, 0, weba1.Length);
                if ((weba1 != null) & weba1.Length > 0)
                {
                    Array.Clear(weba1, 0, weba1.Length - 1);
                }
                weba1 = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                weba1 = null;
                functionReturnValue = "";
            }
            return functionReturnValue;
        }

        //------------------------
        //解碼
        //------------------------
        public static string Decode(string WEStr)
        {
            string functionReturnValue = null;

            byte[] weba1;
            try
            {
                weba1 = Convert.FromBase64String(WEStr);

                functionReturnValue = Encoding.UTF8.GetString(weba1, 0, weba1.Length);
                if ((weba1 != null) & weba1.Length > 0)
                {
                    Array.Clear(weba1, 0, weba1.Length - 1);
                }
                weba1 = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                weba1 = null;
                functionReturnValue = "";
            }
            return functionReturnValue;
        }
    }
    class Crypto
    {
        //------------------------
        //加密(SHA1)
        //------------------------
        public static string doHMacSH1(string input, string key)
        {
            string retVal = "";
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            using (HMACSHA1 hmac = new HMACSHA1(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
                string hashValue = Convert.ToBase64String(hashBytes);
                retVal = UrlEncodeUpperCase(hashValue);
            }
            return retVal;
        }

        //------------------------
        //加密(SHA512)
        //------------------------
        public static string doHMacSH512(FileStream input)
        {
            string retVal = "";
            
            using (HMACSHA512 hamc = new HMACSHA512())
            {
                byte[] hashBytes = hamc.ComputeHash(input);
                retVal = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
            return retVal;
        }

        //------------------------
        //加密(SHA512) for Memory
        //------------------------
        public static string doHMacSH512(MemoryStream input)
        {
            string retVal = "";

            using (HMACSHA512 hamc = new HMACSHA512())
            {
                byte[] hashBytes = hamc.ComputeHash(input);
                retVal = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
            return retVal;
        }

        //------------------------
        //URL編碼
        //------------------------
        public static string UrlEncodeUpperCase(string value)
        {
            value = HttpUtility.UrlEncode(value);
            return Regex.Replace(value, "(%[0-9a-f][0-9a-f])",
                                 delegate(Match match)
                                 {
                                     string v = match.ToString();
                                     return v.ToUpper();
                                 }
                                );
        }
    }
    class Times
    {
        //------------------------
        //計算從1970/1/1至今的毫秒數
        //------------------------
        public static double GetTimestamp(DateTime now)
        {
            TimeSpan span = (now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            return Convert.ToInt64(span.TotalSeconds);
        }

        //------------------------
        //將 TimeStamp 轉回 DateTime 
        //------------------------
        public static DateTime ConvertTimestamp2Datetime(double timestamp)
        {
            try
            {
                DateTime converted = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                DateTime newDateTime = converted.AddSeconds(timestamp);
                return newDateTime.ToLocalTime();
            }
            catch (Exception e)
            { }

            return DateTime.Now;
           
        }
    }
    public class awsClient
    {
        struct awsConfig
        {
            public string sServicePortalURL;
            public string sServiceGatewayURL;
            public string sHashedPassword;
            public string sUsername;
            public string sTokenString;
            public string sInfoRelayURL;
            public string sWebRelayURL;
            public string sJobRelayURL;
            public string chameleonURL;
            public string tsdbaseURL;
            public string searchURL;
            public string sPackageDisplay;
            public int BUFFER_SIZE;
            public string sSID;
            public string sProgKey;
            public string sLastError;
            public string sEtagString;
            public string sUsedSpace;
            public string sPackageSize;
            public string sExpireDate;
            public Hashtable sFileExtValue; //(file name, transaction id)
            public string sLatestCheckSum;
        }


        private awsConfig config;
        private Int32 dbErrorCode;
        private string ExceptionMsg;
        private string responseData;

        public string dataSet
        {
            get { return responseData; }
        }

        public string lastErrorMessage
        {
            get { return config.sLastError; }
        }

        public string ExceptionMessage 
        {
            get { return ExceptionMsg; }
        }

        public int lastError
        {
            get { return dbErrorCode; }
        }

        //------------------------
        //初始值設定
        //------------------------
        public awsClient(string SID, string ProgKey, string username, string password)
        {
            config.sServicePortalURL = "https://sp.yostore.net";
            config.BUFFER_SIZE = 1024 * 1024;
            config.sUsername = username;
            config.sSID = SID;
            config.sProgKey = ProgKey;
            hashedPassword(password);

            if (!login())
            {
                throw new InvalidOperationException(this.dbErrorCode + ": Login Fail");
            }
        }

        public string getToken()
        {
            return this.config.sTokenString;
        }

        public string getDBUrl()
        {
            return this.config.chameleonURL;
        }

        public string getUser()
        {
            return this.config.sUsername;
        }

        public string getUrl()
        {
            return  " " + this.config.sServiceGatewayURL + " " + this.config.sInfoRelayURL + " " + this.config.sWebRelayURL;
        }

        //------------------------
        //檢查初始值
        //------------------------
        private bool checkObj()
        {
            if ((config.sServiceGatewayURL.Length == 0) || (config.sTokenString.Length == 0))
                return false;

            return true;
        }

        //------------------------
        //登入
        //------------------------
        private bool login()
        {
            return acquireToken();

        }

        //------------------------
        //密碼編碼
        //------------------------
        private void hashedPassword(string password)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            config.sHashedPassword = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(password.ToLower()))).Replace("-", "");
            md5 = null;
        }

        //------------------------
        //與伺服器取Token
        //------------------------
        private bool acquireToken()
        {
            string mXML;
            string responseXML;

            XmlDocument ydom = new XmlDocument();
            XmlNodeList xNodes;
            XmlNodeList yNodes;

            config.sLastError = "";

            try
            {

                if (config.sServiceGatewayURL == null )
                {
                    requestServiceGateway();
                    //sgURLTB.Text = sServiceGatewayURL;
                    if (config.sServiceGatewayURL == null)
                    {
                        //this.config.sLastError = "Cannot Get URL of Service Gateway";
                        return false;
                    }
                }

                mXML = "<aaa><userid>" + config.sUsername + "</userid><password>" + config.sHashedPassword + "</password><time>2008/1/1</time></aaa>";

                responseXML = postData(config.sServiceGatewayURL + "/member/acquiretoken/", mXML);

                ydom.LoadXml(responseXML);

                xNodes = ydom.SelectNodes("aaa");

                if (xNodes.Item(0).SelectNodes("status").Item(0).InnerText != "0")
                {
                    this.dbErrorCode = Convert.ToInt32( xNodes.Item(0).SelectNodes("status").Item(0).InnerText);
                    return false;
                }

                config.sTokenString = xNodes.Item(0).SelectNodes("token").Item(0).InnerText;

                config.sInfoRelayURL = "https://" + xNodes.Item(0).SelectNodes("inforelay").Item(0).InnerText;

                config.sWebRelayURL = "https://" + xNodes.Item(0).SelectNodes("webrelay").Item(0).InnerText;

                config.sJobRelayURL = "https://" + xNodes.Item(0).SelectNodes("jobrelay").Item(0).InnerText;

                config.chameleonURL = "https://" + xNodes.Item(0).SelectNodes("chameleondb").Item(0).InnerText;

                config.tsdbaseURL = "https://" + xNodes.Item(0).SelectNodes("tsdbase").Item(0).InnerText;

                config.searchURL = "https://" + xNodes.Item(0).SelectNodes("searchserver").Item(0).InnerText;

                yNodes = xNodes.Item(0).SelectNodes("package");
                config.sPackageDisplay = yNodes.Item(0).SelectNodes("display").Item(0).InnerText;

                ydom = null;
            }
            catch (Exception ex)
            {
                config.sTokenString = "";
                config.sLastError = "acquireToken():" + ex.Message;
                return false;
            }

            return true;
        }

        //------------------------
        //伺服器連線值設定
        //------------------------
        private bool requestServiceGateway()
        {
            string mXML = "";
            string responseXML = "";
            try
            {
                XmlDocument ydom = new XmlDocument();
                XmlNodeList xNodes;

                mXML = "<requestservicegateway><userid>" + config.sUsername + "</userid><password>" + config.sHashedPassword + "</password><language></language><service>1</service>></requestservicegateway>";
                responseXML = postData(config.sServicePortalURL + "/member/requestservicegateway/", mXML);

                if (responseXML == "")
                {
                    config.sServiceGatewayURL = "";
                    // no data return
                    config.sLastError = "ServiceGatewayURL is Null";
                    return false;

                }
                ydom.LoadXml(responseXML);
                try
                {
                    xNodes = ydom.SelectNodes("requestservicegateway");
                    config.sServiceGatewayURL = "https://" + xNodes.Item(0).SelectNodes("servicegateway").Item(0).InnerText;
                }
                catch (Exception ex)
                {
                    config.sLastError = ex.Message;
                    return false;
                }
                if (config.sServiceGatewayURL == "")
                {
                    config.sLastError = "ServiceGatewayURL is Null";
                    return false;
                }
                ydom = null;

            }
            catch (Exception ex)
            {
                config.sServiceGatewayURL = "";
                config.sLastError = ex.Message;
                return false;
            }

            return true;
        }

        private string postData(string rUrl, FileStream fs, long fileSize)
        {
            string backStr;
            backStr = "";

            try
            {
                WebRequest httpRequest;
                httpRequest = WebRequest.Create(rUrl);
                httpRequest.Method = "POST";

                httpRequest.Headers.Add("Authorization", makeAuthorizeString());
                httpRequest.Headers.Add("Cookie", "Cookie=OMNISTORE_VER=1_0;sid=" + config.sSID + ";");
                UTF8Encoding encoding = new UTF8Encoding();
                Stream newStream;
                byte[] data = new byte[config.BUFFER_SIZE];
                int readLen = 0;
                httpRequest.ContentLength = fileSize;
                newStream = httpRequest.GetRequestStream();

                fs.Seek(0, SeekOrigin.Begin);
                while ((readLen = fs.Read(data, 0, config.BUFFER_SIZE)) > 0)
                {
                    newStream.Write(data, 0, readLen);
                }
                newStream.Close();

                WebResponse httpResponse = httpRequest.GetResponse();
                StreamReader readStream;
                readStream = new StreamReader(httpResponse.GetResponseStream(), encoding);
                char[] read = new char[257];
                // Reads 256 characters at a time.
                int count = readStream.Read(read, 0, 256);

                while (count > 0)
                {
                    // Dumps the 256 characters to a string and displays the string to the console.
                    string str = new string(read, 0, count);
                    backStr = backStr + str;
                    count = readStream.Read(read, 0, 256);
                    str = null;
                }

                httpResponse.Close();
                encoding = null;
                readStream = null;
                read = null;
            }
            catch (Exception e)
            {
                string msg = "postData():" + e.Message + ", Detail-URL:" + rUrl;
                ArgumentException ae = new ArgumentException(msg);
                throw ae;
            }

            return backStr;
        }

        //------------------------
        //使用Post與伺服器溝通
        //------------------------
        private string postData(string rUrl, string dataStr)
        {
            string backStr;
            backStr = "";

            WebRequest httpRequest;

            try
            {
                httpRequest = WebRequest.Create(rUrl);
                httpRequest.Method = "POST";

                httpRequest.Headers.Add("Authorization", makeAuthorizeString());

                httpRequest.Headers.Add("Cookie", "Cookie=OMNISTORE_VER=1_0;sid=" + config.sSID + ";");

                UTF8Encoding encoding = new UTF8Encoding();
                if (dataStr != null && dataStr.Trim().Length > 0)
                {
                    Stream newStream;
                    byte[] data;
                    data = encoding.GetBytes(dataStr);
                    httpRequest.ContentLength = data.Length;

                    newStream = httpRequest.GetRequestStream();
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                }

                WebResponse httpResponse = httpRequest.GetResponse();

                StreamReader readStream;
                readStream = new StreamReader(httpResponse.GetResponseStream(), encoding);
                char[] read = new char[257];
                // Reads 256 characters at a time.
                int count = readStream.Read(read, 0, 256);

                while (count > 0)
                {
                    // Dumps the 256 characters to a string and displays the string to the console.
                    string str = new string(read, 0, count);
                    backStr = backStr + str;
                    count = readStream.Read(read, 0, 256);
                    str = null;
                }
                httpResponse.Close();

                encoding = null;
                readStream = null;
                read = null;

                return backStr;
            }
            catch (Exception e)
            {

                string msg = "postData():" + e.Message + ", Detail-URL:" + rUrl + ", data=" + dataStr;
                ArgumentException ae = new ArgumentException(msg);
                throw ae;
            }
        }

        private string postData(string rUrl, MemoryStream ms)
        {
            string backStr;
            backStr = "";
            try
            {
                WebRequest httpRequest;
                httpRequest = WebRequest.Create(rUrl);
                httpRequest.Method = "POST";

                httpRequest.Headers.Add("Authorization", makeAuthorizeString());
                httpRequest.Headers.Add("Cookie", "Cookie=OMNISTORE_VER=1_0;sid=" + config.sSID + ";");
                UTF8Encoding encoding = new UTF8Encoding();
                Stream newStream;
                byte[] data = new byte[config.BUFFER_SIZE];
                int readLen = 0;
                httpRequest.ContentLength = ms.Length;
                newStream = httpRequest.GetRequestStream();

                ms.Seek(0, SeekOrigin.Begin);
                while ((readLen = ms.Read(data, 0, config.BUFFER_SIZE)) > 0)
                {
                    newStream.Write(data, 0, readLen);
                }
                newStream.Close();

                WebResponse httpResponse = httpRequest.GetResponse();
                StreamReader readStream;
                readStream = new StreamReader(httpResponse.GetResponseStream(), encoding);
                char[] read = new char[257];
                // Reads 256 characters at a time.
                int count = readStream.Read(read, 0, 256);

                while (count > 0)
                {
                    // Dumps the 256 characters to a string and displays the string to the console.
                    string str = new string(read, 0, count);
                    backStr = backStr + str;
                    count = readStream.Read(read, 0, 256);
                    str = null;
                }

                httpResponse.Close();
                encoding = null;
                readStream = null;
                read = null;
            }
            catch (Exception e)
            {
                string msg = "postData():" + e.Message + ", Detail-URL:" + rUrl ;
                ArgumentException ae = new ArgumentException(msg);
                throw ae;
            }

            return backStr;
        }

      

        //------------------------
        //使用Post與Chameleon伺服器溝通
        //------------------------
        private string postData(string rUrl, string dbAPIName, string clientSet_name, string dataStr)
        {
            string backStr;
            backStr = "";

            WebRequest httpRequest;

            try
            {

                httpRequest = WebRequest.Create(rUrl);
                httpRequest.Method = "POST";

                httpRequest.Headers.Add("Authorization", makeAuthorizeString());

                httpRequest.Headers.Add("X-Omni-Target", dbAPIName);
                //httpRequest.Headers.Add("Content-Type", "text/x-omni-json-1.0");
                httpRequest.ContentType = "text/x-omni-json-1.0";
                httpRequest.Headers.Add("X-Omni-ClientSet", clientSet_name);
                httpRequest.Headers.Add("X-Omni-Token", this.config.sTokenString);
                httpRequest.Headers.Add("X-Omni-Sid", this.config.sSID);


                UTF8Encoding encoding = new UTF8Encoding();
                if (dataStr != null && dataStr.Trim().Length > 0)
                {
                    Stream newStream;
                    byte[] data;
                    data = encoding.GetBytes(dataStr);
                    httpRequest.ContentLength = data.Length;

                    newStream = httpRequest.GetRequestStream();
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                }

                WebResponse httpResponse = httpRequest.GetResponse();

                StreamReader readStream;
                readStream = new StreamReader(httpResponse.GetResponseStream(), encoding);
                char[] read = new char[257];
                // Reads 256 characters at a time.
                int count = readStream.Read(read, 0, 256);

                while (count > 0)
                {
                    // Dumps the 256 characters to a string and displays the string to the console.
                    string str = new string(read, 0, count);
                    backStr = backStr + str;
                    count = readStream.Read(read, 0, 256);
                    str = null;
                }
                httpResponse.Close();

                string[] status = httpResponse.Headers.GetValues("X-Omni-Status");
                


                encoding = null;
                readStream = null;
                read = null;

                this.responseData = null;
                this.responseData = backStr;

                return status[0];
            }
            catch (Exception e)
            {
                string msg = "postData():" + e.Message + ", Detail-URL:" + rUrl + ", data=" + dataStr;
                ArgumentException ae = new ArgumentException(msg);
                throw ae;
            }
        }

        //------------------------
        //使用Post與Tsdbase伺服器溝通by JSON
        //------------------------
        public string postDatabyJSON(string service_name, string action_name, string dataStr)
        {
            string backStr;
            backStr = "";
            string rUrl = this.config.tsdbaseURL + "/tsdbase/entry";

            WebRequest httpRequest;

            try
            {

                httpRequest = WebRequest.Create(rUrl);
                httpRequest.Method = "POST";

                httpRequest.Headers.Add("Authorization", makeAuthorizeString());

                httpRequest.Headers.Add("X-Omni-Service", service_name);
                //httpRequest.Headers.Add("Content-Type", "text/x-omni-json-1.0");
                httpRequest.ContentType = "text/x-omni-json-1.0";
                httpRequest.Headers.Add("X-Omni-Action", action_name);
                httpRequest.Headers.Add("X-Omni-Token", this.config.sTokenString);
                httpRequest.Headers.Add("X-Omni-Sid", this.config.sSID);


                UTF8Encoding encoding = new UTF8Encoding();
                if (dataStr != null && dataStr.Trim().Length > 0)
                {
                    Stream newStream;
                    byte[] data;
                    data = encoding.GetBytes(dataStr);
                    httpRequest.ContentLength = data.Length;

                    newStream = httpRequest.GetRequestStream();
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                }

                WebResponse httpResponse = httpRequest.GetResponse();

                StreamReader readStream;
                readStream = new StreamReader(httpResponse.GetResponseStream(), encoding);
                char[] read = new char[257];
                // Reads 256 characters at a time.
                int count = readStream.Read(read, 0, 256);

                while (count > 0)
                {
                    // Dumps the 256 characters to a string and displays the string to the console.
                    string str = new string(read, 0, count);
                    backStr = backStr + str;
                    count = readStream.Read(read, 0, 256);
                    str = null;
                }
                httpResponse.Close();

                string[] status = httpResponse.Headers.GetValues("X-Omni-Status");

                encoding = null;
                readStream = null;
                read = null;

                this.responseData = null;
                this.responseData = backStr;

                if (action_name == "PutEntries" || status[0]!="0")
                {
                    return status[0];
                }
                return this.responseData;


            }
            catch (Exception e)
            {
                string msg = "postData():" + e.Message + ", Detail-URL:" + rUrl + ", data=" + dataStr;
                ArgumentException ae = new ArgumentException(msg);
                throw ae;
            }
        }


        //------------------------
        //驗證參數值設定
        //------------------------
        private string makeAuthorizeString()
        {
            //Declare OAuth Header Parameters
            string signatureMethod = "HMAC-SHA1";
            string timestamp = Convert.ToString(Times.GetTimestamp(DateTime.Now));
            string nonce = timestamp;

            StringBuilder oAuthParams = new StringBuilder();
            oAuthParams.Append("nonce=").Append(nonce)
                       .Append("&signature_method=").Append(signatureMethod)
                       .Append("&timestamp=").Append(timestamp);
            string oAuthParasURLEn = Crypto.UrlEncodeUpperCase(oAuthParams.ToString());
            string signature = Crypto.doHMacSH1(oAuthParasURLEn, config.sProgKey);

            StringBuilder oAuth = new StringBuilder();
            oAuth.Append("signature_method=\"").Append(signatureMethod).Append("\",")
                 .Append("timestamp=\"").Append(timestamp).Append("\",")
                 .Append("nonce=\"").Append(nonce).Append("\",")
                 .Append("signature=\"").Append(signature).Append("\"");

            return oAuth.ToString();
        }

        //------------------------
        //確認目錄名稱是否存在
        //------------------------
        public string isExist(string Name, string parentFolderID)
        {
            string id;
            try
            {
                id = propfind(Name, parentFolderID, system.unknown);
            }
            catch (Exception ex)
            {
                return null;
            }

            if (id.Length == 0 || id == null)
                return null;
            
            return id;
        }

        //------------------------
        //取得目錄ID
        //------------------------
        public string getFolderID(string FolderName, string parentFolderID)
        {
            string id="";

            try 
            {
                id = propfind( FolderName,  parentFolderID, system.folder);
            }

            catch (Exception ex)
            {
                return null;
            }
            return  id;
        }

        //------------------------
        //取得檔案ID
        //------------------------
        public string getFileID(string fileName, string parentFolderID)
        {
            string id="";

            try
            {
                id = propfind(fileName.Substring(fileName.LastIndexOf("\\") +1) , parentFolderID, system.file);
            }

            catch (Exception ex)
            {
                return null;
            }
            return id;
        }

        //------------------------
        //查詢檔案或目錄是否存在
        //------------------------
        private string propfind(string FolderName, string parentFolderID, system type )
        {
                string mXML, responseXML;
                string currentFolderID;
               

                if (!checkObj())
                {
                    throw new NullReferenceException();
                }

                if (config.sEtagString == "")
                    config.sEtagString = "system.new";

                XmlDocument ydom = new XmlDocument();
                XmlNodeList xNodes;

                string propType;
                
                switch (type){
                    case system.file:
                        propType = "system.file";
                        break;
                    case system.folder:
                        propType = "system.folder";
                        break;
                    case system.unknown:
                        propType = "system.unknown";
                        break;
                    default:
                        propType = "system.unknown";
                        break;
                }


                mXML = "<propfind><token>" + config.sTokenString + "</token><scrip>" + config.sEtagString + "</scrip><userid>" + config.sUsername + "</userid><parent>" + parentFolderID + "</parent><find>" + SimpleBase64.Encode(FolderName) + "</find><type>" + propType + "</type></propfind>";

                try
                {
                    responseXML = postData(config.sInfoRelayURL + "/find/propfind/", mXML);
                if (responseXML.IndexOf("<status>2</status>") != -1)
                {
                    config.sTokenString = "";
                    config.sEtagString = "";
                    ydom = null;
                    // Throw exception
                    return "";
                }
                ydom.LoadXml(responseXML);
                xNodes = ydom.SelectNodes("propfind");
                string returnType;

                config.sEtagString = xNodes.Item(0).SelectNodes("scrip").Item(0).InnerText;
                returnType = xNodes.Item(0).SelectNodes("type").Item(0).InnerText;
                currentFolderID = xNodes.Item(0).SelectNodes("id").Item(0).InnerText;

                return currentFolderID;
            }
            catch { return ""; }
        }

        //------------------------
        //建立資料夾
        //------------------------
        public string createFolder(string folderName, string parentFolderID, bool needEncrypt)
        {
            string xmlCreateFolder;
            string encrypt;

            if (needEncrypt)
                encrypt = "1";
            else
                encrypt = "0";

            xmlCreateFolder = "<create><token>"+ config.sTokenString+"</token><userid>"+config.sUsername +"</userid>" +
	                          "<parent>"+ parentFolderID + "</parent><isencrypted>"+ encrypt+"</isencrypted>" +
                              "<display>" + SimpleBase64.Encode(folderName) + "</display> " +
                              "<attribute>" + createAttributeTag(DateTime.Now, DateTime.Now, DateTime.Now) + "</attribute></create>";

            string responseXML;
            try
            {
                responseXML = postData(config.sInfoRelayURL + "/folder/create/", xmlCreateFolder);
            }
            catch (Exception ex)
            {
                return "-1" + ex.Message.ToString();//不可預期的錯誤
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("create");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = null;
                    break;
                case "2":
                    config.sTokenString = "";
                    config.sEtagString = "";
                    strResult = "2:Authentication Fail";
                    break;
                case "3":
                    strResult = "3:Payload is not validate";
                    break;
                case "200":
                    strResult = "200:唯讀權限,無法進行操作";
                    break;
                case "211":
                    strResult = "211:名稱為空白";
                    break;
                case "213":
                    strResult = "213:名稱長度超過限制";
                    break;
                case "214":
                    strResult = "214:名稱重覆";
                    break;
                case "218":
                    strResult = "218:要處理的目錄不存在或已刪除";
                    break;
            }

            return strResult;
        }

        //------------------------
        //移除資料夾
        //------------------------
        public string removeFolder(string[] folderID, bool isChildOnly=false)
        {
            string xmlRemoveFolder;
            int childOnly;
            string folderIDList;

            if (isChildOnly)
                childOnly = 1; //只刪除指定目錄以下的檔案及子目錄,不刪除指定目錄本身
            else
                childOnly = 0; //刪除含指定目錄,及其以下所有檔案及子目錄

            if (folderID.Length > 1 )
            {
                folderIDList = String.Join(",",folderID);
            }
            else
                folderIDList = folderID [0].ToString();

            xmlRemoveFolder = "<remove><token>" + config.sTokenString + "</token><userid>" + config.sUsername + "</userid>" +
                              "<id>" + folderIDList + "</id><ischildonly>" + childOnly + "</ischildonly></remove>";

            string responseXML;
            try
            {
                responseXML = postData(config.sInfoRelayURL + "/folder/remove/", xmlRemoveFolder);
            }
            catch (Exception ex)
            {
                return "-1:" + ex.Message.ToString();
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("remove");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = null;
                    break;
                case "2":
                    config.sTokenString = "";
                    config.sEtagString = "";
                    strResult = "2:Authentication Fail";
                    break;
                case "200":
                    strResult = "200:唯讀權限,無法進行操作";
                    break;
                case "218":
                    strResult = "218:要處理的目錄不存在或已刪除";//一次刪除多目錄時不會有這個錯誤
                    break;
                case "999":
                    strResult = "999:General Error";
                    break;
            }

            return strResult;
        }

        //------------------------
        //移除檔案
        //------------------------
        public string removeFile(string[] folderID)
        {
            string xmlRemoveFile;
            string fileIDList;

            if (folderID.Length > 1)
            {
                fileIDList = String.Join(",", folderID);
            }
            else
                fileIDList = folderID[0].ToString();

            xmlRemoveFile = "<remove><token>" + config.sTokenString + "</token><userid>" + config.sUsername + "</userid>" +
                              "<id>" + fileIDList + "</id></remove>";

            string responseXML;
            try
            {
                responseXML = postData(config.sInfoRelayURL + "/file/remove/", xmlRemoveFile);
            }
            catch (Exception ex)
            {
                return "-1:" + ex.Message.ToString();
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("remove");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = null;
                    break;
                case "2":
                    config.sTokenString = "";
                    config.sEtagString = "";
                    strResult = "2:Authentication Fail";
                    break;
                case "200":
                    strResult = "200:唯讀權限,無法進行操作";
                    break;
                case "218":
                    strResult = "218:要處理的檔案不存在或已刪除";
                    break;
                case "999":
                    strResult = "999:General Error";
                    break;
            }

            return strResult;
        }

        //------------------------
        //重新命名目錄
        //------------------------
        public string renameFolder(string folderID, string newFolderName, bool needEncrypt=false){
            string xmlRenameFolder;
            string isEncrypted;

            if (needEncrypt)
                isEncrypted = "1";
            else
                isEncrypted = "0";

            //automatically get encrypted flag of old folder
            xmlRenameFolder = "<rename><token>" + config.sTokenString +"</token>" +
                              "<userid>" + config.sUsername + "</userid>" +
                              "<id>" + folderID + "</id><isencrypted>" + isEncrypted + "</isencrypted>" +
                              "<display>"+ SimpleBase64.Encode(newFolderName) + "</display></rename>";

            string responseXML;
            try
            {
                responseXML = postData(config.sInfoRelayURL + "/folder/rename/", xmlRenameFolder);
            }
            catch (Exception ex)
            {
                return "-1:" + ex.Message.ToString();
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("rename");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = null;
                    break;
                case "2":
                    config.sTokenString = "";
                    config.sEtagString = "";
                    strResult = "2:Authentication Fail";
                    break;
                case "12":
                    strResult = "12:Inbound XML 解不開(XML不完整)";
                    break;
                case "211":
                    strResult = "211:上傳 Folder Name 是空值";
                    break;
                case "213":
                    strResult = "213:上傳 Folder Name 長度超過 255";
                    break;
                case "214":
                    strResult = "214:上傳 Folder Name 已存在";
                    break;
                case "218":
                    strResult = "218:Folder 不存在";
                    break;
                case "255":
                    strResult = "255:上傳參數錯誤(例如:<id>參數未傳)";
                    break;
                case "235":
                    strResult = "235:上傳參數的操作要求錯誤";
                    break;
                case "999":
                    strResult = "999:General Error";
                    break;
            }

            return strResult;
        }

        //------------------------
        //重新命名檔案
        //------------------------
        public string renameFile(string fileID, string newFileName, bool needEncrypt = false)
        {
            string xmlRenameFile;
            string isEncrypted;

            if (needEncrypt)
                isEncrypted = "1";
            else
                isEncrypted = "0";

            //automatically get encrypted flag of old folder
            xmlRenameFile = "<rename><token>" + config.sTokenString + "</token>" +
                              "<userid>" + config.sUsername + "</userid>" +
                              "<id>" + fileID + "</id><isencrypted>" + isEncrypted + "</isencrypted>" +
                              "<display>" + SimpleBase64.Encode(newFileName) + "</display></rename>";

            string responseXML;
            try
            {
                responseXML = postData(config.sInfoRelayURL + "/file/rename/", xmlRenameFile);
            }
            catch (Exception ex)
            {
                return "-1:" + ex.Message.ToString();
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("rename");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = null;
                    break;
                case "2":
                    config.sTokenString = "";
                    config.sEtagString = "";
                    strResult = "2:Authentication Fail";
                    break;
                case "211":
                    strResult = "211:上傳 File Name 是空值";
                    break;
                case "213":
                    strResult = "213:上傳 File Name 長度超過 255";
                    break;
                case "214":
                    strResult = "214:上傳 File Name 已存在";
                    break;
                case "218":
                    strResult = "218:Folder 不存在";
                    break;
                case "219":
                    strResult = "219:File 不存在";
                    break;
                case "255":
                    strResult = "255:上傳參數錯誤(例如:<id>參數未傳)";
                    break;
                case "235":
                    strResult = "235:上傳參數的操作要求錯誤";
                    break;
                case "999":
                    strResult = "999:General Error";
                    break;
            }

            return strResult;
        }

        //------------------------
        //移動目錄
        //------------------------
        public string moveFolder(string folderID, string destinationParentFolderID, string newFolderName)
        {
            string xmlMoveFolder;


            //automatically get encrypted flag of old folder
            xmlMoveFolder = "<move><token>" + config.sTokenString + "</token>" +
                              "<userid>" + config.sUsername + "</userid>" +
                              "<id>" + folderID + "</id><display>" + SimpleBase64.Encode(newFolderName) + "</display>" +
                              "<parent>" + destinationParentFolderID + "</parent></move>";

            string responseXML;
            try
            {
                responseXML = postData(config.sInfoRelayURL + "/folder/move/", xmlMoveFolder);
            }
            catch (Exception ex)
            {
                return "-1" + ex.Message.ToString();
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("move");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = null;
                    break;
                case "2":
                    config.sTokenString = "";
                    config.sEtagString = "";
                    strResult = "2:Authentication Fail";
                    break;
                case "3":
                    strResult = "3:???";
                    break;
                case "213":
                    strResult = "213:上傳 Folder Name 長度超過 255";
                    break;
                case "214":
                    strResult = "214:指定移動的 Folder Name 在目的地資料夾中已存在";
                    break;
                case "215":
                    strResult = "215:移動的目的地與來源 Folder 的上一層為同一資料夾";
                    break;
                case "216":
                    strResult = "216:移動的目的地不存在";
                    break;
                case "225":
                    strResult = "225:上傳參數錯誤";
                    break;
                case "233":
                    strResult = "233:移動的目的地與來源 Folder Root 不相同";
                    break;
                case "235":
                    strResult = "235:上傳參數的操作要求錯誤";
                    break;
                case "999":
                    strResult = "999:General Error";
                    break;
            }

            return strResult;
        }

        //------------------------
        //移動檔案
        //------------------------
        public string moveFile(string fileID, string destinationParentFolderID)
        {
            string xmlMoveFile;

            //automatically get encrypted flag of old folder
            xmlMoveFile = "<move><token>" + config.sTokenString + "</token>" +
                              "<userid>" + config.sUsername + "</userid>" +
                              "<id>" + fileID + "</id>" +
                              "<parent>" + destinationParentFolderID + "</parent></move>";

            string responseXML;
            try
            {
                responseXML = postData(config.sInfoRelayURL + "/file/move/", xmlMoveFile);
            }
            catch (Exception ex)
            {
                return "-1:" + ex.Message.ToString();
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("move");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = null;
                    break;
                case "2":
                    config.sTokenString = "";
                    config.sEtagString = "";
                    strResult = "2:Authentication Fail";
                    break;
                case "3":
                    strResult = "3:???";
                    break;
                case "214":
                    strResult = "214:指定移動的 File Name 在目的地資料夾中已存在";
                    break;
                case "215":
                    strResult = "215:移動的目的地與來源 Folder 的上一層為同一資料夾";
                    break;
                case "216":
                    strResult = "216:移動的目的地不存在";
                    break;
                case "218":
                    strResult = "218:Folder 不存在";
                    break;
                case "219":
                    strResult = "219:File 不存在";
                    break;
                case "225":
                    strResult = "225:上傳參數錯誤";
                    break;
                case "233":
                    strResult = "233:移動的目的地與來源 Folder Root 不相同";
                    break;
                case "235":
                    strResult = "235:上傳參數的操作要求錯誤";
                    break;
                case "999":
                    strResult = "999:General Error";
                    break;
            }

            return strResult;
        }

        //------------------------
        //建立屬性標籤
        //------------------------
        private string createAttributeTag(DateTime ct, DateTime la, DateTime lw)
        {
            
            string fileCT = Convert.ToString(Times.GetTimestamp(ct ));
            string fileLA = Convert.ToString(Times.GetTimestamp(la));
            string fileLW = Convert.ToString(Times.GetTimestamp(lw));

            /* Composing file times to be attribute */
            StringBuilder attribute = new StringBuilder();
            attribute.Append("<creationtime>").Append(fileCT).Append("</creationtime>")
                     .Append("<lastaccesstime>").Append(fileLA).Append("</lastaccesstime>")
                     .Append("<lastwritetime>").Append(fileLW).Append("</lastwritetime>");

            //return ( Crypto.UrlEncodeUpperCase(attribute.ToString()) );
            return (attribute.ToString());

        }

        //------------------------
        //取得使用者帳戶資訊
        //------------------------
        public string getAccountInfo()
        {
            string mXML;
            string responseXML;

            if (!checkObj())
            { 
                throw new NullReferenceException();
             }

            try
            {
                if (config.sServiceGatewayURL == null) return null;
                mXML = "<getinfo><token>" + config.sTokenString + "</token><userid>" + config.sUsername + "</userid><time>2008/1/1</time></getinfo>";
                responseXML = postData(config.sServiceGatewayURL + "/member/getinfo/", mXML);

                if (responseXML.IndexOf("<status>999</status>") != -1)
                {
                    return null;
                }

                return (responseXML);
            }
            catch (Exception ex)
            {
                config.sTokenString = "";
                return null;
            }
        }

        //------------------------
        //取得最近更新之檔案清單
        //------------------------
        public string getLastestChangeFiles(string folderID)
        {
            string mXML;
            string responseXML;

            if (!checkObj())
            {
                throw new NullReferenceException();
            }

            try
            {
                if (config.sServiceGatewayURL == null) return null;
                mXML = "<getlatestchangefiles><token>" + config.sTokenString + "</token><userid>" + config.sUsername + "</userid><top>10</top><targetroot>" + folderID + "</targetroot><sortdirection>0</sortdirection></getlatestchangefiles>";
                responseXML = postData(config.sInfoRelayURL + "/file/getlatestchangefiles/", mXML);

                if (responseXML.IndexOf("<status>999</status>") != -1)
                {
                    return null;
                }
                
                XmlDocument responseDOM = new XmlDocument();
                responseDOM.LoadXml(responseXML);

                XmlNodeList fctimeNodes = responseDOM.SelectNodes("/getlatestchangefiles/entry/attribute/creationtime");
                foreach (XmlNode fctn in fctimeNodes)
                {
                    fctn.InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(fctn.InnerText)).ToString();
                }

                XmlNodeList fltimeNodes = responseDOM.SelectNodes("/getlatestchangefiles/entry/attribute/lastaccesstime");
                foreach (XmlNode fltn in fltimeNodes)
                {
                    fltn.InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(fltn.InnerText)).ToString();
                }

                XmlNodeList fwtimeNodes = responseDOM.SelectNodes("/getlatestchangefiles/entry/attribute/lastwritetime");
                foreach (XmlNode fwtn in fwtimeNodes)
                {
                    fwtn.InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(fwtn.InnerText)).ToString();
                }

                return (responseDOM.InnerXml);
            }
            catch (Exception ex)
            {
                config.sTokenString = "";
                return null;
            }
        }

        //------------------------
        //取得最近上傳之檔案清單
        //------------------------
        public string getLatestUploads(string folderID)
        {
            string mXML;
            string responseXML;

            if (!checkObj())
            {
                throw new NullReferenceException();
            }

            try
            {
                if (config.sServiceGatewayURL == null) return null;
                mXML = "<getlatestuploads><token>" + config.sTokenString + "</token><userid>" + config.sUsername + "</userid><top>10</top><targetroot>" + folderID + "</targetroot><sortdirection>0</sortdirection></getlatestuploads>";
                responseXML = postData(config.sInfoRelayURL + "/file/getlatestuploads/", mXML);

                if (responseXML.IndexOf("<status>999</status>") != -1)
                {
                    return null;
                }

                XmlDocument responseDOM = new XmlDocument();
                responseDOM.LoadXml(responseXML);

                XmlNodeList fctimeNodes = responseDOM.SelectNodes("/getlatestuploads/entry/attribute/creationtime");
                foreach (XmlNode fctn in fctimeNodes)
                {
                    fctn.InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(fctn.InnerText)).ToString();
                }

                XmlNodeList fltimeNodes = responseDOM.SelectNodes("/getlatestuploads/entry/attribute/lastaccesstime");
                foreach (XmlNode fltn in fltimeNodes)
                {
                    fltn.InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(fltn.InnerText)).ToString();
                }

                XmlNodeList fwtimeNodes = responseDOM.SelectNodes("/getlatestuploads/entry/attribute/lastwritetime");
                foreach (XmlNode fwtn in fwtimeNodes)
                {
                    fwtn.InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(fwtn.InnerText)).ToString();
                }

                return (responseDOM.InnerXml);
            }
            catch (Exception ex)
            {
                config.sTokenString = "";
                return null;
            }
        }

        //------------------------
        //設定根目錄參數值
        //------------------------
        public string browseFolder(SysFolder folderID)
        {
            string xmlRet;

            switch (folderID)
            {
                case SysFolder.MyBackup:
                    xmlRet=browseFolder("-3");
                    break;
                case SysFolder.MyCollections:
                    xmlRet=browseFolder("0");
                    break;
                case SysFolder.MySync:
                    xmlRet=browseFolder("-5");
                    break;
                default:
                    xmlRet = null;
                    break;

            }

             return (xmlRet);
        }

        //------------------------
        //取得根目錄以下資訊
        //------------------------
        public string browseFolder(string folderID, string extList=null)
        {
            if (folderID == null)
            {
                throw new ArgumentNullException() ;
            }
            if (extList == null)
                extList = "";

            string xmlBrowseCmd = "<browse><token>" + config.sTokenString + "</token><scrip>" + config.sEtagString + "</scrip><userid>" + config.sUsername + "</userid><folderid>" + folderID.ToString() + "</folderid><fileext>"+ extList +"</fileext><language></language></browse>";
            
            XmlDocument domBrowseCmd = new XmlDocument();

            domBrowseCmd.LoadXml(xmlBrowseCmd);
            return browse(domBrowseCmd.InnerXml);
        }

        //------------------------
        //與伺服器取得根目錄以下資訊
        //------------------------
        private string browse(string xmlBrowseCmd)
        {
            if (config.sTokenString == null)
            {
                throw new NullReferenceException();
            }


            string responseXML;

            responseXML = postData(config.sInfoRelayURL + "/folder/browse/", xmlBrowseCmd);
            if (responseXML.IndexOf("<status>0</status>") == -1)
            {
                config.sTokenString = "";
                config.sEtagString = "";
                return null;
            }
           
            XmlDocument responseDOM = new XmlDocument();

            responseDOM.LoadXml(responseXML);
            XmlNodeList folderNodes = responseDOM.SelectNodes("/browse/parentfolder/name");
            if (folderNodes[0].InnerText.Length > 0)
                folderNodes[0].InnerText = SimpleBase64.Decode(folderNodes[0].InnerText);


            folderNodes = responseDOM.SelectNodes("/browse/folder/display");
            foreach (XmlNode xn in folderNodes)
            {
                xn.InnerText = SimpleBase64.Decode(xn.InnerText);
            }

            XmlNodeList fileNodes = responseDOM.SelectNodes("/browse/file/display");
            foreach (XmlNode fn in fileNodes)
            {
                fn.InnerText = SimpleBase64.Decode(fn.InnerText);
            }

            XmlNodeList xctimeNodes = responseDOM.SelectNodes("/browse/folder/attribute/creationtime");
            foreach (XmlNode xctn in xctimeNodes)
            {
                try
                {
                    xctn.InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(xctn.InnerText)).ToString();
                }
                catch (Exception e)
                {
                    xctn.InnerText = null;
                }
            }

            XmlNodeList xltimeNodes = responseDOM.SelectNodes("/browse/folder/attribute/lastaccesstime");
            foreach (XmlNode xltn in xltimeNodes)
            {
                try
                {
                    xltn.InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(xltn.InnerText)).ToString();
                }
                catch (Exception e)
                {
                    xltn.InnerText = null;
                }
            }

            XmlNodeList xwtimeNodes = responseDOM.SelectNodes("/browse/folder/attribute/lastwritetime");
            foreach (XmlNode xwtn in xwtimeNodes)
            {
                xwtn.InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(xwtn.InnerText)).ToString();
            }

            XmlNodeList fctimeNodes = responseDOM.SelectNodes("/browse/file/attribute/creationtime");
            foreach (XmlNode fctn in fctimeNodes)
            {
                fctn.InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(fctn.InnerText)).ToString();
            }

            XmlNodeList fltimeNodes = responseDOM.SelectNodes("/browse/file/attribute/lastaccesstime");
            foreach (XmlNode fltn in fltimeNodes)
            {
                fltn.InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(fltn.InnerText)).ToString();
            }

            XmlNodeList fwtimeNodes = responseDOM.SelectNodes("/browse/file/attribute/lastwritetime");
            foreach (XmlNode fwtn in fwtimeNodes)
            {
                fwtn.InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(fwtn.InnerText)).ToString();
            }

            //responseDOM.SelectNodes

            return (responseDOM.InnerXml);

        }

        //------------------------
        //取得檔案或資料夾相關資料
        //------------------------
        public string getEntryInfo(string entryID, bool entryType)
        {
            string mXML;
            string responseXML;
            string eType;

            if (entryType)
                eType = "1";
            else
                eType = "0";

            mXML = "<getentryinfo><token>" + config.sTokenString + "</token><isfolder>" + eType + "</isfolder><entryid>" + entryID + "</entryid></getentryinfo>";
            try
            {
                responseXML = postData(config.sInfoRelayURL + "/fsentry/getentryinfo/", mXML);
                if (responseXML.IndexOf("<status>0</status>") == -1)
                {
                    config.sTokenString = "";
                    config.sEtagString = "";
                    return null;
                }
            }
            catch (Exception ex)
            {
                return "-1:" + ex.Message.ToString();
            }

            XmlDocument responseDOM = new XmlDocument();

            responseDOM.LoadXml(responseXML);
            XmlNodeList folderNodes = responseDOM.SelectNodes("/getentryinfo/display");
            if (folderNodes[0].InnerText.Length > 0)
                folderNodes[0].InnerText = SimpleBase64.Decode(folderNodes[0].InnerText);

            XmlNodeList timeCreation = responseDOM.SelectNodes("/getentryinfo/attribute/creationtime");
            if (timeCreation[0].InnerText.Length > 0)
                timeCreation[0].InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(timeCreation[0].InnerText)).ToString("yyyy-MM-dd HH:mm:ss");

            XmlNodeList timeLasaccess = responseDOM.SelectNodes("/getentryinfo/attribute/lastaccesstime");
            if (timeLasaccess[0].InnerText.Length > 0)
                timeLasaccess[0].InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(timeLasaccess[0].InnerText)).ToString("yyyy-MM-dd HH:mm:ss");

            XmlNodeList timeLastwrite = responseDOM.SelectNodes("/getentryinfo/attribute/lastwritetime");
            if (timeLastwrite[0].InnerText.Length > 0)
                timeLastwrite[0].InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(timeLastwrite[0].InnerText)).ToString("yyyy-MM-dd HH:mm:ss");

            return responseDOM.InnerXml;
        }

        //------------------------
        //設定檔案或資料夾標示 (目前 ASUS WebStorge 只開放星號標示 markid = 1; markid = "" 則清除此檔案或目錄的所有標示)
        //------------------------
        public string setentrymark(string entryID, string[] markID, bool entryType)
        {
            string mXML;
            string responseXML;
            string eType;
            string markIDList;

            if (entryType)
                eType = "1";
            else
                eType = "0";

            if (markID.Length > 1)
            {
                markIDList = String.Join(",", markID);
            }
            else
                markIDList = markID[0].ToString();

            mXML = "<getentryinfo><token>" + config.sTokenString + "</token><isfolder>" + eType + "</isfolder><entryid>" + entryID + "</entryid><markid>" + markIDList + "</markid></getentryinfo>";

            try
            {
                responseXML = postData(config.sInfoRelayURL + "/fsentry/setentrymark/", mXML);
            }
            catch (Exception ex)
            {
                return "-1:" + ex.Message.ToString();
            }
            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("setentrymark");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = null;
                    break;
                case "2":
                    config.sTokenString = "";
                    config.sEtagString = "";
                    strResult = "2:Authentication Fail";
                    break;
                case "218":
                    strResult = "218:要處理的目錄不存在或已刪除";
                    break;
                case "219":
                    strResult = "219:檔案不存在或已刪除";
                    break;
                case "225":
                    strResult = "225:參數值不在容許的定義域內(不合法的 Mark ID, 目前伺服器提供的是1)";
                    break;
                case "999":
                    strResult = "999:General Error";
                    break;
            }

            return strResult;
        }

        //------------------------
        //分享檔案或目錄
        //------------------------
        public string getsharecode(string entryID, string password, bool entryType, string actiontype)
        {
            string mXML;
            string responseXML;
            string eType;

            if (entryType)
                eType = "1"; //file
            else
                eType = "0"; //forder

            string strPassword = "";
            if (actiontype == "2")
            {
                this.hashedPassword(password);
                strPassword = config.sHashedPassword;
            }

            mXML = "<getsharecode><token>" + config.sTokenString + "</token><userid>" + config.sUsername + "</userid><entrytype>" + eType + "</entrytype><entryid>" + entryID + "</entryid><password>" + strPassword + "</password><actiontype>" + actiontype + "</actiontype></getsharecode>";
            try
            {
                responseXML = postData(config.sInfoRelayURL + "/fsentry/getsharecode/", mXML);
            }
            catch (Exception ex)
            {
                return "-1:" + ex.Message.ToString();
            }

            return responseXML;
        }

        //------------------------
        //取消分享檔案或目錄
        //------------------------
        public string deletesharecode(string entryID, string password, bool entryType)
        {
            string mXML;
            string responseXML;
            string eType;

            if (entryType)
                eType = "1"; //file
            else
                eType = "0"; //forder

            mXML = "<deletesharecode><token>" + config.sTokenString + "</token><userid>" + config.sUsername + "</userid><entrytype>" + eType + "</entrytype><entryid>" + entryID + "</entryid><password></password></deletesharecode>";

            try
            {
                responseXML = postData(config.sInfoRelayURL + "/fsentry/deletesharecode/", mXML);
            }
            catch (Exception ex)
            {
                return "-1:" + ex.Message.ToString();
            }

            return responseXML;
        }

        //------------------------
        //查詢分享檔案是否有密碼保護
        //------------------------
        public string checkpassword(string sharecode)
        {
            string mXML;
            string responseXML;

            mXML = "<checkpassword><token>" + config.sTokenString + "</token><userid>" + config.sUsername + "</userid><suri>" + sharecode + "</suri></checkpassword>";
            try
            {
                responseXML = postData(config.sInfoRelayURL + "/fsentry/checkpassword/", mXML);
            }
            catch (Exception ex)
            {
                return "-1:" + ex.Message.ToString();
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            
            xNodes = xmlRead.SelectNodes("checkpassword");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = xNodes.Item(0).SelectNodes("ifpassword").Item(0).InnerText;
                    break;
                case "2":
                    config.sTokenString = "";
                    config.sEtagString = "";
                    strResult = "2:Authentication Fail";
                    break;
                case "3":
                    strResult = "3:Payload is not validate";
                    break;
                case "218":
                    strResult = "218:要處理的目錄不存在或已刪除";
                    break;
                case "219":
                    strResult = "219:要處理的檔案不存在或已刪除";
                    break;
                case "229":
                    strResult = "229:存取指定檔案的密碼不正確";
                    break;
                case "999":
                    strResult = "999:General Error";
                    break;
            }

            return strResult;
        }

        //------------------------
        //驗證分享檔案密碼
        //------------------------
        public string comparepassword(string entryID, string password, bool entrytype)
        {
            string mXML;
            string responseXML;
            string eType;

            if (entrytype)
                eType = "1"; //folder
            else
                eType = "0"; //file

            string strPassword = "";
            this.hashedPassword(password);
            strPassword = config.sHashedPassword;

            mXML = "<comparepassword><token>" + config.sTokenString + "</token><userid>" + config.sUsername + "</userid><isfolder>" + eType + "</isfolder><ffid>" + entryID + "</ffid><passwd>" + strPassword + "</passwd></comparepassword>";
            try
            {
                responseXML = postData(config.sInfoRelayURL + "/fsentry/comparepassword/", mXML);
            }
            catch (Exception ex)
            {
                return "-1:" + ex.Message.ToString();
            }
            
            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("comparepassword");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = null;
                    break;
                case "2":
                    config.sTokenString = "";
                    config.sEtagString = "";
                    strResult = "2:Authentication Fail";
                    break;
                case "3":
                    strResult = "3:Payload is not validate";
                    break;
                case "218":
                    strResult = "218:要處理的目錄不存在或已刪除";
                    break;
                case "219":
                    strResult = "219:要處理的檔案不存在或已刪除";
                    break;
                case "229":
                    strResult = "229:存取指定檔案的密碼不正確";
                    break;
                case "999":
                    strResult = "999:General Error";
                    break;
            }

            return strResult;
        }

        //------------------------
        //分享檔案列表
        //------------------------
        public string getsharedentries()
        {
            string mXML;
            string responseXML;

            mXML = "<getsharedentries><token>" + config.sTokenString + "</token><userid>" + config.sUsername + "</userid><kind></kind><pagesize></pagesize><sortby></sortby><sortdirection></sortdirection><firstentrybound></firstentrybound></getsharedentries>";
            try
            {
                responseXML = postData(config.sInfoRelayURL + "/fsentry/getsharedentries/", mXML);
            }
            catch (Exception ex)
            {
                return "-1:" + ex.Message.ToString();
            }

            XmlDocument responseDOM = new XmlDocument();

            responseDOM.LoadXml(responseXML);

            XmlNodeList timeCreation = responseDOM.SelectNodes("/getsharedentries/entry/attribute/creationtime");
            if (timeCreation[0].InnerText.Length > 0)
                timeCreation[0].InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(timeCreation[0].InnerText)).ToString("yyyy-MM-dd HH:mm:ss");

            XmlNodeList timeLasaccess = responseDOM.SelectNodes("/getsharedentries/entry/attribute/lastaccesstime");
            if (timeLasaccess[0].InnerText.Length > 0)
                timeLasaccess[0].InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(timeLasaccess[0].InnerText)).ToString("yyyy-MM-dd HH:mm:ss");

            XmlNodeList timeLastwrite = responseDOM.SelectNodes("/getsharedentries/entry/attribute/lastwritetime");
            if (timeLastwrite[0].InnerText.Length > 0)
                timeLastwrite[0].InnerText = Times.ConvertTimestamp2Datetime(Convert.ToDouble(timeLastwrite[0].InnerText)).ToString("yyyy-MM-dd HH:mm:ss");

            return responseDOM.InnerXml;
        }

        //------------------------
        //取得MySyncFolder
        //------------------------
        public string getMySyncFolder()
        {
            string mXML;
            string responseXML;

            mXML = "<getmysyncfolder><token>" + config.sTokenString + "</token><userid>" + config.sUsername + "</userid></getmysyncfolder>";
            try
            {
                responseXML = postData(config.sInfoRelayURL + "/folder/getmysyncfolder/", mXML);
            }
            catch (Exception ex)
            {
                return "-1:" + ex.Message.ToString();
            }

            XmlDocument responseDOM = new XmlDocument();

            responseDOM.LoadXml(responseXML);
            XmlNodeList statusNodes = responseDOM.SelectNodes("/getmysyncfolder/status");
            XmlNodeList folderNodes = responseDOM.SelectNodes("/getmysyncfolder/id");
            string mySyncFolderID = null;

            if (folderNodes[0].InnerText.Length > 0)
                mySyncFolderID = folderNodes[0].InnerText;

            string status = statusNodes[0].InnerText;
            if (status != "0" )
                config.sLastError = "getMySyncFolder():" + status;

            return mySyncFolderID;
        }        

        //Web Relay
        //------------------------
        //取得 Transaction ID
        //------------------------
        private string getFileExtValue(string fileName, int columnIndex)
        {
            string returnMsg;
            returnMsg = "";

            if (config.sFileExtValue != null)
            {
                foreach (DictionaryEntry hsTable in config.sFileExtValue)
                {
                    if (hsTable.Key == fileName)
                    {
                        switch (columnIndex)
                        {
                            case 0:
                                returnMsg = hsTable.Key.ToString();
                                break;
                            case 1:
                                returnMsg = hsTable.Value.ToString();
                                break;
                        }
                    }
                }
            }
            return returnMsg;
        }

        //------------------------
        //上傳檔案
        //------------------------
        public int uploadFile(string filePath, string folderID, bool overWrite)
        {
            string resultMsg = "";
            string fileName;
            long fileSize;
            string[] attribute;
            FileStream fsFile;
            string transactionID;
            string fileID = "";

            transactionID = "";

            FileInfo fi = new FileInfo(filePath);
            fileSize = fi.Length;
            fileName = fi.Name;

            attribute = new string[] { fi.CreationTime.ToString(), fi.LastAccessTime.ToString(), fi.LastWriteTime.ToString() };
            fsFile = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            transactionID = getFileExtValue(fileName, 1);//續傳 : transaction id

            if (overWrite == true) //overwrite : fileid
            {
                string resultXML;
                resultXML = browseFolder(folderID, null);
                XmlDocument xmlRead = new XmlDocument();
                xmlRead.LoadXml(resultXML);
                XmlNodeList xNodes = xmlRead.SelectNodes("browse");
                string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

                if (strResult == "0")
                {
                    XmlNodeList xfileNodes = xmlRead.SelectNodes("//browse/file");
                    string display;
                    foreach (XmlNode xctn in xfileNodes)
                    {
                        display = xctn.SelectNodes("display").Item(0).InnerText;
                        if (display == fileName)
                        {
                            fileID = xctn.SelectNodes("id").Item(0).InnerText;
                        }
                    }
                }
                else
                    return 0x000003FF; //檔案已經存在
                
            }

            int iRet = initbinaryupload(fileName, folderID, fileSize, attribute, fsFile, overWrite, transactionID, fileID);

            if (iRet != 0)
            {
                return iRet;
            }

            if (transactionID == "")
                transactionID = getFileExtValue(fileName, 1);

            //上傳檔案
            iRet = 0;
            iRet = resumebinaryupload(fsFile, fileSize, transactionID);

            if (iRet != 0)
            {
                return iRet;
            }

            //上傳檔案完成
            iRet = 0;
            iRet = finishbinaryupload(transactionID);

            fsFile.Close();

            return iRet;

        }

        //下載檔案
        public void DownloadFile(string FileName, string parentID)
        {
            //取得fileid
            string xml = this.getFileID(FileName, parentID);

            //呼叫DirectDownload

            string url = this.config.sWebRelayURL + "/webrelay/directdownload/" + this.config.sTokenString + "/?fi=" + xml;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream resStream = response.GetResponseStream();

            using (Stream output = File.OpenWrite(FileName))
            using (Stream input = resStream)
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, bytesRead);
                }
            }



            return;

            
            

        }

        //下載檔案轉成字串回傳
        public string DownloadFileToString(string FileName, string parentID)
        {
            //取得fileid
            string xml = this.getFileID(FileName, parentID);

            //呼叫DirectDownload

            string url = this.config.sWebRelayURL + "/webrelay/directdownload/" + this.config.sTokenString + "/?fi=" + xml;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream resStream = response.GetResponseStream();

            //using (Stream output = File.OpenWrite(FileName))
            MemoryStream output;
            using (Stream input = resStream)
            {
                output = new MemoryStream();

                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, bytesRead);
                }
            }

            output.Seek(0, SeekOrigin.Begin);

            string data;

            using (TextReader reader = new StreamReader(output, Encoding.UTF8))
            {
                data = reader.ReadToEnd();
            }

            return data;

        }


        //自memory上傳檔案
        //------------------------
        public int uploadFilefromMemory(MemoryStream ms, string fileName, string folderID, bool overWrite)
        {
            string resultMsg = "";
            int iRet = 0;
            long fileSize;
            string[] attribute;
            FileStream fsFile;
            string transactionID;
            string fileID = "";

            transactionID = "";
            string nowtime = DateTime.Now.ToLocalTime().ToString();

            attribute = new string[] { nowtime, nowtime, nowtime };
            

            transactionID = getFileExtValue(fileName, 1);//續傳 : transaction id

            if (overWrite == true) //overwrite : fileid
            {
                string resultXML;
                resultXML = browseFolder(folderID, null);
                XmlDocument xmlRead = new XmlDocument();
                xmlRead.LoadXml(resultXML);
                XmlNodeList xNodes = xmlRead.SelectNodes("browse");
                string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

                if (strResult == "0")
                {
                    XmlNodeList xfileNodes = xmlRead.SelectNodes("//browse/file");
                    string display;
                    foreach (XmlNode xctn in xfileNodes)
                    {
                        display = xctn.SelectNodes("display").Item(0).InnerText;
                        if (display == fileName)
                        {
                            fileID = xctn.SelectNodes("id").Item(0).InnerText;
                        }
                    }
                }
                else
                    return 0x000003FF;

            }

            iRet = initbinaryupload(fileName, folderID, attribute, ms, overWrite, transactionID, fileID);

            if (iRet != 0)
            {
                return iRet;
            }

            if (transactionID == "")
                transactionID = getFileExtValue(fileName, 1);

            //上傳檔案
            iRet = 0;
            iRet = resumebinaryupload(ms, transactionID);

            if (iRet != 0)
            {
                return iRet;
            }

            //上傳檔案完成
            iRet = 0;
            iRet = finishbinaryupload(transactionID);

            ms.Close();

            return iRet;

        }

        //------------------------
        //初始上傳檔案(from Memory)
        //------------------------
        private int initbinaryupload(string fileName, string parentID, string[] attribute, MemoryStream ms, bool isResume, string transactionID, string fileID)
        {
            config.sLatestCheckSum = "";
            string responseXML;
            responseXML = "";

            //初始
            string queryString;
            string dis = "?dis=" + config.sSID;
            string tk = "&tk=" + config.sTokenString;
            string na = "&na=" + HttpUtility.UrlEncode(SimpleBase64.Encode(fileName));
            parentID = "&pa=" + parentID;
            string sg = "&sg=" + Crypto.doHMacSH512(ms);
            string at = "&at=" + HttpUtility.UrlEncode(createAttributeTag(Convert.ToDateTime(attribute[0]), Convert.ToDateTime(attribute[1]), Convert.ToDateTime(attribute[2])));
            string fs = "&fs=" + ms.Length.ToString();
            string tx = "";
            if (transactionID != "")
                tx = "&tx=" + transactionID;

            string fi = "";
            if (fileID != "")
                fi = "&fi=" + fileID;

            string sc = "";
            //string sc = "&sc=" + ;

            queryString = dis + tk + na + parentID + sg + at + fs + tx + fi + sc;

            try
            {
                responseXML = postData(config.sWebRelayURL + "/webrelay/initbinaryupload/" + queryString, "");
            }
            catch (Exception ex)
            {
                this.ExceptionMsg = ex.Message;
                return -1;
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("initbinaryupload");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = "0";
                    if (config.sFileExtValue == null)
                    {
                        config.sFileExtValue = new Hashtable();
                        config.sFileExtValue.Add(fileName, xNodes.Item(0).SelectNodes("transid").Item(0).InnerText.ToString());
                    }
                    else
                    {
                        string ExistFileName = getFileExtValue(fileName, 0);
                        if (ExistFileName == "")
                        {
                            config.sFileExtValue.Add(fileName, xNodes.Item(0).SelectNodes("transid").Item(0).InnerText.ToString());
                        }
                    }

                    if (fileID != "")
                        config.sLatestCheckSum = xNodes.Item(0).SelectNodes("latestchecksum").Item(0).InnerText;
                    break;
                case "2":
                    config.sTokenString = "";
                    config.sEtagString = "";
                    strResult = "2";
                    break;
                case "211":
                    strResult = "211";
                    break;
                case "213":
                    strResult = "213";
                    break;
                case "214":
                    strResult = "214";
                    break;
                case "218":
                    strResult = "218";
                    break;
                case "219":
                    strResult = "219";
                    break;
                case "220":
                    strResult = "220";
                    break;
                case "221":
                    strResult = "221";
                    break;
                case "224":
                    strResult = "224";
                    break;
                case "226":
                    strResult = "226";
                    break;
                case "250":
                    strResult = "250";
                    break;
                case "251":
                    strResult = "251";
                    break;
                case "252":
                    strResult = "252";
                    break;
                case "999":
                    strResult = "999";
                    break;
            }

            return Convert.ToInt32(strResult);
        }



        //------------------------
        //上傳檔案初始
        //------------------------
        private int initbinaryupload(string fileName, string parentID, long fileSize, string[] attribute, FileStream fsFile, bool isResume, string transactionID, string fileID)
        {
            config.sLatestCheckSum = "";
            string responseXML;
            responseXML = "";

            //初始
            string queryString;
            string dis = "?dis=" + config.sSID;
            string tk = "&tk=" + config.sTokenString;
            string na = "&na=" + HttpUtility.UrlEncode(SimpleBase64.Encode(fileName));
            parentID = "&pa=" + parentID;
            string sg = "&sg=" + Crypto.doHMacSH512(fsFile);
            string at = "&at=" + HttpUtility.UrlEncode(createAttributeTag(Convert.ToDateTime(attribute[0]), Convert.ToDateTime(attribute[1]), Convert.ToDateTime(attribute[2])));
            string fs = "&fs=" + fileSize.ToString();
            string tx = "";
            if (transactionID != "")
                tx = "&tx=" + transactionID;

            string fi = "";
            if (fileID != "")
                fi = "&fi=" + fileID;

            string sc = "";
            //string sc = "&sc=" + ;

            queryString = dis + tk + na +  parentID + sg + at + fs + tx + fi + sc;

            try
            {
                responseXML = postData(config.sWebRelayURL + "/webrelay/initbinaryupload/" + queryString, "");
            }
            catch (Exception ex)
            {
                this.ExceptionMsg = ex.Message;
                return -1;
                
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("initbinaryupload");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = "0";
                    if (config.sFileExtValue == null)
                    {
                        config.sFileExtValue = new Hashtable();
                        config.sFileExtValue.Add(fileName, xNodes.Item(0).SelectNodes("transid").Item(0).InnerText.ToString());
                    }
                    else
                    {
                        string ExistFileName = getFileExtValue(fileName, 0);
                        if (ExistFileName == "")
                        {
                            config.sFileExtValue.Add(fileName, xNodes.Item(0).SelectNodes("transid").Item(0).InnerText.ToString());
                        }
                    }
                    
                    if (fileID != "")
                        config.sLatestCheckSum = xNodes.Item(0).SelectNodes("latestchecksum").Item(0).InnerText;
                    break;
                case "2":
                    config.sTokenString = "";
                    config.sEtagString = "";
                    strResult = "2";
                    break;
                case "211":
                    strResult = "211";
                    break;
                case "213":
                    strResult = "213";
                    break;
                case "214":
                    strResult = "214";
                    break;
                case "218":
                    strResult = "218";
                    break;
                case "219":
                    strResult = "219";
                    break;
                case "220":
                    strResult = "220";
                    break;
                case "221":
                    strResult = "221";
                    break;
                case "224":
                    strResult = "224";
                    break;
                case "226":
                    strResult = "226";
                    break;
                case "250":
                    strResult = "250";
                    break;
                case "251":
                    strResult = "251";
                    break;
                case "252":
                    strResult = "252";
                    break;
                case "999":
                    strResult = "999";
                    break;
            }

            return Convert.ToInt32( strResult);
        }

        //------------------------
        //上傳檔案(From Memory)
        //------------------------
        private int resumebinaryupload(MemoryStream ms,  string transactionID)
        {
            string responseXML;
            responseXML = "";
            string queryString;

            string dis = "?dis=" + config.sSID;
            string tk = "&tk=" + config.sTokenString;
            string tx = "&tx=" + transactionID;

            queryString = dis + tk + tx;

            try
            {
                responseXML = postData(config.sWebRelayURL + "/webrelay/resumebinaryupload/" + queryString, ms);
            }
            catch (Exception ex)
            {
                this.ExceptionMsg = ex.Message;
                return -1;
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("resumebinaryupload");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = "0";
                    break;
                case "220":
                    strResult = "220";
                    break;
                case "251":
                    strResult = "251";
                    break;
                case "999":
                    strResult = "999";
                    break;
            }

            return Convert.ToInt32(strResult);
        }

        //------------------------
        //上傳檔案
        //------------------------
        private int resumebinaryupload(FileStream fsFile, long fileSize, string transactionID)
        {
            string responseXML;
            responseXML = "";
            string queryString;

            string dis = "?dis=" + config.sSID;
            string tk = "&tk=" + config.sTokenString;
            string tx = "&tx=" + transactionID;

            queryString = dis + tk + tx;

            try
            {
                responseXML = postData(config.sWebRelayURL + "/webrelay/resumebinaryupload/" + queryString, fsFile, fileSize);
            }
            catch (Exception ex)
            {
                this.ExceptionMsg = ex.Message;
                return -1;
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("resumebinaryupload");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    break;
                case "220":
                    strResult = "220";
                    break;
                case "251":
                    strResult = "251";
                    break;
                case "999":
                    strResult = "999";
                    break;
            }

            return Convert.ToInt32(strResult);
        }

        //------------------------
        //上傳檔案完成
        //------------------------
        private int finishbinaryupload(string transactionID)
        {
            string responseXML;
            responseXML = "";
            string queryString;

            string dis = "?dis=" + config.sSID;
            string tk = "&tk=" + config.sTokenString;
            string tx = "&tx=" + transactionID;
            string lsg = "";
            if (config.sLatestCheckSum != "")
                lsg = "&lsg=" + config.sLatestCheckSum;

            queryString = dis + tk + tx + lsg;

            try
            {
                responseXML = postData(config.sWebRelayURL + "/webrelay/finishbinaryupload/" + queryString, "");
            }
            catch (Exception ex)
            {
                this.ExceptionMsg = ex.Message;
                return -1;
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("finishbinaryupload");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;

            switch (strResult)
            {
                case "0": //success
                    strResult = "0";
                    string removeKey = "";
                    foreach (DictionaryEntry hsTable in config.sFileExtValue)
                    {
                        if (hsTable.Value.ToString() == transactionID)
                        {
                            removeKey = hsTable.Key.ToString();
                        }
                    }
                    config.sFileExtValue.Remove(removeKey);//檔案已全部上載完畢, 清除此 file 的 data
                    break;
                case "2":
                    config.sTokenString = "";
                    config.sEtagString = "";
                    strResult = "2";
                    break;
                case "218":
                    strResult = "218";
                    break;
                case "219":
                    strResult = "219";
                    break;
                case "220":
                    strResult = "220";
                    break;
                case "250":
                    strResult = "250";
                    break;
                case "999":
                    strResult = "999";
                    break;
            }

            return Convert.ToInt32( strResult);
        }

        //-------- For HBase ---------
        public bool PostToHBase(string rawData, string apiDirectory)
        {
            string status;
            bool OK = true;
            this.ExceptionMsg = null;
            this.dbErrorCode = 0;
            try
            {
                status = postData(this.config.tsdbaseURL + apiDirectory, "", "" , rawData);
                this.dbErrorCode = Convert.ToInt32(status);
            }
            catch (Exception ex)
            {
                this.dbErrorCode = 1000;
                ExceptionMsg = ex.Message;
                OK = false;
            }

            if (this.dbErrorCode != 0)
            {
                OK = false;
                ExceptionMsg = getErrorMsg(this.dbErrorCode);
            }
            return OK;
        }

        private string getErrorMsg(int errorCode)
        {
            string msg;
            switch (errorCode)
            {
                case 2:
                    msg = "Authentication Fail";
                    break;
                case 5:
                    msg = "Authorization Fail";
                    break;
                
                case 201:
                    msg = "AUTH Input Data FAIL";
                    break;

                case 300:
                    msg = "Stream Exception";
                    break;

                case 301:
                    msg = "Xml Stream Exception";
                    break;

                case 310:
                    msg = "Required Field Validator Exception";
                    break;
                
                case 311:
                    msg = "Field Format Exception";
                    break;
                
                case 404:
                    msg = "Schema Not Found";
                    break;

                case 405:
                    msg = "Action Not Support";
                    break;

                case 999:
                    msg = "A general Erro";
                    break;
                default:
                    msg = "UnExpected Error";
                    break;

            }

            return msg;
        }

        public string QueryHBase(string query, string apiDirectory)
        {
            string status;
            string  retData = null;
            this.ExceptionMsg = null;
            this.dbErrorCode = 0;
            try
            {
                status = postData(this.config.tsdbaseURL + apiDirectory, "", "", query);
                this.dbErrorCode = Convert.ToInt32(status);
                if (this.dbErrorCode == 0)
                    retData = this.responseData;
            }
            catch (Exception ex)
            {
                this.dbErrorCode = 1000;
                ExceptionMsg = ex.Message;
                retData = null;
            }

            if (this.dbErrorCode != 0)
            {

                ExceptionMsg = getErrorMsg(this.dbErrorCode);

                retData = null;
            }
            return retData;

        }
 
        //------------------------
        //Create ChameleonDB Schema
        //------------------------
        public bool createSchema(string ClientSetName, string payload)
        {
            string status;
            bool OK = true;
            this.ExceptionMsg = null;
            this.dbErrorCode = 0;
            try
            {
                status = postData(this.config.chameleonURL + "/chameleon/", "ChameleonDB.CreateEntrySchema", ClientSetName, payload);
                this.dbErrorCode = Convert.ToInt32(status);
            }
            catch (Exception ex)
            {
                this.dbErrorCode = 1000;
                ExceptionMsg = ex.Message;
                OK = false;
            }

            if (this.dbErrorCode != 0)
                OK =false;

            return OK;
        }

        public string  Query(string ClientSetName, string payload)
        {
            string status;
            
            this.ExceptionMsg = null;
            this.dbErrorCode = 0;
            try
            {
                status = postData(this.config.chameleonURL + "/chameleon/", "ChameleonDB.Query", ClientSetName, payload);
                this.dbErrorCode = Convert.ToInt32(status);
            }
            catch (Exception ex)
            {
                this.dbErrorCode = 1000;
                ExceptionMsg = ex.Message;

            }

            if (this.dbErrorCode != 0)
                return null;

            return null;
        }

        public string QueryByKeyValue(string ClientSetName, string payload)
        {
            string status;

            this.ExceptionMsg = null;
            this.dbErrorCode = 0;
            try
            {
                status = postData(this.config.chameleonURL + "/chameleon/", "ChameleonDB.GetEntry", ClientSetName, payload);
                this.dbErrorCode = Convert.ToInt32(status);
            }
            catch (Exception ex)
            {
                this.dbErrorCode = 1000;
                ExceptionMsg = ex.Message;

            }

            if (this.dbErrorCode != 0)
                return null;

            return this.responseData;
        }

        public bool Insert(string ClientSetName, string payload)
        {
            string status;
            bool OK = true;
            this.ExceptionMsg = null;
            this.dbErrorCode = 0;
            try
            {
                status = postData(this.config.chameleonURL + "/chameleon/", "ChameleonDB.PutEntry", ClientSetName, payload);
                this.dbErrorCode = Convert.ToInt32(status);
            }
            catch (Exception ex)
            {
                this.dbErrorCode = 1000;
                ExceptionMsg = ex.Message;
                OK = false;
            }

            if (this.dbErrorCode != 0)
                OK = false;

            return OK;
        }

        public bool InsertMultiRows(string ClientSetName, string payload)
        {
            string status;
            bool OK = true;
            this.ExceptionMsg = null;
            this.dbErrorCode = 0;
            try
            {
                status = postData(this.config.chameleonURL + "/chameleon/", "ChameleonDB.PutEntries", ClientSetName, payload);
                this.dbErrorCode = Convert.ToInt32(status);
            }
            catch (Exception ex)
            {
                this.dbErrorCode = 1000;
                ExceptionMsg = ex.Message;
                OK = false;
            }

            if (this.dbErrorCode != 0)
                OK = false;

            return OK;
        }
        /*search server
         * fulltext/search
         */
        public void search_sqlquery(bool is_folder, string keyword)
        {
            string xmlsearch;

            //automatically get encrypted flag of old folder
            xmlsearch = "<sqlquery><userid>" + config.sUsername + "</userid>" +
                              "<keyword>" + keyword + "</keyword>" +
                              "<kind>" + "2" + "</kind></sqlquery>";

            string responseXML;
            try
            {
                responseXML = postData(config.searchURL + "/fulltext/sqlquery/" + config.sTokenString, xmlsearch);
            }
            catch (Exception ex)
            {
                return;
            }

            XmlDocument xmlRead = new XmlDocument();
            XmlNodeList xNodes;
            xmlRead.LoadXml(responseXML);
            xNodes = xmlRead.SelectNodes("move");
            string strResult = xNodes.Item(0).SelectNodes("status").Item(0).InnerText;
        }
    }
}
