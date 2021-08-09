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
        private bool _endCurrentTest;
        private ObservableCollection<TabItemViewModelBase> _tabItems;
        public ObservableCollection<TabItemViewModelBase> TabItems
        {
            get 
            {
                if (_tabItems == null)
                {
                    _tabItems = new ObservableCollection<TabItemViewModelBase>();
                }
                return _tabItems;
            }
        }
        private Dictionary<string, ServerData> _keyToData = new Dictionary<string, ServerData>();
        public MainWindowViewModel()
        {
            _listener = new Listener();
            _listener.GennerateMessage = GenerateMessage;
            _testProcess = new TestProcess();
            _endCurrentTest = false;
            _tabItems = new ObservableCollection<TabItemViewModelBase>();
            TabItemViewModelBase item = new TabItemViewModelBase("DBR");
            item.PropertyChanged += this.TabItemPropertyChanged;
            _tabItems.Add(item);
            item = new DLRTabItemViewModel("DLR");
            item.PropertyChanged += this.TabItemPropertyChanged;
            _tabItems.Add(item);
            item = new TabItemViewModelBase("DCN");
            item.PropertyChanged += this.TabItemPropertyChanged;
            _tabItems.Add(item);           
        }
        private void OnStartButtonClicked(object obj)
        {
            var thread1 = new Thread(_listener.StartListening);
            thread1.Start();
            //var thread2 = new Thread(StartTest);
            //thread2.Start();          
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
            Process myProcess = Process.Start("Test.exe");
            while (true)
            {
                if (myProcess.HasExited)
                {
                    MessageBox.Show(string.Format("succeed to exit {0}",myProcess.ExitCode));
                    break;
                }
                if (_endCurrentTest)
                {
                    myProcess.Kill();
                    myProcess.WaitForExit();
                    MessageBox.Show(string.Format("end current Test"));
                    break;
                }
                
            }
            while (true)
            {
                var testList = _listener.ItemList;
                //todo: update ui
                foreach (var item in testList)
                {
                    _endCurrentTest = false;
                    //todo: download dll from ftp
                    Process curProcess = Process.Start("Test.exe");
                    if (curProcess == null)
                        continue;
                    int exitCode = WaitProcess(curProcess);
                    if (exitCode == 0)
                    {
                        curProcess = Process.Start("Conclustion.exe");
                        if (curProcess == null)
                            continue;
                        exitCode = WaitProcess(curProcess);
                    }
                    if (exitCode == 0)
                    {
                        curProcess = Process.Start("GetPicture.exe");
                        if (curProcess == null)
                            continue;
                        exitCode = WaitProcess(curProcess);
                    }
                    //todo: _listener send message
                }
            }
        }
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
            TabItemViewModelBase item = (TabItemViewModelBase)sender;
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
                        serverData.KeyToPictureSet.Add("1D", new List<string>() { "1.csv", "2.csv" });
                        serverData.KeyToPictureSet.Add("2D", new List<string>() { "3.csv", "4.csv" });
                    }
                    break;
                case "DLR":
                    break;
                case "DCN":
                    break;
                default:
                    break;
            }
        }
        private string GenerateMessage()
        {
            if (_keyToData.Count == 0)
                return string.Empty;
            string jsonString = JsonSerializer.Serialize(_keyToData);
            return jsonString;
            
        }
    }
}
