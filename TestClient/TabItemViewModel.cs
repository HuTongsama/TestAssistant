using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
                    SaveConfig(value.ToString());
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

        private bool _useServerConfig = false;
        public bool UseServerConfig 
        {
            get => _useServerConfig;
            set
            {
                if (value != _useServerConfig)
                {
                    _useServerConfig = value;
                    if (value != null)
                        SetConfigValue(GetConfigKey(Header, "UseServerConfig"), value.ToString());
                    EnableContent = !value;
                    NotifyPropertyChanged("UseServerConfig");
                }
            }
        }
        private bool _enableContent = true;
        public bool EnableContent
        {
            get => _enableContent;
            set
            {
                if (value != _enableContent)
                {
                    _enableContent = value;
                    NotifyPropertyChanged("EnableContent");
                }
            }
        }
        public ObservableCollection<ListItem> ServerConfigList { get; set; } = new ObservableCollection<ListItem>();
        private ListItem _selectedServerConfig = new ListItem();
        public ListItem SelectedServerConfig
        {
            get => _selectedServerConfig;
            set
            {
                if (value != _selectedServerConfig)
                {
                    _selectedServerConfig = value;
                    if (value != null)
                        SetConfigValue(GetConfigKey(Header, "ServerConfig"), value.ItemName);
                    NotifyPropertyChanged("SelectedServerConfig");
                }
            }
        }
        public ObservableCollection<ListItem> TemplateSetList { get; set; } = new ObservableCollection<ListItem>();
        private ListItem _selectedDefaultTemplate = new ListItem();
        public ListItem SelectedDefaultTemplate
        {
            get => _selectedDefaultTemplate;
            set 
            {
                if (value != _selectedDefaultTemplate)
                {
                    _selectedDefaultTemplate = value;
                    if (value != null)
                        SetConfigValue(GetConfigKey(Header, "DefaultTemplate"), value.ItemName);
                    NotifyPropertyChanged("SelectedDefaultTemplate");
                }
            }
        }
        public ObservableCollection<ListItem> PictureSetList { get; set; } = new ObservableCollection<ListItem>();
        public ObservableCollection<ListItem> SelectedPicList { get; set; } = new ObservableCollection<ListItem>();
        public ObservableCollection<ListItem> DecodeTypeList { get; set; } = new ObservableCollection<ListItem>();
        private ListItem _selectedDecodeType = new ListItem();
        public ListItem SelectedDecodeType
        {
            get => _selectedDecodeType;
            set
            {
                if (value != _selectedDecodeType)
                {
                    _selectedDecodeType = value;
                    if (value != null)
                        SetConfigValue(GetConfigKey(Header, "DecodeType"), value.ItemName);
                    NotifyPropertyChanged("SelectedDecodeType");
                }
            }
        }
        public ObservableCollection<ListItem> TestVersionList { get; set; } = new ObservableCollection<ListItem>();
        public ListItem SelectedTestVersion { get; set; } = new ListItem();
        public ObservableCollection<ListItem> StandardVersionList { get; set; } = new ObservableCollection<ListItem>();
        public ListItem SelectedStandardVersion { get; set; } = new ListItem();
        private Dictionary<string, List<ListItem>> _keyToPicSet = new Dictionary<string, List<ListItem>>();
        public Dictionary<string, List<ListItem>> KeyToPicSet
        {
            get => _keyToPicSet;
            set
            {
                if (value != _keyToPicSet)
                {
                    _keyToPicSet = value;
                    Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    string[] configKeys = config.AppSettings.Settings.AllKeys;
                    foreach (var item in _keyToPicSet)
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            CheckState tmpState = Enumerable.FirstOrDefault(_keyToCheckState,
                                delegate (CheckState state) { return state.StateKey == item.Key; });
                            if (tmpState != null)
                                _keyToCheckState.Remove(tmpState);
                            CheckState checkState = new CheckState(item.Key);
                            string configKey = GetConfigKey(Header, item.Key);
                            bool stateValue = false;
                            if (configKeys.Contains(configKey))
                            {
                                string configVal = config.AppSettings.Settings[configKey].Value;
                                if (configVal != string.Empty)
                                {
                                    stateValue = bool.Parse(configVal);
                                }
                            }
                            else
                            {
                                config.AppSettings.Settings.Add(configKey, "");
                                config.Save();
                            }
                            checkState.PropertyChanged += this.CheckStatePropertyChanged;
                            checkState.State = stateValue;
                            checkState.SaveConfig = delegate (string configVal) { SetConfigValue(configKey, configVal); };                          
                            _keyToCheckState.Add(checkState);
                        });

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
        private Dictionary<string, string> _picToTemplate = new Dictionary<string, string>();
        public Dictionary<string, string> PicToTemplate
        {
            get => _picToTemplate;
        }
        private string _x86Path = string.Empty;
        public string X86Path 
        {
            get
            {
                return _x86Path;
            }
            set
            {
                if (value != _x86Path)
                {
                    _x86Path = value;
                    SetConfigValue(GetConfigKey(Header, "X86DllPath"), value);
                    NotifyPropertyChanged("X86Path");
                }
            }
        }
        private string _x64Path = string.Empty;
        public string X64Path 
        {
            get
            {
                return _x64Path;
            }
            set
            {
                if (value != _x64Path)
                {
                    _x64Path = value;
                    SetConfigValue(GetConfigKey(Header, "X64DllPath"), value);
                    NotifyPropertyChanged("X64Path");
                }
            }
        }
        private string _extraSuffix = string.Empty;
        public string ExtraSuffix { get; set; }
        private void OnX86PathButtonClick(object obj)
        {
            string path = OnPathButtonClicked();
            if (path != null)
            { 
                X86Path = path;
            }
        }
        private RelayCommand _x86PathButtonCommand;
        public ICommand X86PathButtonCommand
        {
            get 
            {
                if (_x86PathButtonCommand == null)
                {
                    _x86PathButtonCommand = new RelayCommand(OnX86PathButtonClick, delegate { return true; });
                }
                return _x86PathButtonCommand;
            }
        }
        private void OnX64PathButtonClick(object obj)
        {
            string path = OnPathButtonClicked();
            if (path != null)
            {
                X64Path = path;
            }
        }
        private RelayCommand _x64PathButtonCommand;
        public ICommand X64PathButtonCommand
        {
            get 
            {
                if (_x64PathButtonCommand == null)
                {
                    _x64PathButtonCommand = new RelayCommand(OnX64PathButtonClick, delegate { return true; });
                }
                return _x64PathButtonCommand;
            }
        }
        private void OnAddStandardButtonClick(object obj)
        {
            if (SelectedTestVersion != null)
            {
                ListItem item = new ListItem(SelectedTestVersion.ItemName);               
                StandardVersionList.Add(item);
                TestVersionList.Remove(SelectedTestVersion);
            }
        }
        private RelayCommand _addStandardCommand;
        public ICommand AddStandardCommand
        {
            get
            {
                if (_addStandardCommand == null)
                {
                    _addStandardCommand = new RelayCommand(OnAddStandardButtonClick, delegate { return true; });
                }
                return _addStandardCommand;
            }
        }
        private void OnRemoveStandardButtonClick(object obj)
        {
            if (SelectedStandardVersion != null)
            {
                ListItem item = new ListItem(SelectedStandardVersion.ItemName);
                TestVersionList.Add(item);
                StandardVersionList.Remove(SelectedStandardVersion);
            }
        }
        private RelayCommand _removeStandardCommand;
        public ICommand RemoveStandardCommand
        {
            get 
            {
                if (_removeStandardCommand == null)                
                {
                    _removeStandardCommand = new RelayCommand(OnRemoveStandardButtonClick, delegate { return true; });
                }
                return _removeStandardCommand;
            }
        }
        public TabItemViewModel(string header,string x86Path = "",string x64Path = "",
            string useServerConfig = "",string standardVersionConfig = "")
        {
            Header = header;
            PictureSetList.CollectionChanged += PictureSetListChanged;
            SelectedPicList.CollectionChanged += SelectedSetListChanged;
            TemplateSetList.CollectionChanged += TemplateSetListChanged;
            DecodeTypeList.CollectionChanged += DecodeTypeListChanged;
            TestVersionList.CollectionChanged += VersionListChanged;
            StandardVersionList.CollectionChanged += StandardVersionListChanged;
            ServerConfigList.CollectionChanged += ServerConfigListChanged;

            X86Path = x86Path;
            X64Path = x64Path;
            if (useServerConfig != null && useServerConfig != string.Empty)            
            {
                UseServerConfig = bool.Parse(useServerConfig);
            }
            if (standardVersionConfig != null && standardVersionConfig != string.Empty)
            {
                var versionList = standardVersionConfig.Split(' ');
                foreach (var version in versionList)
                {
                    if (version == string.Empty)
                        continue;
                    ListItem item = new ListItem(version);
                    StandardVersionList.Add(item);
                }
            }
        }

        private void UpdateSelectedPicList(ListItem item, bool isAdd, EqualityComparer<ListItem> comparer)
        {
            if (isAdd)
            {
                if (Enumerable.Contains(SelectedPicList, item, comparer)
                 || !Enumerable.Contains(PictureSetList, item, comparer))
                {
                    return;
                }
                else
                {
                    item.UseCustomerRightClick = true;
                    item.OwnerCallBack = SelectedItemRightClicked;
                    SelectedPicList.Add(item);

                }
            }
            else
            {
                for (int i = 0; i < SelectedPicList.Count; ++i)
                {
                    if (SelectedPicList[i].ItemName == item.ItemName)
                    {                       
                        SelectedPicList.RemoveAt(i);
                        break;
                    }
                }

            }
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
                SameListItem isSameItem = new SameListItem();
                foreach (var item in curPicSet)
                {
                    UpdateSelectedPicList(item, checkState.State, isSameItem);

                }
            }
        }
        private void ListItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ListItem item = (ListItem)sender;
            if (item == null)
                return;
            switch (e.PropertyName)
            {
                case "IsSelected":
                    {
                        SameListItem isSameItem = new SameListItem();
                        ListItem addItem = new ListItem(item.ItemName);
                        UpdateSelectedPicList(addItem, item.IsSelected, isSameItem);
                    }
                    break;
                default:
                    break;
            }
        }
        private void PictureSetListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                foreach (var item in e.NewItems)
                {
                    ListItem listItem = (ListItem)item;
                    listItem.PropertyChanged += ListItemPropertyChanged;
                }
            }
            
        }
        private void SelectedSetListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    ListItem listItem = (ListItem)item;
                    foreach (var picListItem in PictureSetList)
                    {
                        if (picListItem.ItemName == listItem.ItemName)
                        {
                            picListItem.IsSelected = false;
                            break;
                        }
                    }
                }
            }
        }
        private void TemplateSetListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ListItem selectedItem = null;
            ReadConfig(GetConfigKey(Header, "DefaultTemplate"), e, out selectedItem);
            if (selectedItem != null)
                SelectedDefaultTemplate = selectedItem;
        }
        private void VersionListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                foreach (var item in e.NewItems)
                {
                    ListItem listItem = item as ListItem;
                    listItem.UseCustomerLeftClick = true;
                }
            }
        }
        private void DecodeTypeListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ListItem selectedItem = null;
            ReadConfig(GetConfigKey(Header, "DecodeType"), e,out selectedItem);
            if (selectedItem != null)
                SelectedDecodeType = selectedItem;
        }
        private void ServerConfigListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ListItem selectedItem = null;
            ReadConfig(GetConfigKey(Header, "ServerConfig"), e, out selectedItem);
            if (selectedItem != null)
                SelectedServerConfig = selectedItem;
        }
        private void StandardVersionListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            VersionListChanged(sender, e);
            string config = string.Empty;
            foreach (var stdVersion in StandardVersionList)
            {
                config += (stdVersion.ItemName + " ");
            }
            SetConfigValue(GetConfigKey(Header, "StandardVersion"), config);
        }
        private void ReadConfig(string configKey, NotifyCollectionChangedEventArgs e,out ListItem selectedItem)
        {
            selectedItem = null;
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string preSelected = config.AppSettings.Settings[configKey].Value;
                foreach (var item in e.NewItems)
                {
                    ListItem listItem = item as ListItem;
                    if (listItem.ItemName == preSelected)
                    {
                        selectedItem = listItem;
                        listItem.IsSelected = true;
                        break;
                    }
                }
            }
        }
        private void UpdateSelectedTemplate(string template)
        {           
            foreach (var listItem in SelectedPicList)
            {
                string itemName = listItem.ItemName;
                if (!listItem.IsSelected)
                    continue;
                if (_picToTemplate.ContainsKey(itemName))
                {
                    _picToTemplate[itemName] = template;
                }
                else
                {
                    _picToTemplate.Add(itemName, template);
                }
            }
        }
        private void SelectedItemRightClicked()
        {
            TemplateViewModel templateVm = new TemplateViewModel();
            templateVm.TemplateList = new ObservableCollection<ListItem>(TemplateSetList);
            templateVm.UpdateOwnerData = UpdateSelectedTemplate;
            TemplateWindowService templateWindowService = new TemplateWindowService();

            templateWindowService.OpenWindow(templateVm);
            //MessageBox.Show("right click");
        }
    }
}
