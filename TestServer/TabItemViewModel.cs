using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
                    NotifyPropertyChanged("X64ProgramPath");
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
        public TabItemViewModel(string header = "",
            string pictureSetPath = "",
            string templatePath = "",
            string x86ProgramPath = "",
            string x64ProgramPath = "")
        {
            Header = header;
            PictureSetPath = pictureSetPath;
            TemplatePath = templatePath;
            X86ProgramPath = x86ProgramPath;
            X64ProgramPath = x64ProgramPath;
            
        }
    }
}
