using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FTP
{
    public class FTPHelper
    {
        private string _ftpUrl = string.Empty;
        private string _userName = string.Empty;
        private string _password = string.Empty;
        private string _ftpPath = string.Empty;


        public FTPHelper()
        {
            _ftpUrl = "ftp://192.168.8.20";
            _ftpPath = "Users/hutong";
        }

        public void Upload(FileInfo localFile, string dstPath)
        {
            if (localFile == null || !localFile.Exists)
                return;
            string path = _ftpUrl + "/" + dstPath + "/" + localFile.Name;
           
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
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        string ftpFilePath = path + "/" + line;
                        request = (FtpWebRequest)WebRequest.Create(ftpFilePath);
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
            string path = _ftpUrl + "/" + dir;
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
            request.Credentials = new NetworkCredential(_userName, _password);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            try
            {
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    return true;
                }
            }
            catch (WebException e)
            {              
                FtpWebResponse response = (FtpWebResponse)e.Response;
                if (response.StatusCode ==
                    FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    return true;
                    //Does not exist
                }
            }
            return false;
        }

        public void UploadDirectory(DirectoryInfo localDirectory, string dstPath)
        {
            var files = localDirectory.GetFiles();
            var directories = localDirectory.GetDirectories();

            if (MakeDirectory(dstPath))
            {
                foreach (var file in files)
                {
                    Upload(file, dstPath);
                }
            }
            foreach (var dir in directories)
            {
                UploadDirectory(dir, dstPath + "/" + dir.Name);
            }
        }

    }
}
