using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GUIBase;
using Data;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestServer
{
    class MainWindowViewModel : ViewModelBase
    {
        private Listener _listener;
        private TestProcess _testProcess;
        private bool _endCurrentTest = false;
        private ObservableCollection<TabItemViewModel> _tabItems;
        public ObservableCollection<TabItemViewModel> TabItems
        {
            get 
            {
                if (_tabItems == null)
                {
                    _tabItems = new ObservableCollection<TabItemViewModel>();
                }
                return _tabItems;
            }
        }
        private Dictionary<string, ServerData> _keyToData = new Dictionary<string, ServerData>();
        public ObservableCollection<ClientData> TestWaitList { get; set; } = new ObservableCollection<ClientData>();        
        private readonly object _testWaitListLock = new object();
        public ObservableCollection<ClientData> CompareWaitList { get; set; } = new ObservableCollection<ClientData>();     
        private readonly object _compareWaitListLock = new object();
        private string _consoleMessage = string.Empty;
        public string ConsoleMessage
        {
            get => _consoleMessage;
            set 
            {
                if (value != _consoleMessage)
                {
                    _consoleMessage = value;
                    NotifyPropertyChanged("ConsoleMessage");
                }
            }
        }

        public MainWindowViewModel()
        {
            _listener = new Listener();
            _listener.GennerateMessage = GenerateMessage;
            _listener.TestListCallback = UpdateTestListCallback;
            _listener.CompareListCallback = UpdateCompareListCallback;

            _testProcess = new TestProcess();
            _endCurrentTest = false;
            _tabItems = new ObservableCollection<TabItemViewModel>();
            string productName = ProductType.DBR.ToString();
            TabItemViewModel item = new TabItemViewModel(productName);
            item.PropertyChanged += this.TabItemPropertyChanged;
            _tabItems.Add(item);
            productName = ProductType.DLR.ToString();
            item = new TabItemViewModel(productName);
            item.PropertyChanged += this.TabItemPropertyChanged;
            _tabItems.Add(item);
            productName = ProductType.DCN.ToString();
            item = new TabItemViewModel(productName);
            item.PropertyChanged += this.TabItemPropertyChanged;
            _tabItems.Add(item);
          
        }
        private void OnStartButtonClicked(object obj)
        {          
            var listeningThread = new Thread(_listener.StartListening);
            listeningThread.IsBackground = true;
            listeningThread.Start();

            var receiveThread = new Thread(_listener.ReceiveMessage);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            var testThread = new Thread(StartTest);
            testThread.IsBackground = true;
            testThread.Start();
        }
        private RelayCommand _startButtonCommand;
        public ICommand StartButtonCommand
        {
            get 
            {
                if (_startButtonCommand == null)
                {
                    _startButtonCommand = new RelayCommand(OnStartButtonClicked, delegate{ return true; });
                }
                return _startButtonCommand;
            }
        }
        private void OnEndButtonClicked(object obj)
        {
            _endCurrentTest = true;
        }
        private RelayCommand _endButtonCommand;
        public ICommand EndButtonCommand
        {
            get 
            {
                if (_endButtonCommand == null)
                {
                    _endButtonCommand = new RelayCommand(OnEndButtonClicked, delegate { return true; });
                }
                return _endButtonCommand;
            }
        }
        private void StartTest()
        {
            //Process myProcess = Process.Start("Test.exe");
            //while (true)
            //{
            //    if (myProcess.HasExited)
            //    {
            //        MessageBox.Show(string.Format("succeed to exit {0}",myProcess.ExitCode));
            //        break;
            //    }
            //    if (_endCurrentTest)
            //    {
            //        myProcess.Kill();
            //        myProcess.WaitForExit();
            //        MessageBox.Show(string.Format("end current Test"));
            //        break;
            //    }
                
            //}
            while (true)
            {
                if (TestWaitList.Count > 0)
                {
                    ClientData data = null;
                    lock (_testWaitListLock)
                    {
                        data = TestWaitList[0];
                        App.Current.Dispatcher.Invoke((Action)delegate
                       {
                           TestWaitList.RemoveAt(0);
                       });                      
                    }
                    if (data != null)
                    {
                        string logInfo = ConsoleMessage + "\nStart test " + data.VersionInfo + "\n";
                        ConsoleMessage = logInfo;
                        while (true)
                        {
                            if (_endCurrentTest)
                            {
                                //kill process
                                break;
                            }
                        }
                        if (!_endCurrentTest)
                        {
                            if (data.StandardVersion != string.Empty)
                            {
                                data.OperateType = OperateType.Compare;
                                lock (_compareWaitListLock)
                                {
                                    CompareWaitList.Add(data);
                                }
                            }
                        }
                        else
                        {
                            _endCurrentTest = false;
                        }
                        logInfo = ConsoleMessage + "\nTest " + data.VersionInfo + " finished" + "\n";
                        ConsoleMessage = logInfo;
                        CallCompare();
                        CallGetPicture();
                    }
                }
            }
            //while (true)
            //{
            //    var testList = _listener.ItemList;
            //    //todo: update ui
            //    foreach (var item in testList)
            //    {
            //        _endCurrentTest = false;
            //        //todo: download dll from ftp
            //        Process curProcess = Process.Start("Test.exe");
            //        if (curProcess == null)
            //            continue;
            //        int exitCode = WaitProcess(curProcess);
            //        if (exitCode == 0)
            //        {
            //            curProcess = Process.Start("Conclustion.exe");
            //            if (curProcess == null)
            //                continue;
            //            exitCode = WaitProcess(curProcess);
            //        }
            //        if (exitCode == 0)
            //        {
            //            curProcess = Process.Start("GetPicture.exe");
            //            if (curProcess == null)
            //                continue;
            //            exitCode = WaitProcess(curProcess);
            //        }
            //        //todo: _listener send message
            //    }
            //}
        }
        private void CallCompare()
        {
            lock (_compareWaitListLock)
            {
                foreach (var data in CompareWaitList)
                {
                    //to compare
                }
                App.Current.Dispatcher.Invoke((Action)delegate
               {
                   CompareWaitList.Clear();
               });
                
            }
        }
        private void CallGetPicture()
        { }
        private int WaitProcess(Process process)
        {
            while (true)
            {
                if (_endCurrentTest)
                {
                    process.Kill();
                    process.WaitForExit();
                }
                if (process.HasExited)
                {
                    return process.ExitCode;
                }
            }
        }
        private void TabItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TabItemViewModel item = (TabItemViewModel)sender;
            if (item == null)
                return;
            string key = item.Header;
            ServerData serverData;
            if (_keyToData.ContainsKey(key))
            {
                serverData = _keyToData[key];
            }
            else
            {
                serverData = new ServerData();
                _keyToData.Add(key, serverData);
            }
            serverData.DataType = "ServerData";
            switch (e.PropertyName)
            {
                case "PictureSetPath":
                    {
                        List<string> fileNames = CommonFuction.GetAllFiles(item.PictureSetPath, "*.csv");
                        serverData.PictureSetList = fileNames;
                    }
                    break;
                case "TemplatePath":
                    {
                        List<string> fileNames = CommonFuction.GetAllFiles(item.TemplatePath, "*.json");
                        serverData.TemplateList = fileNames;
                    }
                    break;
                default:
                    break;
            }

            switch (key)
            {
                case "DBR":
                    {
                        if (!serverData.KeyToPictureSet.ContainsKey("1D"))
                            serverData.KeyToPictureSet.Add("1D", new List<string>() { "1.csv", "2.csv" });
                        if (!serverData.KeyToPictureSet.ContainsKey("2D"))
                            serverData.KeyToPictureSet.Add("2D", new List<string>() { "3.csv", "4.csv" });
                        if (Enumerable.Count(serverData.DecodeTypeList,(decodeType)=> decodeType == TestDataType.File.ToString()) == 0)
                        {
                            serverData.DecodeTypeList.Add(TestDataType.File.ToString());
                        }
                        if (Enumerable.Count(serverData.DecodeTypeList, (decodeType) => decodeType == TestDataType.Buffer.ToString()) == 0)
                        {
                            serverData.DecodeTypeList.Add(TestDataType.Buffer.ToString());
                        }
                            
                    }
                    break;
                case "DLR":
                    break;
                case "DCN":
                    break;
                default:
                    break;
            }
            string message = GenerateMessage();
            byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            _listener.SendToAllClients(data);
        }
        private string GenerateMessage()
        {
            if (_keyToData.Count == 0)
                return string.Empty;
            string jsonString = JsonSerializer.Serialize(_keyToData);
            return jsonString;
            
        }
        private void UpdateTestListCallback(List<ClientData> dataList)
        {
            if (dataList == null || dataList.Count == 0)
                return;
            lock (_testWaitListLock)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    foreach (var data in dataList)
                    {
                        TestWaitList.Add(data);
                    }
                });
                
            }
        }
        private void UpdateCompareListCallback(List<ClientData> dataList)
        {
            if (dataList == null || dataList.Count == 0)
                return;
            lock (_compareWaitListLock)
            {
                foreach (var data in dataList)
                {
                    CompareWaitList.Add(data);
                }
            }
        }
    }
}
