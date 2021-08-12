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
        public bool IsModal { get; } = true;
        public int SelectedId { get; set; } = -1;
        public Action CloseAction { get; set; } = delegate { };
        public Action<string> UpdateOwnerData { get; set; } = delegate { };

        private void TemplateButtonClick(object obj)
        {
            if (SelectedId > 0 && SelectedId < TemplateList.Count)
            { 
                UpdateOwnerData(TemplateList[SelectedId].ItemName); 
            }
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
