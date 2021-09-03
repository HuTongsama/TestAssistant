﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GUIBase;
using System.Text.Json;
using System.Text.Json.Serialization;
using Data;
using System.Threading;
using System.Windows.Input;
using System.IO;
using FTP;
using System.Windows;
using System.Configuration;

namespace TestClient
{

    public class MainWindowViewModel : ViewModelBase
    {
        private Dictionary<string, TabItemViewModel> _tabItems = new Dictionary<string, TabItemViewModel>();
        public Dictionary<string, TabItemViewModel> TabItems
        {
            get => _tabItems;
        }
        private TabItemViewModel _selectedItem = null;
        public TabItemViewModel SelectedItem 
        {
            get => _selectedItem;
            set
            {
                if (value != _selectedItem)
                {
                    _selectedItem = value;
                    SetConfigValue("selectedProduct", value.Header);
                    NotifyPropertyChanged("SelectedItem");
                }
            }
        }
        private Client _client = new Client();
        private ClientData _clientData = null;
        private bool _needSendData = false;
        private string _ftpCachePath = string.Empty;

        private void OnPerformanceButtonClick(object obj)
        {
            ClientData data = GenerateClientData();
            ButtonClickAction(data, OperateType.Performance);          
        }
        private RelayCommand _performanceButtonCommand;
        public ICommand PerformanceButtonCommand
        {
            get 
            {
                if (_performanceButtonCommand == null)
                {
                    _performanceButtonCommand = new RelayCommand(OnPerformanceButtonClick,delegate { return true; });
                }
                return _performanceButtonCommand;
            }
        }
        private void OnStabilityButtonClick(object obj)
        {
            ClientData data = GenerateClientData();
            ButtonClickAction(data, OperateType.Stability);

        }
        private RelayCommand _stabilityButtonCommand;
        public ICommand StabilityButtonCommand
        {
            get 
            {
                if (_stabilityButtonCommand == null)
                {
                    _stabilityButtonCommand = new RelayCommand(OnStabilityButtonClick, delegate { return true; });
                }
                return _stabilityButtonCommand;
            }
        }
        private void OnCompareButtonClick(object obj)
        {
            ClientData data = GenerateClientData();
            ButtonClickAction(data, OperateType.Compare);
        }
        private RelayCommand _compareButtonCommand;
        public ICommand CompareButtonCommand
        {
            get
            {
                if (_compareButtonCommand == null)
                {
                    _compareButtonCommand = new RelayCommand(OnCompareButtonClick, delegate { return true; });
                }
                return _compareButtonCommand;
            }
        }
        public ObservableCollection<ListItem> ServerTestList { get; set; } = new ObservableCollection<ListItem>();
        public ObservableCollection<ListItem> ServerCompareList { get; set; } = new ObservableCollection<ListItem>();
        public MainWindowViewModel()
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string selectedProduct = config.AppSettings.Settings["selectedProduct"].Value;
            _ftpCachePath = "Testing/Cache";
            //FTPHelper ftpHelper = new FTPHelper();
            //ftpHelper.MakeDirectory(_ftpCachePath);
            string productName = ProductType.DBR.ToString();
            TabItemViewModel item = new TabItemViewModel(
                productName,
                config.AppSettings.Settings["dbrX86DllPath"].Value,
                config.AppSettings.Settings["dbrX64DllPath"].Value,
                config.AppSettings.Settings["dbrUseServerConfig"].Value,
                config.AppSettings.Settings["dbrStandardVersion"].Value);
            _tabItems.Add(productName, item);       
            SelectedItem = item;
            
            productName = ProductType.DLR.ToString();
            item = new TabItemViewModel(
                productName,
                config.AppSettings.Settings["dlrX86DllPath"].Value,
                config.AppSettings.Settings["dlrX64DllPath"].Value,
                config.AppSettings.Settings["dlrUseServerConfig"].Value,
                config.AppSettings.Settings["dlrStandardVersion"].Value);
            if (selectedProduct != string.Empty && selectedProduct == productName)
                SelectedItem = item;
            _tabItems.Add(productName, item);

            productName = ProductType.DCN.ToString();
            item = new TabItemViewModel(
                productName,
                config.AppSettings.Settings["dcnX86DllPath"].Value,
                config.AppSettings.Settings["dcnX64DllPath"].Value,
                config.AppSettings.Settings["dcnUseServerConfig"].Value,
                config.AppSettings.Settings["dcnStandardVersion"].Value);
            if (selectedProduct != string.Empty && selectedProduct == productName)
                SelectedItem = item;
            _tabItems.Add(productName, item);

            Thread clientThread = new Thread(ClientThreadFunc);
            clientThread.IsBackground = true;
            clientThread.Start();

        }

