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
        public TabItemViewModel SelectedItem { get; set; } = null;
        private Client _client = new Client();
        public MainWindowViewModel()
        {
            TabItemViewModel item = new TabItemViewModel("DBR");
            _tabItems.Add("DBR", item);
            SelectedItem = item;
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
                                App.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    foreach (var picSet in serverData.PictureSetList)
                                    {
                                        ListItem item = new ListItem(picSet);
                                        if (Enumerable.Contains(curTab.PictureSetList, item, sameListItem))
                                            continue;
                                        curTab.PictureSetList.Add(item);
                                    }
                                });

                                foreach (var template in serverData.TemplateList)
                                {
                                    ListItem item = new ListItem(template);
                                    if (Enumerable.Contains(curTab.TemplateSetList, item, sameListItem))
                                        continue;
                                    curTab.TemplateSetList.Add(item);
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
                catch (Exception)
                {
                    continue;
                }

            }
        }
    }
}
