using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GUIBase;
namespace TestClient
{
    public class ListItem : ViewModelBase
    {
        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }
        private string _itemName = string.Empty;
        public string ItemName
        {
            get => _itemName;
            set
            {
                if (value != _itemName)
                {
                    _itemName = value;
                    NotifyPropertyChanged("ItemName");
                }
            }
        }

        public ListItem(string itemName = null, bool isSelected = false)
        {
            IsSelected = isSelected;
            ItemName = itemName;
        }
        public bool IsSameListItem(ListItem other)
        {
            if (_itemName == other.ItemName)
                return true;
            return false;
        }
    }
}
