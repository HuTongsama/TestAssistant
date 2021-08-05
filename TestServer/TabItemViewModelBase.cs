using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GUIBase;
namespace TestServer
{
    public class TabItemViewModelBase : ViewModelBase
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
        public TabItemViewModelBase(string header = "",
            string pictureSetPath = "",
            string templatePath = "")
        {
            Header = header;
            PictureSetPath = pictureSetPath;
            TemplatePath = templatePath;
            
        }
    }
}
