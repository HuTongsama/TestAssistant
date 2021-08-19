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

namespace TestClient
{

    public class MainWindowViewModel : ViewModelBase
    {
        private Dictionary<string, TabItemViewModel> _tabItems = new Dictionary<string, TabItemViewModel>();
        public Dictionary<string, TabItemViewModel> TabItems
        {
            get => _tabItems;
        }
        public TabItemViewModel SelectedItem { get; set; } = null;
        private Client _client = new Client();
        private ClientData _clientData = null;
        private bool _needSendData = false;

        private void OnPerformanceButtonClick(object obj)
        {
            ClientData data = GenerateClientData();
            if (data != null)
            { 
                data.OperateType = OperateType.Performance;
                _needSendData = true;
                _clientData = data;
            }
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
            if (data != null)
            {
                data.OperateType = OperateType.Stability;
                _needSendData = true;
                _clientData = data;
            }
            
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
            if (data != null)
            {
                data.OperateType = OperateType.Compare;
                _needSendData = true;
            }
        }
        public MainWindowViewModel()
        {
            string productName = ProductType.DBR.ToString();
            TabItemViewModel item = new TabItemViewModel(productName);
            _tabItems.Add(productName, item);
            SelectedItem = item;
            
            productName = ProductType.DLR.ToString();
            item = new TabItemViewModel(productName);
            _tabItems.Add(productName, item);

            productName = ProductType.DCN.ToString();
            item = new TabItemViewModel(productName);
            _tabItems.Add(productName, item);
            Thread clientThread = new Thread(ClientThreadFunc);
            clientThread.IsBackground = true;
            clientThread.Start();

        }

        private void UpdateTabCollection(List<string> srcList, ObservableCollection<ListItem> listItems, SameListItem sameListItem)
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
                    continue;
                SendClientData();
                string receiveData = _client.ReceiveData();
                try
                {
                    var jsonValue = JsonSerializer.Deserialize<Dictionary<string, ServerData>>(receiveData);
                    foreach (var tabData in jsonValue)
                    {
                        string key = tabData.Key;
                        ServerData serverData = tabData.Value;
                        if (serverData.DataType == "Message")
                        {

                        }
                        else if (serverData.DataType == "ServerData")
                        {
                            if (_tabItems.ContainsKey(key))
                            {
                                TabItemViewModel curTab = _tabItems[key];
                                SameListItem sameListItem = new SameListItem();
                               
                                UpdateTabCollection(serverData.PictureSetList, curTab.PictureSetList, sameListItem);
                                UpdateTabCollection(serverData.TemplateList, curTab.TemplateSetList, sameListItem);
                                UpdateTabCollection(serverData.DecodeTypeList, curTab.DecodeTypeList, sameListItem);
                               
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
                        }
                        //TabItems.
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
            foreach (var listItem in SelectedItem.SelectedPicList)
            {
                data.ImageCsvList.Add(listItem.ItemName);
            }
            
            foreach (var keyPair in SelectedItem.PicToTemplate)
            {
                string template = keyPair.Value;
                string csv = keyPair.Key;
                if (!data.TemplateToCsvSet.ContainsKey(template))
                {
                    data.TemplateToCsvSet.Add(template,new List<string>());
                    
                }
                data.TemplateToCsvSet[template].Add(csv);
            }

            foreach (var decodeType in SelectedItem.DecodeTypeList)
            {
                if (decodeType.IsSelected)
                {
                    data.TestDataType = (TestDataType)Enum.Parse(typeof(TestDataType), decodeType.ItemName);
                    break;
                }
            }
            foreach (var template in SelectedItem.TemplateSetList)
            {
                if (template.IsSelected)
                {
                    data.DefaultTemplate = template.ItemName;
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
                string jsonString = JsonSerializer.Serialize(_clientData);
                byte[] data = System.Text.Encoding.ASCII.GetBytes(jsonString);
                _client.SendData(data);
            }
        }
    }
}
