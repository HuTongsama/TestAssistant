using System;
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

namespace TestClient
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Dictionary<string, TabItemViewModel> _tabItems = new Dictionary<string, TabItemViewModel>();
        public Dictionary<string, TabItemViewModel> TabItems
        {
            get => _tabItems;
        }
       
        private Client _client = new Client();
        public MainWindowViewModel()
        {
            TabItemViewModel item = new TabItemViewModel("DBR");
            _tabItems.Add("DBR", item);
            item = new TabItemViewModel("DLR");
            _tabItems.Add("DLR", item);
            item = new TabItemViewModel("DCN");
            _tabItems.Add("DCN", item);
            Thread clientThread = new Thread(ClientThreadFunc);
            clientThread.Start();

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
                string receiveData = _client.ReceiveData();
                var jsonValue = JsonSerializer.Deserialize<Dictionary<string,ServerData>>(receiveData);
                foreach (var tabData in jsonValue)
                {
                    string key = tabData.Key;
                    ServerData serverData = tabData.Value;
                    if (serverData.DataType == "Message")
                    {

                    }
                    else if(serverData.DataType == "ServerData")
                    {
                        if (_tabItems.ContainsKey(key))
                        {
                            TabItemViewModel curTab = _tabItems[key];
                            App.Current.Dispatcher.Invoke((Action)delegate
                            {
                                foreach (var picSet in serverData.PictureSetList)
                                {
                                    curTab.PictureSetList.Add(new ListItem(picSet));
                                }
                            });
                            
                            foreach (var template in serverData.TemplateList)
                            {
                                curTab.TemplateSetList.Add(new ListItem(template));
                            }
 
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
        }
    }
}
