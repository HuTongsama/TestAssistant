using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Data;
using GUIBase;
namespace TestServer
{
    public class TabItemViewModel : ViewModelBase
    {
        string _pictureSetPath = string.Empty;
        public string PictureSetPath 
        {
            get { return _pictureSetPath; }
            set
            {
                if(value != _pictureSetPath)
                {
                    _pictureSetPath = value;
                    SetConfigValue(GetConfigKey(Header,"PicturePath"), value);
                    if (_pathToWatcher.ContainsKey(PictureSetPath))
                    {
                        FileSystemWatcher watcher = new FileSystemWatcher(PictureSetPath);
                        watcher.EnableRaisingEvents = true;
                        watcher.Created += PictureSetContentCreated;
                        _pathToWatcher[PictureSetPath] = watcher;
                    }
                    else
                    {
                        FileSystemWatcher watcher = new FileSystemWatcher(PictureSetPath);
                        watcher.EnableRaisingEvents = true;
                        watcher.Created += PictureSetContentCreated;
                        _pathToWatcher.Add(PictureSetPath, watcher);
                    }
                    NotifyPropertyChanged("PictureSetPath");
                }
            }
        }
        string _templatePath = string.Empty;
        public string TemplatePath 
        {
            get { return _templatePath; }
            set
            {
                if (value != _templatePath)
                {
                    _templatePath = value;
                    SetConfigValue(GetConfigKey(Header,"TemplatePath"), value);
                    if (_pathToWatcher.ContainsKey(TemplatePath))
                    {
                        FileSystemWatcher watcher = new FileSystemWatcher(TemplatePath);
                        watcher.EnableRaisingEvents = true;
                        watcher.Created += TemplateSetContentCreated;
                        _pathToWatcher[TemplatePath] = watcher;
                    }
                    else
                    {
                        FileSystemWatcher watcher = new FileSystemWatcher(TemplatePath);
                        watcher.EnableRaisingEvents = true;
                        watcher.Created += TemplateSetContentCreated;
                        _pathToWatcher.Add(TemplatePath, watcher);
                    }
                    NotifyPropertyChanged("TemplatePath");
                }
            }
        }
        string _x86ProgramPath = string.Empty;
        public string X86ProgramPath
        {
            get => _x86ProgramPath;
            set
            {
                if (value != _x86ProgramPath)
                {
                    _x86ProgramPath = value;
                    SetConfigValue(GetConfigKey(Header,"X86ProgramPath"), value);
                    NotifyPropertyChanged("X86ProgramPath");
                }
            }
        }
        string _x64ProgramPath = string.Empty;
        public string X64ProgramPath
        {
            get => _x64ProgramPath;
            set
            {
                if (value != _x64ProgramPath)
                {
                    _x64ProgramPath = value;
                    SetConfigValue(GetConfigKey(Header, "X64ProgramPath"), value);
                    NotifyPropertyChanged("X64ProgramPath");
                }
            }
        }
        string _stdVersionPath = string.Empty;
        public string StdVersionPath
        {
            get => _stdVersionPath;
            set
            {
                if (value != _stdVersionPath)
                {
                    _stdVersionPath = value;
                    SetConfigValue(GetConfigKey(Header, "StdVersionPath"), value);
                    if (_pathToWatcher.ContainsKey(StdVersionPath))
                    {
                        FileSystemWatcher watcher = new FileSystemWatcher(StdVersionPath);
                        watcher.EnableRaisingEvents = true;
                        watcher.Created += StdVersionContentCreated;
                        _pathToWatcher[StdVersionPath] = watcher;
                    }
                    else
                    {
                        FileSystemWatcher watcher = new FileSystemWatcher(StdVersionPath);
                        watcher.Created += StdVersionContentCreated;
                        watcher.EnableRaisingEvents = true;                      
                        _pathToWatcher.Add(StdVersionPath, watcher);
                    }
                    NotifyPropertyChanged("StdVersionPath");
                }
            }
        }

        
        private void OnPicSetPathButtonClicked(object obj)
        {
            string path = OnPathButtonClicked();
            if (path != null)
            {
                PictureSetPath = path;
            }
        }
        RelayCommand _picSetPathButtonClicked;
        public ICommand PicSetPathButtonClicked
        {
            get 
            {
                if (_picSetPathButtonClicked == null)
                {
                    _picSetPathButtonClicked = new RelayCommand(OnPicSetPathButtonClicked, delegate { return true; });
                }
                return _picSetPathButtonClicked;
            }
        }

        private void OnTemplatePathButtonClicked(object obj)
        {
            string path = OnPathButtonClicked();
            if (path != null)
            {
                TemplatePath = path;
            }
        }
        RelayCommand _templatePathButtonClicked;
        public ICommand TemplatePathButtonClicked
        {
            get 
            {
                if (_templatePathButtonClicked == null)
                {
                    _templatePathButtonClicked = new RelayCommand(OnTemplatePathButtonClicked, delegate { return true; });
                }
                return _templatePathButtonClicked;
            }
        }

