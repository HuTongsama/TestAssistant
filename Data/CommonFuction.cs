using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Data
{
    public class CommonFuction
    {
        static public List<string> GetAllFiles(string dir, string searchPattern)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                FileInfo[] files = dirInfo.GetFiles(searchPattern);
                List<string> fileNames = new List<string>();
                foreach (var file in files)
                {
                    fileNames.Add(file.Name);
                }
                return fileNames;
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format("{0} is invalid directory", dir));
                return new List<string>();
            }

        }

        static public void WriteLog(string logMessage)
        {
            string fileName = "ServerLog.txt";
            if (!File.Exists(fileName))
            {
                var stream = System.IO.File.OpenWrite(fileName);
                stream.Close();
            }
            System.IO.File.AppendAllText(fileName, logMessage + "\r\n");
        }
    }
}
