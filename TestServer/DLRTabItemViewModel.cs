using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GUIBase;

namespace TestServer
{
    class DLRTabItemViewModel : TabItemViewModelBase
    {
        private string _modelPath = string.Empty;
        public string ModelPath
        {
            get 
            {
                return _modelPath;
            }
            set 
            {
                if (value != _modelPath)
                {
                    _modelPath = value;
                    NotifyPropertyChanged("ModelPath");
                }
            }
        }
        private void OnModelButtonClicked(object obj)
        {
            string path = OnPathButtonClicked();
            if (path != null)
            {
                ModelPath = path;
            }
        }
        private RelayCommand _modelButtonClicked;
        public ICommand ModelButtonClicked
        {
            get 
            {
                if (_modelButtonClicked == null)
                {
                    _modelButtonClicked = new RelayCommand(OnModelButtonClicked, delegate { return true; });
                }
                return _modelButtonClicked;
            }
        }

        public DLRTabItemViewModel(string header = "",
            string pictureSetPath = "",
            string templatePath = "",
            string modelPath = "") : base(header,pictureSetPath,templatePath)
        {
            ModelPath = modelPath;
        }
    }
}
