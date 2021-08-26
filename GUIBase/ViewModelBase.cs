using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
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