        private void UpdateTabCollection(List<string> srcList, ObservableCollection<ListItem> listItems,
            SameListItem sameListItem)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                foreach (var picSet in srcList)
                {
                    ListItem item = new ListItem(picSet);
                    if (Enumerable.Contains(listItems, item, sameListItem))
                        continue;
                    listItems.Add(item);
                }
            });
        }
        private void ClientThreadFunc()
        {
            _client.Connect();
            while (true)
            {
                if (_client == null)
                    continue;
                if (!_client.IsConnected())
                {
                    MessageBox.Show("服务器断开连接");
                    break;
                }
                SendClientData();
                string receiveData = _client.ReceiveData();
                try
                {
                    var serverData = JsonSerializer.Deserialize<ServerData>(receiveData);
                    SameListItem sameListItem = new SameListItem();
                    if (serverData.ProductType != string.Empty)
                    {                     
                        if (_tabItems.ContainsKey(serverData.ProductType))
                        {
                            TabItemViewModel curTab = _tabItems[serverData.ProductType];                          
                            if (serverData.PictureSetChanged)
                            {
                                UpdateTabCollection(serverData.PictureSetList, curTab.PictureSetList, sameListItem);
                            }
                            if (serverData.TemplateSetChanged)
                            {
                                UpdateTabCollection(serverData.TemplateList, curTab.TemplateSetList, sameListItem);
                            }
                            if (serverData.DecodeTypeList.Count > 0)
                            {
                                UpdateTabCollection(serverData.DecodeTypeList, curTab.DecodeTypeList, sameListItem);
                            }
                            if (serverData.KeyToPictureSet.Count > 0)
                            {
                                Dictionary<string, List<ListItem>> tmpKeyToPicSet = new Dictionary<string, List<ListItem>>();
                                foreach (var keyPair in serverData.KeyToPictureSet)
                                {
                                    string tmpKey = keyPair.Key;
                                    List<String> tmpValue = keyPair.Value;
                                    List<ListItem> tmpList = new List<ListItem>();
                                    foreach (var str in tmpValue)
                                    {
                                        tmpList.Add(new ListItem(str));
                                    }
                                    tmpKeyToPicSet.Add(tmpKey, tmpList);
                                }
                                curTab.KeyToPicSet = tmpKeyToPicSet;
                            }
                            if (serverData.StdVersionListChanged)
                            {
                                UpdateTabCollection(serverData.StdVersionList, curTab.StandardVersionList, sameListItem);
                            }                          
                            if (serverData.FinishedVersionInfo != string.Empty)
                            {
                                App.Current.Dispatcher.Invoke((Action)
                                    delegate
                                    {
                                        ListItem listItem = new ListItem(serverData.FinishedVersionInfo);
                                        curTab.TestVersionList.Add(listItem);
                                    });
                            }
                            if (serverData.ConfigListChanged)
                            {
                                UpdateTabCollection(serverData.ConfigList, curTab.ServerConfigList, sameListItem);
                            }
                        }                      
                    }
                    if (serverData.TestListChanged)
                    {
                        App.Current.Dispatcher.Invoke((Action)
                                delegate
                                {
                                    ServerTestList.Clear();
                                });
                        UpdateTabCollection(serverData.TestWaitingList, ServerTestList, sameListItem);
                    }
                    if (serverData.CompareListChanged)
                    {
                        App.Current.Dispatcher.Invoke((Action)
                            delegate
                            {
                                ServerCompareList.Clear();
                            });
                        UpdateTabCollection(serverData.CompareWaitingList, ServerCompareList, sameListItem);
                    }
                    if (serverData.Message != string.Empty)
                    {
                        LogMessage(serverData.Message);
                    }
                }
                catch (Exception)
                {
                    continue;
                }

            }
        }
        private ClientData GenerateClientData()
        {
            if (SelectedItem == null)
                return null;
            ClientData data = new ClientData();
            data.ProductType = (ProductType)Enum.Parse(typeof(ProductType), SelectedItem.Header);
            if (SelectedItem.UseServerConfig)
            {
                data.UseServerConfig = true;
                foreach (var serverConfig in SelectedItem.ServerConfigList)
                {
                    if (serverConfig.IsSelected)
                    {
                        data.ServerConfig = serverConfig.ItemName;
                        break;
                    }
                }
            }
            else
            {
                foreach (var listItem in SelectedItem.SelectedPicList)
                {
                    data.ImageCsvList.Add(listItem.ItemName);
                }
                foreach (var template in SelectedItem.TemplateSetList)
                {
                    if (template.IsSelected)
                    {
                        data.DefaultTemplate = template.ItemName;
                        break;
                    }
                }
                foreach (var keyPair in SelectedItem.PicToTemplate)
                {
                    string template = keyPair.Value;
                    string csv = keyPair.Key;
                    if (!data.TemplateToCsvSet.ContainsKey(template))
                    {
                        data.TemplateToCsvSet.Add(template, new List<string>());

                    }
                    data.TemplateToCsvSet[template].Add(csv);
                }
            }
            foreach (var decodeType in SelectedItem.DecodeTypeList)
            {
                if (decodeType.IsSelected)
                {
                    data.TestDataType = (TestDataType)Enum.Parse(typeof(TestDataType), decodeType.ItemName);
                    break;
                }
            }
            
            if (SelectedItem.SelectedTestVersion != null)
            {
                data.TestVersion = SelectedItem.SelectedTestVersion.ItemName;
            }
            if (SelectedItem.SelectedStandardVersion != null)
            {
                data.StandardVersion = SelectedItem.SelectedStandardVersion.ItemName;
            }
            data.UserName = Environment.UserName;
            data.UploadTime = DateTime.Now;
            data.VersionInfo = "_v_" + data.UploadTime.ToString("yyMMdd_HHmmss") + "_" + data.UserName;
            if (SelectedItem.ExtraSuffix != null && SelectedItem.ExtraSuffix != string.Empty)
                data.VersionInfo += "_" + SelectedItem.ExtraSuffix;
            return data;
        }
        private void SendClientData()
        {
            if (_needSendData)
            {
                _needSendData = false;
                if (_clientData == null)
                    return;
                //todo: up to ftp
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                //options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                string jsonString = JsonSerializer.Serialize(_clientData,options);             
                byte[] data = System.Text.Encoding.ASCII.GetBytes(jsonString);
                _client.SendData(data);
            }
        }
        private bool UploadDll(string ftpPath)
        {
            try
            {
                List<string> dllPathList = new List<string>();
                if (SelectedItem.X64Path != string.Empty)
                    dllPathList.Add(SelectedItem.X64Path);
                if (SelectedItem.X86Path != string.Empty)
                    dllPathList.Add(SelectedItem.X86Path);
                FTPHelper ftpHelper = new FTPHelper();
                if (ftpHelper.MakeDirectory(ftpPath))
                {
                    foreach (var path in dllPathList)
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(SelectedItem.X64Path);
                        FileInfo[] files = dirInfo.GetFiles("*.dll", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            ftpHelper.Upload(file, ftpPath);
                        }
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }
        private bool DataCheck(ClientData data,out string errMessage)
        {
            errMessage = string.Empty;
            bool isValid = true;
            if (data.OperateType != OperateType.Compare)
            {
                if (!data.UseServerConfig)
                {
                    if (data.DefaultTemplate == null || data.DefaultTemplate == string.Empty)
                    {
                        isValid = false;
                        errMessage += "invalid DefaultTemplate\n";
                    }
                    if (data.TestDataType == TestDataType.Empty)
                    {
                        isValid = false;
                        errMessage += "invalid TestDataType\n";
                    }
                }
                else
                {
                    if (data.ServerConfig == null || data.ServerConfig == string.Empty)
                    {
                        isValid = false;
                        errMessage += "invalid ServerConfig\n";
                    }
                }
                if (SelectedItem.X64Path == null ||
                    SelectedItem.X64Path == string.Empty
                    || !System.IO.Directory.Exists(SelectedItem.X64Path))
                {
                    isValid = false;
                    errMessage += "invalid X64Path\n";
                }
                if (SelectedItem.X86Path == null||
                    SelectedItem.X86Path == string.Empty
                    || !System.IO.Directory.Exists(SelectedItem.X86Path))
                {
                    isValid = false;
                    errMessage += "invalid X86Path\n";
                }
            }
            else
            {
                if (data.TestVersion == null ||
                    data.TestVersion == string.Empty)
                {
                    isValid = false;
                    errMessage += "invalid TestVersion\n";
                }
                if (data.StandardVersion == null ||
                    data.StandardVersion == string.Empty)
                {
                    isValid = false;
                    errMessage += "invalid StandardVersion\n";
                }
            }
            return isValid;
        }
        private void ButtonClickAction(ClientData data, OperateType operateType)
        {
            if (data != null)
            {
                data.OperateType = operateType;
                string errMessage = string.Empty;
                if (DataCheck(data, out errMessage))
                {
                    string ftpCachePath = _ftpCachePath + "/" + data.VersionInfo;
                    data.FtpCachePath = ftpCachePath;
                    if (data.OperateType == OperateType.Compare)
                    {
                        _needSendData = true;
                        _clientData = data;
                    }
                    else
                    {
                        if (UploadDll(ftpCachePath))
                        {
                            _needSendData = true;
                            _clientData = data;
                        }
                    }
                }
                else
                {
                    MessageBox.Show(errMessage);
                }
            }
        }
    }
}