        private void OnX86ProgramPathButtonClicked(object obj)
        {
            string path = OnPathButtonClicked();
            if (path != null)
            {
                X86ProgramPath = path;
            }
        }
        RelayCommand _x86ProgramButtonClicked;
        public ICommand X86ProgramButtonClicked
        {
            get
            {
                if (_x86ProgramButtonClicked == null)
                {
                    _x86ProgramButtonClicked = new RelayCommand(OnX86ProgramPathButtonClicked, delegate { return true; });
                }
                return _x86ProgramButtonClicked;
            }
        }

        private void OnX64ProgramPathButtonClicked(object obj)
        {
            string path = OnPathButtonClicked();
            if (path != null)
            {
                X64ProgramPath = path;
            }
        }
        RelayCommand _x64ProgramButtonClicked;
        public ICommand X64ProgramButtonClicked
        {
            get
            {
                if (_x64ProgramButtonClicked == null)
                {
                    _x64ProgramButtonClicked = new RelayCommand(OnX64ProgramPathButtonClicked, delegate { return true; });
                }
                return _x64ProgramButtonClicked;
            }
        }

        private void OnStdVersionButtonClicked(object obj)
        {
            string path = OnPathButtonClicked();
            if (path != null)
            {
                StdVersionPath = path;
            }
        }
        RelayCommand _stdVersionButtonCommand;
        public ICommand StdVersionButtonCommand
        {
            get
            {
                if (_stdVersionButtonCommand == null)
                {
                    _stdVersionButtonCommand = new RelayCommand(OnStdVersionButtonClicked, delegate { return true; });   
                }
                return _stdVersionButtonCommand;
            }
        }
        private string _header = string.Empty;
        public string Header
        {
            set 
            {
                if (value != _header)
                {
                    _header = value;
                    NotifyPropertyChanged("Header");
                }                
            }
            get => _header;
            
        }
        public Action<byte[]> SendDataCallback { get; set; } = delegate { };
        Dictionary<string, FileSystemWatcher> _pathToWatcher = new Dictionary<string, FileSystemWatcher>();
        public TabItemViewModel(string header = "",
            string pictureSetPath = "",
            string templatePath = "",
            string x86ProgramPath = "",
            string x64ProgramPath = "",
            string stdVersionPath ="")
        {
            Header = header;
            PictureSetPath = pictureSetPath;          
            TemplatePath = templatePath;
            X86ProgramPath = x86ProgramPath;
            X64ProgramPath = x64ProgramPath;
            StdVersionPath = stdVersionPath;
        }
        private void PictureSetContentCreated(object obj, FileSystemEventArgs e)
        {
            ServerData serverData = new ServerData();
            DirectoryInfo dirInfo = new DirectoryInfo(PictureSetPath);
            var allCsvs = dirInfo.GetFiles("*.csv");
            foreach (var csv in allCsvs)
            {
                serverData.PictureSetList.Add(csv.Name);
            }
            Dictionary<string, ServerData> keyValues = new Dictionary<string, ServerData>()
            {
                { Header,serverData}
            };
            string jsonString = JsonSerializer.Serialize(keyValues);
            byte[] data = System.Text.Encoding.ASCII.GetBytes(jsonString);
            SendDataCallback(data);

        }
        private void TemplateSetContentCreated(object obj, FileSystemEventArgs e)
        {
            ServerData serverData = new ServerData();
            DirectoryInfo dirInfo = new DirectoryInfo(PictureSetPath);
            var allJsons = dirInfo.GetFiles("*.json");
            foreach (var template in allJsons)
            {
                serverData.TemplateList.Add(template.Name);
            }
            Dictionary<string, ServerData> keyValues = new Dictionary<string, ServerData>()
            {
                { Header,serverData}
            };
            string jsonString = JsonSerializer.Serialize(keyValues);
            byte[] data = System.Text.Encoding.ASCII.GetBytes(jsonString);
            SendDataCallback(data);
        }
        private void StdVersionContentCreated(object obj, FileSystemEventArgs e)
        {
            ServerData serverData = new ServerData();
            DirectoryInfo dirInfo = new DirectoryInfo(PictureSetPath);
            var allDirs = dirInfo.GetDirectories();
            foreach (var dir in allDirs)
            {
                serverData.StdVersionList.Add(dir.Name);
            }
            Dictionary<string, ServerData> keyValues = new Dictionary<string, ServerData>()
            {
                { Header,serverData}
            };
            string jsonString = JsonSerializer.Serialize(keyValues);
            byte[] data = System.Text.Encoding.ASCII.GetBytes(jsonString);
            SendDataCallback(data);
        }
    }
}
