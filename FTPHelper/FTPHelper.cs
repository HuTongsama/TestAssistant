using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Data;
namespace FTP
{
    public class FtpConfig
    {
        public string FtpUrl { get; set; } = string.Empty;
        public string FtpCachePath { get; set; } = string.Empty;
        public string FtpUserName { get; set; } = string.Empty;
        public string FtpPassword { get; set; } = string.Empty;
    }

    public class FTPHelper
    {
        private string _ftpUrl = string.Empty;
        private string _userName = string.Empty;
        private string _password = string.Empty;


        public FTPHelper(string url,string userName,string password)
        {
            _ftpUrl = "ftp://" + url;
            _userName = userName;
            _password = password;
        }

        public void Upload(FileInfo localFile, string dstPath)
        {
            if (localFile == null || !localFile.Exists)
                return;
            string path = _ftpUrl + "/" + dstPath + "/" + HttpUtility.UrlEncode(localFile.Name);
           
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
            
            request.Credentials = new NetworkCredential(_userName, _password);
            request.Method = WebRequestMethods.Ftp.UploadFile;            
            using (Stream stream = request.GetRequestStream())
            {
                using (FileStream fileStream = localFile.OpenRead())
                {
                    byte[] buf = new byte[1024];
                    int count = fileStream.Read(buf, 0, buf.Length);
                    while (count > 0)
                    {
                        stream.Write(buf, 0, count);
                        count = fileStream.Read(buf, 0, buf.Length);
                    }
                    fileStream.Close();
                }
            }
        }

        public bool Download(string ftpPath, string localPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(localPath);
            if (dirInfo.Exists)
            {
                dirInfo.Delete(true);
            }
            dirInfo.Create();
            if (MakeDirectory(ftpPath))
            {
                string path = _ftpUrl + "/" + ftpPath;                
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
                NetworkCredential networkCredential = new NetworkCredential(_userName, _password);
                request.Credentials = networkCredential;
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        string ftpFilePath = path + "/" + line;
                        request = (FtpWebRequest)WebRequest.Create(ftpFilePath);
                        request.Credentials = networkCredential;
                        request.Method = WebRequestMethods.Ftp.DownloadFile;

                        string localFilePath = localPath + "/" + line;
                        using (FileStream fileStream = new FileStream(localFilePath, FileMode.OpenOrCreate))
                        {
                            using (FtpWebResponse ftpResponse = (FtpWebResponse)request.GetResponse())
                            {
                                fileStream.Position = fileStream.Length;
                                byte[] buf = new byte[1024];
                                int count = ftpResponse.GetResponseStream().Read(buf, 0, buf.Length);
                                while (count > 0)
                                {
                                    fileStream.Write(buf, 0, count);
                                    count = ftpResponse.GetResponseStream().Read(buf, 0, buf.Length);
                                }
                                ftpResponse.GetResponseStream().Close();
                            }
                        }
                        line = reader.ReadLine();
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        //can check if directory exist
        public bool MakeDirectory(string dir)
        {
            bool hasDir = false;
            var paths = dir.Split('/');           
            string curPath = _ftpUrl;
            foreach (var tmpPath in paths)
            {
                try
                {                   
                    curPath = curPath + "/" + tmpPath;                  
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(curPath);
                    request.Credentials = new NetworkCredential(_userName, _password);
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;

                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {
                        hasDir = true;
                    }
                }
                catch (Exception e)
                {
                    WebException webE = e as WebException;
                    FtpWebResponse response = null;
                    if (webE != null)
                         response = (FtpWebResponse)webE.Response;
                    if (response != null && response.StatusCode ==
                        FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        hasDir = true;
                    }
                    else 
                    {
                        CommonFuction.WriteLog(e.Message + '\r');
                        hasDir = false;
                        return hasDir;
                    }
                }
                if (!hasDir)
                    return false;
            }
            return hasDir;
            
        }

        public void UploadDirectory(DirectoryInfo localDirectory, string dstPath)
        {
            var files = localDirectory.GetFiles();
            var directories = localDirectory.GetDirectories();

            string ftpPath = dstPath + "/" + localDirectory.Name;
            if (MakeDirectory(ftpPath))
            {
                foreach (var file in files)
                {
                    Upload(file, ftpPath);
                }
            }
            foreach (var dir in directories)
            {
                UploadDirectory(dir, ftpPath);
            }
        }

    }
}
