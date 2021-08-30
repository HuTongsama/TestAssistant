﻿using System;
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
        private bool _endCurrentProcess = false;
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
        private string _newFolderNameInConclusion = string.Empty;
        public MainWindowViewModel()
        {
            _listener = new Listener();
            _listener.GetServerData = GenerateServerDataList;
            _listener.TestListCallback = UpdateTestListCallback;
            _listener.CompareListCallback = UpdateCompareListCallback;

            _testProcess = new TestProcess();
            _endCurrentProcess = false;
            _tabItems = new ObservableCollection<TabItemViewModel>();

            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
           
            string productName = ProductType.DBR.ToString();
            TabItemViewModel item = new TabItemViewModel(productName,
                config.AppSettings.Settings["dbrPicturePath"].Value,
                config.AppSettings.Settings["dbrTemplatePath"].Value,
                config.AppSettings.Settings["dbrX86ProgramPath"].Value,
                config.AppSettings.Settings["dbrX64ProgramPath"].Value,
                config.AppSettings.Settings["dbrStdVersionPath"].Value);                    
            item.PropertyChanged += this.TabItemPropertyChanged;
            item.SendDataCallback = SendToAllClients;
            _tabItems.Add(item);
            
                     

            productName = ProductType.DLR.ToString();
            item = new TabItemViewModel(productName,
                config.AppSettings.Settings["dlrPicturePath"].Value,
                config.AppSettings.Settings["dlrTemplatePath"].Value,
                config.AppSettings.Settings["dlrX86ProgramPath"].Value,
                config.AppSettings.Settings["dlrX64ProgramPath"].Value,
                config.AppSettings.Settings["dlrStdVersionPath"].Value);            
            item.PropertyChanged += this.TabItemPropertyChanged;
            item.SendDataCallback = SendToAllClients;
            _tabItems.Add(item);
            productName = ProductType.DCN.ToString();
            item = new TabItemViewModel(productName,
                config.AppSettings.Settings["dcnPicturePath"].Value,
                config.AppSettings.Settings["dcnTemplatePath"].Value,
                config.AppSettings.Settings["dcnX86ProgramPath"].Value,
                config.AppSettings.Settings["dcnX64ProgramPath"].Value,
                config.AppSettings.Settings["dcnStdVersionPath"].Value);
            item.PropertyChanged += this.TabItemPropertyChanged;
            item.SendDataCallback = SendToAllClients;
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
            _endCurrentProcess = true;
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
        private TabItemViewModel GetTabModelByProduct(ProductType productType)
        {
            TabItemViewModel productTab = null;
            foreach (var tab in TabItems)
            {
                if (tab.Header == productType.ToString())
                {
                    productTab = tab;
                    break;
                }
            }
            return productTab;
        }
        private void GenerateServerData()
        {
            foreach (var tabItem in TabItems)
            {
                string key = tabItem.Header;
                ServerData serverData = new ServerData();
                serverData.ProductType = key;
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
                    serverData.PictureSetChanged = true;
                }
                if (tabItem.TemplatePath != string.Empty)
                {
                    List<string> fileNames = CommonFuction.GetAllFiles(tabItem.TemplatePath, "*.json");
                    serverData.TemplateList = fileNames;
                    serverData.TemplateSetChanged = true;
                }
                if (tabItem.StdVersionPath != string.Empty)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(tabItem.StdVersionPath);
                    var subDirs = dirInfo.GetDirectories();
                    List<string> dirNames = new List<string>();
                    serverData.StdVersionListChanged = true;
                    foreach (var dir in subDirs)
                    {
                        dirNames.Add(dir.Name);
                    }
                    serverData.StdVersionList = dirNames;
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

                          
                            ServerData serverData = new ServerData();
                            string message = string.Empty;
                            serverData.ProductType = data.ProductType.ToString();
                            if (algorithmTestJson != null)
                            {
                                message = "Start test " + data.VersionInfo;
                                LogMessage(message);
                                serverData.Message = message;
                                serverData.TestListChanged = true;
                                foreach (var waitItem in TestWaitList)
                                {
                                    serverData.TestWaitingList.Add(waitItem.VersionInfo);                                  
                                }
                                serverData.CompareListChanged = true;
                                foreach (var compareItem in CompareWaitList)
                                {
                                    serverData.CompareWaitingList.Add(compareItem.VersionInfo);
                                }
                                SendToAllClients(serverData);
                                if (!CallTest(data))
                                {
                                    message = "CallTest failed " + data.VersionInfo;
                                    LogMessage(message);
                                    serverData.Message = message;
                                    SendToAllClients(serverData);
                                    TabItemViewModel productTab = GetTabModelByProduct(data.ProductType);
                                    if (productTab != null)
                                    {
                                        string csvPath = productTab.PictureSetPath;
                                        DirectoryInfo dirInfo = new DirectoryInfo(csvPath);
                                        FileInfo[] fileList = dirInfo.GetFiles("*_.csv");
                                        foreach (var file in fileList)
                                        {
                                            File.Delete(file.FullName);
                                        }
                                    }
                                }
                                else
                                {
                                    if (data.OperateType == OperateType.Performance)
                                    {
                                        TabItemViewModel productTab = GetTabModelByProduct(data.ProductType);
                                        if (productTab != null)
                                        {
                                            string resultPath = ResultPath + "/" + data.VersionInfo;
                                            Directory.CreateDirectory(resultPath);
                                            string csvPath = productTab.PictureSetPath;
                                            DirectoryInfo dirInfo = new DirectoryInfo(csvPath);
                                            FileInfo[] fileList = dirInfo.GetFiles("*_.csv");
                                            foreach (var file in fileList)
                                            {
                                                File.Move(file.FullName, Path.Combine(resultPath, file.Name));
                                            }
                                        }
                                    }

                                    if (data.StandardVersion != null && data.StandardVersion != string.Empty)
                                    {
                                        data.OperateType = OperateType.Compare;
                                        lock (_compareWaitListLock)
                                        {
                                            App.Current.Dispatcher.Invoke((Action)delegate
                                            {
                                                data.TestVersion = data.VersionInfo;
                                                CompareWaitList.Add(data);
                                            });
                                        }
                                    }

                                    serverData.FinishedVersionInfo = data.VersionInfo;
                                    
                                }
                                message = "Test " + data.VersionInfo + " finished";
                                LogMessage(message);
                                serverData.Message = message;
                                SendToAllClients(serverData);
                                CallCompare();
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
                else if (CompareWaitList.Count > 0)
                {
                    CallCompare();
                }
            }
           
        }
        private bool CallTest(ClientData data)
        {

            AutoTestJson autoTestJson = new AutoTestJson();
            string autoTestJsonPath = _currentLocalPath + "/AutoTestConfig.json";
            if (System.IO.File.Exists(autoTestJsonPath))
            {
                byte[] jsonData = System.IO.File.ReadAllBytes(autoTestJsonPath);
                autoTestJson = JsonSerializer.Deserialize<AutoTestJson>(jsonData);
            }

            string args = "-m ";
            switch (data.OperateType)
            {
                case OperateType.Performance:
                    autoTestJson.testType = "-p";
                    break;
                case OperateType.Stability:
                    autoTestJson.testType = "-s";
                    break;
                case OperateType.Compare:
                default:
                    return false;
            }
            string autoTestPath = _currentLocalPath.Substring(0, _currentLocalPath.LastIndexOf("/"));
            args += data.VersionInfo;            
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.UseShellExecute = false;
            processStartInfo.WorkingDirectory = autoTestPath;
            processStartInfo.Arguments = args;
            processStartInfo.FileName = autoTestPath + "/" + "AutoTest.exe";
            switch (data.ProductType)
            {
                case ProductType.DBR:
                    autoTestJson.runnerName = "DBRAutoTest.exe";
                    break;
                case ProductType.DLR:
                    autoTestJson.runnerName = "DLRAutoTest.exe";
                    break;
                case ProductType.DCN:
                    autoTestJson.runnerName = "DCNAutoTest.exe";
                    break;
                default:
                    return false;
            }
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            string autoTestConfig = JsonSerializer.Serialize(autoTestJson,options);
            autoTestJsonPath = autoTestPath + "/AutoTestConfig.json";
            System.IO.File.WriteAllText(autoTestJsonPath, autoTestConfig);        
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
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = ConclusionPath;
                watcher.Created += ConclusionFolderCreated;
                watcher.EnableRaisingEvents = true;
                string message = string.Empty;
                ServerData serverData = new ServerData();
                foreach (var data in CompareWaitList)
                {
                    serverData.CompareWaitingList.Add(data.VersionInfo);
                }
                foreach (var data in CompareWaitList)
                {
                    serverData.CompareWaitingList.RemoveAt(0);
                    serverData.CompareListChanged = true;
                    message = "Start compare " + data.TestVersion;
                    LogMessage(message);
                    serverData.Message = message;
                    serverData.ProductType = data.ProductType.ToString();
                    SendToAllClients(serverData);
                    if (data.StandardVersion != string.Empty)
                    {
                        string stdPath = ResultPath + "\\" + data.StandardVersion;
                        string cmpPath = ResultPath + "\\" + data.TestVersion;
                        string args = ConclusionPath + "/" + " " + stdPath + " " + cmpPath;
                        ProcessStartInfo processStartInfo = new ProcessStartInfo();
                        processStartInfo.UseShellExecute = false;
                        processStartInfo.WorkingDirectory = _currentLocalPath;
                        processStartInfo.Arguments = args;
                        processStartInfo.FileName = _currentLocalPath + "\\" + "CSVConclusion.exe";
                        Process compareProcess = Process.Start(processStartInfo);
                        if (compareProcess == null)
                            continue;
                        int exitCode = WaitProcess(compareProcess);

                        if (exitCode == 0)
                        {
                            CallGetPicture(data);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    message = "Compare " + data.TestVersion + " finished";
                    LogMessage(message);
                    serverData.Message = message;
                    SendToAllClients(serverData);
                }
                App.Current.Dispatcher.Invoke((Action)delegate
               {
                   CompareWaitList.Clear();
               });
                
            }
        }
        private void ConclusionFolderCreated(object source, FileSystemEventArgs e)
        {
            _newFolderNameInConclusion = e.FullPath;
        }
        
        private void CallGetPicture(ClientData data)
        {
            if (!System.IO.Directory.Exists(_newFolderNameInConclusion))
                return;
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.UseShellExecute = false;
            processStartInfo.WorkingDirectory = _currentLocalPath;
            processStartInfo.Arguments = _newFolderNameInConclusion;
            processStartInfo.FileName = _currentLocalPath + "/" + "GetDiffPicture.exe";
            LogMessage("Start getPicture");
            Process compareProcess = Process.Start(processStartInfo);
            LogMessage("GetPicture finished");
            if (compareProcess == null)
                return;
            int exitCode = WaitProcess(compareProcess);
            if (exitCode == 0)
            {
                string path = "Users/hutong/" + data.UserName;
                FTPHelper ftpHelper = new FTPHelper();
                DirectoryInfo dirInfo = new DirectoryInfo(_newFolderNameInConclusion);
                if (ftpHelper.MakeDirectory(path))
                { 
                    ftpHelper.UploadDirectory(dirInfo, path); 
                }
                else
                {
                    LogMessage("Ftp path error :" + path);
                }
            }
            return;
        }
        private int WaitProcess(Process process)
        {
            while (true)
            {
                if (_endCurrentProcess)
                {
                    KillProcessAndChildren(process.Id);
                    process.WaitForExit();
                }
                if (process.HasExited)
                {
                    if (_endCurrentProcess)
                        _endCurrentProcess = false;
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
                serverData.ProductType = item.Header;
                _keyToData.Add(key, serverData);
            }           
            switch (e.PropertyName)
            {
                case "PictureSetPath":
                    {
                        List<string> fileNames = CommonFuction.GetAllFiles(item.PictureSetPath, "*.csv");
                        serverData.PictureSetList = fileNames;
                        serverData.PictureSetChanged = true;
                    }
                    break;
                case "TemplatePath":
                    {
                        List<string> fileNames = CommonFuction.GetAllFiles(item.TemplatePath, "*.json");
                        serverData.TemplateList = fileNames;
                        serverData.TemplateSetChanged = true;
                    }
                    break;
                case "StdVersionPath":
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(item.StdVersionPath);
                        var subDirs = dirInfo.GetDirectories();
                        List<string> dirNames = new List<string>();
                        foreach (var dir in subDirs)
                        {
                            dirNames.Add(dir.Name);
                        }
                        serverData.StdVersionList = dirNames;
                        serverData.StdVersionListChanged = true;
                    }
                    break;
                default:
                    break;
            }
      
            //string message = GenerateMessage();
            //byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            SendToAllClients(serverData);
        }
        private void SendToAllClients(ServerData serverData)
        {           
            string jsonString = JsonSerializer.Serialize(serverData);
            byte[] data = System.Text.Encoding.ASCII.GetBytes(jsonString);
            SendToAllClients(data);
        }
        private void SendToAllClients(byte[] data)
        {
            _listener.SendToAllClients(data);
        }
        private List<ServerData> GenerateServerDataList()
        {
            List<ServerData> serverDataList = new List<ServerData>();
            if (_keyToData.Count == 0)
                return serverDataList;
            foreach (var keyPair in _keyToData)
            {
                serverDataList.Add(keyPair.Value);
            }
            return serverDataList;
            
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
                    ServerData serverData = new ServerData();
                    foreach (var clientData in TestWaitList)
                    {
                        serverData.TestWaitingList.Add(clientData.VersionInfo);
                    }
                    serverData.TestListChanged = true;
                    SendToAllClients(serverData);
                });
                
            }
        }
        private void UpdateCompareListCallback(List<ClientData> dataList)
        {
            if (dataList == null || dataList.Count == 0)
                return;
            lock (_compareWaitListLock)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    foreach (var data in dataList)
                    {
                        CompareWaitList.Add(data);
                    }
                    ServerData serverData = new ServerData();
                    foreach (var clientData in CompareWaitList)
                    {
                        serverData.CompareWaitingList.Add(clientData.VersionInfo);
                    }
                    serverData.CompareListChanged = true;
                    SendToAllClients(serverData);
                });          
            }
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
            TabItemViewModel productTab = GetTabModelByProduct(productType);
            
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
            algorithmTestJson.DefaultTemplate = new Dictionary<string, string>()
            {
                {data.DefaultTemplate, templateName }
            };
            foreach (var templateToCsv in data.TemplateToCsvSet)
            {
                algorithmTestJson.Template.Add(
                    templateToCsv.Key,                      
                    new Dictionary<string, List<string>>()
                    {
                        { templateName,templateToCsv.Value }
                    });
            }
            algorithmTestJson.ImageCsvSet = data.ImageCsvList;
            return algorithmTestJson;
        }
    }
}
