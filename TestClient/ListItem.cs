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
    }

    class SameListItem : EqualityComparer<ListItem>
    {
        public override bool Equals(ListItem x, ListItem y)
        {
            if (x.ItemName == y.ItemName)
                return true;
            return false;
        }

        public override int GetHashCode(ListItem obj)
        {
            return obj.ItemName.Length;
        }
    }
}
