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
using System.IO;
using FTP;
using System.Configuration;

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

        private string _resultPath = string.Empty;
        public string ResultPath
        {
            get => _resultPath;
            set
            {
                if (value != _resultPath)
                {
                    _resultPath = value;
                    SetConfigValue("resultPath", value);
                    NotifyPropertyChanged("ResultPath");
                }
            }
        }
        private string _conclusionPath = string.Empty;
        public string ConclusionPath
        {
            get => _conclusionPath;
            set
            {
                if (value != _conclusionPath)
                {
                    _conclusionPath = value;
                    SetConfigValue("conclusionPath", value);
                    NotifyPropertyChanged("ConclusionPath");
                }
            }
        }
        private string _dllPath = string.Empty;
        public string DllPath
        {
            get => _dllPath;
            set
            {
                if (value != _dllPath)
                {
                    _dllPath = value;
                    SetConfigValue("dllPath", value);
                    NotifyPropertyChanged("DLLPath");
                }
            }
        }

        private string _currentLocalPath = string.Empty;
        private void OnResultPathButtonClick(object obj)
        {
            string path = OnPathButtonClicked();
            if (path != null)
            {
                ResultPath = path;
            }
        }
        private RelayCommand _resultPathButtonCommand;
        public ICommand ResultPathButtonCommand
        {
            get
            {
                if (_resultPathButtonCommand == null)
                {
                    _resultPathButtonCommand = new RelayCommand(OnResultPathButtonClick, delegate { return true; });
                }
                return _resultPathButtonCommand;
            }
        }

        private void OnConclusionPathButtonClick(object obj)
        {
            string path = OnPathButtonClicked();
            if (path != null)
            {
                ConclusionPath = path;
            }
        }
        private RelayCommand _conclusionPathButtonCommand;
        public ICommand ConclusionPathButtonCommand
        {
            get
            {
                if (_conclusionPathButtonCommand == null)
                {
                    _conclusionPathButtonCommand = new RelayCommand(OnConclusionPathButtonClick, delegate { return true; });
                }
                return _conclusionPathButtonCommand;
            }
        }

        private void OnDllPathButtonClick(object obj)
        {
            string path = OnPathButtonClicked();
            if (path != null)
            {
                DllPath = path;
            }
        }
        private RelayCommand _dllPathButtonCommand;
        public ICommand DllPathButtonCommand
        {
            get 
            {
                if (_dllPathButtonCommand == null)
                {
                    _dllPathButtonCommand = new RelayCommand(OnDllPathButtonClick, delegate { return true; });
                }
                return _dllPathButtonCommand;
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

            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
           
            string productName = ProductType.DBR.ToString();
            TabItemViewModel item = new TabItemViewModel(productName,
                config.AppSettings.Settings["dbrPicturePath"].Value,
                config.AppSettings.Settings["dbrTemplatePath"].Value,
                config.AppSettings.Settings["dbrX86ProgramPath"].Value,
                config.AppSettings.Settings["dbrX64ProgramPath"].Value);                    
            item.PropertyChanged += this.TabItemPropertyChanged;
            _tabItems.Add(item);



            productName = ProductType.DLR.ToString();
            item = new TabItemViewModel(productName,
                config.AppSettings.Settings["dlrPicturePath"].Value,
                config.AppSettings.Settings["dlrTemplatePath"].Value,
                config.AppSettings.Settings["dlrX86ProgramPath"].Value,
                config.AppSettings.Settings["dlrX64ProgramPath"].Value);
            
            item.PropertyChanged += this.TabItemPropertyChanged;
            _tabItems.Add(item);
            productName = ProductType.DCN.ToString();
            item = new TabItemViewModel(productName,
                config.AppSettings.Settings["dcnPicturePath"].Value,
                config.AppSettings.Settings["dcnTemplatePath"].Value,
                config.AppSettings.Settings["dcnX86ProgramPath"].Value,
                config.AppSettings.Settings["dcnX64ProgramPath"].Value);
            item.PropertyChanged += this.TabItemPropertyChanged;
            _tabItems.Add(item);
       
            ResultPath = config.AppSettings.Settings["resultPath"].Value;
            ConclusionPath = config.AppSettings.Settings["conclusionPath"].Value;
            DllPath = config.AppSettings.Settings["dllPath"].Value;
            GenerateServerData();

        }

        private bool _isEnableStartButton = true;
        public bool IsEnableStartButton 
        {
            get => _isEnableStartButton;
            set
            {
                if (value != _isEnableStartButton)
                {
                    _isEnableStartButton = value;
                    NotifyPropertyChanged("IsEnableStartButton");
                }
            } 
        }
        private string _startButtonText = "启动服务器";
        public string StartButtonText
        {
            get => _startButtonText;
            set
            {
                if (value != _startButtonText)
                {
                    _startButtonText = value;
                    NotifyPropertyChanged("StartButtonText");
                }
            }
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
            IsEnableStartButton = false;
            StartButtonText = "运行中";
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

        private void GenerateServerData()
        {
            foreach (var tabItem in TabItems)
            {
                string key = tabItem.Header;
                ServerData serverData = new ServerData();
                serverData.DataType = "ServerData";
                _keyToData.Add(key, serverData);

                switch (key)
                {
                    case "DBR":
                        {
                            if (!serverData.KeyToPictureSet.ContainsKey("1D"))
                                serverData.KeyToPictureSet.Add("1D", new List<string>()
                            {   "Gen1D.csv",
                                "ScanDoc.OneD_Check.csv",
                                "zxing.oned_Check.csv",
                                "isthmusinc_Check.csv",
                                "N95-2592x1944_Check.csv",
                                "NonScanDoc.OneD_Check.csv",
                                "downPic_code128.csv",
                                "1DFrame.csv"
                            });
                            if (Enumerable.Count(serverData.DecodeTypeList, (decodeType) => decodeType == TestDataType.File.ToString()) == 0)
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
                if (tabItem.PictureSetPath != string.Empty)
                {
                    List<string> fileNames = CommonFuction.GetAllFiles(tabItem.PictureSetPath, "*.csv");
                    serverData.PictureSetList = fileNames;
                }
                if (tabItem.TemplatePath != string.Empty)
                {
                    List<string> fileNames = CommonFuction.GetAllFiles(tabItem.TemplatePath, "*.json");
                    serverData.TemplateList = fileNames;
                }
            }
        }
        private void StartTest()
        {          
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
                        if (DownLoadDllFromFtp(data))
                        {
                            CopyContentToLocalPath(data);
                            AlgorithmTestJson algorithmTestJson = TransClientToJson(data);
                            JsonSerializerOptions options = new JsonSerializerOptions();
                            options.WriteIndented = true;
                            string jsonString = JsonSerializer.Serialize(algorithmTestJson, options);
                            System.IO.File.WriteAllText(_currentLocalPath + "/config.json", jsonString);
                            if (algorithmTestJson != null)
                            {
                                LogMessage("Start test " + data.VersionInfo);
                                if (!CallTest(data))
                                {
                                    LogMessage("CallTest failed " + data.VersionInfo);
                                    if (_endCurrentTest)
                                    {
                                        _endCurrentTest = false;
                                    }
                                    //todo: clear current test file
                                    continue;
                                }
                                if (data.StandardVersion != string.Empty)
                                {
                                    data.OperateType = OperateType.Compare;
                                    lock (_compareWaitListLock)
                                    {
                                        App.Current.Dispatcher.Invoke((Action)delegate
                                        {
                                            CompareWaitList.Add(data);
                                        });                                       
                                    }
                                }
                                LogMessage("Test " + data.VersionInfo + " finished");
                                //CallCompare();
                                //CallGetPicture();
                                
                            }
                            else
                            {
                                MessageBox.Show("error algorithm json");
                            }
                        }
                        else
                        {
                            LogMessage("DownLoadDllFromFtp fail " + data.VersionInfo);
                        }
                    }
                }
            }
           
        }
        private bool CallTest(ClientData data)
        {
            string args = string.Empty;
            switch (data.OperateType)
            {
                case OperateType.Performance:
                    args += "-p ";
                    break;
                case OperateType.Stability:
                    args += "-s ";
                    break;
                case OperateType.Compare:
                default:
                    return false;
            }
            args += data.VersionInfo + "_" + DateTime.Now.ToString("HHmmss");            
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.UseShellExecute = false;
            processStartInfo.WorkingDirectory = _currentLocalPath;
            processStartInfo.Arguments = args;
            switch (data.ProductType)
            {
                case ProductType.DBR:
                    processStartInfo.FileName = _currentLocalPath + "/" + "DBRAutoTest.exe";
                    break;
                case ProductType.DLR:
                    processStartInfo.FileName = _currentLocalPath + "/" + "DLRAutoTest.exe";
                    break;
                case ProductType.DCN:
                    processStartInfo.FileName = _currentLocalPath + "/" + "DCNAutoTest.exe";
                    break;
                default:
                    return false;
            }
            try
            {
                Process testProcess = Process.Start(processStartInfo);
                if (testProcess == null)
                {
                    return false;
                }
                int exitCode = WaitProcess(testProcess);
                if (exitCode == 0)
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
            
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
        private void LogMessage(string message)
        {
            string logInfo = ConsoleMessage + "\n" + message + "\n";
            ConsoleMessage = logInfo;
        }
        private bool DownLoadDllFromFtp(ClientData data)
        {
            _currentLocalPath = DllPath + "/" + data.VersionInfo;       
            FTPHelper ftpHelper = new FTPHelper();
            if (ftpHelper.Download(data.FtpCachePath, _currentLocalPath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void CopyContentToLocalPath(ClientData data)
        {
            string contentPath = string.Empty;
            foreach (var tabItem in TabItems)
            {
                if (tabItem.Header == data.ProductType.ToString())
                {
                    contentPath = tabItem.X64ProgramPath;
                    break;
                }
            }
            if (contentPath == string.Empty)
                return;
            DirectoryInfo dirInfo = new DirectoryInfo(contentPath);
            FileInfo[] fileList = dirInfo.GetFiles("*.*");
            foreach (var file in fileList)
            {
                File.Copy(file.FullName, Path.Combine(_currentLocalPath, file.Name));
            }
        }
        private AlgorithmTestJson TransClientToJson(ClientData data)
        {
            AlgorithmTestJson algorithmTestJson = null;
            ProductType productType = data.ProductType;
            OperateType operateType = data.OperateType;
            TabItemViewModel productTab = null;
            foreach (var tab in TabItems)
            {
                if (tab.Header == productType.ToString())
                {
                    productTab = tab;
                    break;
                }
            }
            if (productTab == null)
            {
                return algorithmTestJson;
            }
            algorithmTestJson = new AlgorithmTestJson();
            if (operateType == OperateType.Performance)
                algorithmTestJson.FilePath = productTab.PictureSetPath;
            else if (operateType == OperateType.Stability)
                algorithmTestJson.FilePath = "";
            algorithmTestJson.TemplatePath = productTab.TemplatePath;
            algorithmTestJson.DecodeType = data.TestDataType.ToString();
            string templateName = string.Empty;
            if (productType == ProductType.DBR)
                templateName = "Test1";
            else if (productType == ProductType.DLR)
                templateName = "locr";
            algorithmTestJson.DefaultTemplate = new KeyValuePair<string, string>(templateName, data.DefaultTemplate);
            foreach (var templateToCsv in data.TemplateToCsvSet)
            {
                algorithmTestJson.Template.Add(new KeyValuePair<string, KeyValuePair<string, List<string>>>
                    (templateToCsv.Key, new KeyValuePair<string, List<string>>
                    (templateName, templateToCsv.Value)));
            }
            algorithmTestJson.ImageCsvSet = data.ImageCsvList;
            return algorithmTestJson;
        }
    }
}
