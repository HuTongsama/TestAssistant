using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GUIBase;
namespace TestClient
{
    public class CheckState : ViewModelBase
    {
        private string _stateKey = string.Empty;
        public string StateKey
        {
            get => _stateKey;
            set
            {
                if (value != string.Empty)
                {
                    _stateKey = value;
                    NotifyPropertyChanged("StateKey");
                }
            }
        }
        private bool _state = false;
        public bool State
        {
            get => _state;
            set 
            {
                if (value != _state)
                {
                    _state = value;
                    NotifyPropertyChanged("State");
                }
            }
        }

        public CheckState(string key = "", bool state = false)
        {
            StateKey = key;
            State = state;
        }
    }
    public class TabItemViewModel : ViewModelBase
    {
        private string _header;
        public string Header
        {
            get => _header;
            set
            {
                if (value != _header)
                {
                    _header = value;
                    NotifyPropertyChanged("Header");
                }
            }
        }

        private List<ListItem> _pictureSetList = new List<ListItem>();
        public List<ListItem> PictureSetList
        {
            get => _pictureSetList;
            set
            {
                if (value != _pictureSetList)
                {
                    _pictureSetList = value;
                    NotifyPropertyChanged("PictureSetList");
                }
            }
        }
        private List<ListItem> _templateSetList = new List<ListItem>();
        public List<ListItem> TemplateSetList
        {
            get => _templateSetList;
            set
            {
                if (value != _templateSetList)
                {
                    _templateSetList = value;
                    NotifyPropertyChanged("TemplateSetList");
                }
            }
        }
        private List<ListItem> _selectedPicList = new List<ListItem>();
        public List<ListItem> SelectedPicList
        {
            get => _selectedPicList;
            set
            {
                if (value != _selectedPicList)
                {
                    _selectedPicList = value;
                    NotifyPropertyChanged("SelectedPicList");
                }
            }
        }
        private Dictionary<string, List<ListItem>> _keyToPicSet = new Dictionary<string, List<ListItem>>();
        public Dictionary<string, List<ListItem>> KeyToPicSet
        {
            get => _keyToPicSet;
            set
            {
                if (value != _keyToPicSet)
                {
                    _keyToPicSet = value;
                    foreach (var item in _keyToPicSet)
                    {
                        CheckState checkState = new CheckState(item.Key);
                        checkState.PropertyChanged += this.CheckStatePropertyChanged;
                        _keyToCheckState.Add(checkState);
                    }
                    NotifyPropertyChanged("KeyToPicSet");
                }
            }
        }
        private ObservableCollection<CheckState> _keyToCheckState = new ObservableCollection<CheckState>();
        public ObservableCollection<CheckState> KeyToCheckState
        {
            get
            {
                if (_keyToCheckState == null)
                {
                    _keyToCheckState = new ObservableCollection<CheckState>();
                }
                return _keyToCheckState;
            }
        }

        public TabItemViewModel(string header)
        {
            Header = header;
            //KeyToCheckState.Add(new CheckState("1",true));
            //KeyToCheckState.Add(new CheckState("2", true));
            //KeyToCheckState.Add(new CheckState("3", true));
        }
        private void CheckStatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CheckState checkState = (CheckState)sender;
            if (sender == null)
                return;
            string key = checkState.StateKey;
            if (_keyToPicSet.ContainsKey(key))
            {
                List<ListItem> curPicSet = _keyToPicSet[key];
                foreach (var item in curPicSet)
                {
                    if (SelectedPicList.Exists(item.IsSameListItem)
                        || !PictureSetList.Exists(item.IsSameListItem))
                    {
                        continue;
                    }
                    else
                    {
                        SelectedPicList.Add(item);
                    }
                }
            }
        }
    }
}
