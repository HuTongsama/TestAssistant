using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GUIBase;
namespace TestClient
{
    class TemplateViewModel : ViewModelBase
    {       
        public Action CloseAction { get; set; } = delegate { };
        public Action<List<string>> UpdateOwnerData { get; set; } = delegate { };

        private void TemplateButtonClick(object obj)
        {
            List<string> selectedNames = new List<string>();
            foreach (var item in TemplateList)
            {
                if (item.IsSelected)
                {
                    selectedNames.Add(item.ItemName); 
                }
            }
            UpdateOwnerData(selectedNames);           
            CloseAction();
        }
        private RelayCommand _templateButtonClickCommand;
        public ICommand TemplateButtonClickCommand
        {
            get 
            {
                if (_templateButtonClickCommand == null)
                {
                    _templateButtonClickCommand = new RelayCommand(TemplateButtonClick, delegate { return true; });
                }
                return _templateButtonClickCommand;
            }
        }
        public ObservableCollection<ListItem> TemplateList { get; set; } = new ObservableCollection<ListItem>();

    }
}
