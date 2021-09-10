using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                        watcher.Created += PictureSetContentChanged;
                        watcher.Deleted += PictureSetContentChanged;
                        watcher.Renamed += PictureSetContentChanged;
                        _pathToWatcher[PictureSetPath] = watcher;
                    }
                    else
                    {
                        FileSystemWatcher watcher = new FileSystemWatcher(PictureSetPath);
                        watcher.EnableRaisingEvents = true;
                        watcher.Created += PictureSetContentChanged;
                        watcher.Deleted += PictureSetContentChanged;
                        watcher.Renamed += PictureSetContentChanged;
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
                        watcher.Created += TemplateSetContentChanged;
                        watcher.Deleted += TemplateSetContentChanged;
                        watcher.Renamed += TemplateSetContentChanged;
                        _pathToWatcher[TemplatePath] = watcher;
                    }
                    else
                    {
                        FileSystemWatcher watcher = new FileSystemWatcher(TemplatePath);
                        watcher.EnableRaisingEvents = true;
                        watcher.Created += TemplateSetContentChanged;
                        watcher.Deleted += TemplateSetContentChanged;
                        watcher.Renamed += TemplateSetContentChanged;
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
                        watcher.Created += StdVersionContentChanged;
                        watcher.Deleted += StdVersionContentChanged;
                        watcher.Renamed += StdVersionContentChanged;
                        _pathToWatcher[StdVersionPath] = watcher;
                    }
                    else
                    {
                        FileSystemWatcher watcher = new FileSystemWatcher(StdVersionPath);
                        watcher.Created += StdVersionContentChanged;
                        watcher.Deleted += StdVersionContentChanged;
                        watcher.Renamed += StdVersionContentChanged;
                        watcher.EnableRaisingEvents = true;                      
                        _pathToWatcher.Add(StdVersionPath, watcher);
                    }
                    NotifyPropertyChanged("StdVersionPath");
                }
            }
        }
        public ProductConfig ProductConfig { get; set; } = null;
        
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
        public event PropertyChangedEventHandler FolderStateChanged;
        private void PictureSetContentChanged(object obj, FileSystemEventArgs e)
        {                    
            DirectoryInfo dirInfo = new DirectoryInfo(PictureSetPath);
            var allCsvs = dirInfo.GetFiles("*.csv");
            foreach (var csv in allCsvs)
            {
                //测试产生的结果csv
                if (csv.Name.Contains("_.csv"))
                    return;
            }
            FolderStateChanged?.Invoke(this, new PropertyChangedEventArgs("PictureSetPath"));

        }
        private void TemplateSetContentChanged(object obj, FileSystemEventArgs e)
        {

            FolderStateChanged?.Invoke(this, new PropertyChangedEventArgs("TemplatePath"));

        }
        private void StdVersionContentChanged(object obj, FileSystemEventArgs e)
        {          
            FolderStateChanged?.Invoke(this, new PropertyChangedEventArgs("StdVersionPath"));
        }
    }
}
