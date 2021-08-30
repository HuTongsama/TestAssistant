using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace GUIBase
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void NotifyPropertyChanged(string propertyName)
        {
            if (propertyName != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _logMessage = string.Empty;

        public string ViewMessage
        {
            get => _logMessage;
            set
            {
                if (value != _logMessage)
                {
                    _logMessage = value;
                    NotifyPropertyChanged("ViewMessage");
                }
            }
        }
        protected string OnPathButtonClicked()
        {
            string path = null;
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select Path";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                path = folderBrowserDialog.SelectedPath;
            }
            return path;
        }
        protected void LogMessage(string message)
        {
            string logInfo = ViewMessage + "\n" + message + "\n";
            ViewMessage = logInfo;
        }
        public Action<string> SaveConfig { get; set; } = delegate { };
        public static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }
        protected void SetConfigValue(string configKey, string value)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[configKey].Value = value;
            config.Save();
        }
        protected string GetConfigKey(string prefix, string key)
        {
            return prefix.ToLower() + key;
        }
    }
}
