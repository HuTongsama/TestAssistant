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
using System.Collections;

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
        private string _crashPath = string.Empty;
        public string CrashPath
        {
            get => _crashPath;
            set
            {
                if (value != _crashPath)
                {
                    _crashPath = value;
                    SetConfigValue("crashPath", value);
                    NotifyPropertyChanged("CrashPath");
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
        private RelayCommand _crashPathButtonCommand;
        public ICommand CrashPathButtonCommand
        {
            get 
            {
                if (_crashPathButtonCommand == null)
                {
                    _crashPathButtonCommand = new RelayCommand(delegate (object obj)
                    {
                        string path = OnPathButtonClicked();
                        if (path != null)
                        {
                            CrashPath = path;
                        }
                    },
                    delegate { return true; });
                }
                return _crashPathButtonCommand;
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
            CrashPath = config.AppSettings.Settings["crashPath"].Value;
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
            Dictionary<string, ServerConfig> productDefaultConfig = new Dictionary<string, ServerConfig>();
            if (System.IO.File.Exists("ServerConfig.json"))
            {
                string jsonData = System.IO.File.ReadAllText("ServerConfig.json");
                productDefaultConfig = JsonSerializer.Deserialize<Dictionary<string, ServerConfig>>(jsonData);
            }
            foreach (var tabItem in TabItems)
            {
                string key = tabItem.Header;
                ServerData serverData = new ServerData();
                serverData.ProductType = key;
                _keyToData.Add(key, serverData);

                if (Enumerable.Count(serverData.DecodeTypeList, (decodeType) => decodeType == TestDataType.File.ToString()) == 0)
                {
                    serverData.DecodeTypeList.Add(TestDataType.File.ToString());
                }
                if (Enumerable.Count(serverData.DecodeTypeList, (decodeType) => decodeType == TestDataType.Buffer.ToString()) == 0)
                {
                    serverData.DecodeTypeList.Add(TestDataType.Buffer.ToString());
                }
                if (productDefaultConfig.ContainsKey(key))
                {
                    ServerConfig config = productDefaultConfig[key];
                    tabItem.ServerConfig = config;
                    foreach (var tagToImageSet in config.TagToImageSet)
                    {
                        if (!serverData.KeyToPictureSet.ContainsKey(tagToImageSet.Key))
                        {
                            serverData.KeyToPictureSet.Add(tagToImageSet.Key, tagToImageSet.Value);
                        }
                    }
                    foreach (var serverConfig in config.DefaultConfig)
                    {
                        serverData.ConfigList.Add(serverConfig);
                    }
                    serverData.ConfigListChanged = true;
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
                        ServerData serverData = new ServerData();
                        string message = string.Empty;
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
                        if (DownLoadDllFromFtp(data))
                        {
                            try
                            {
                                CopyContentToLocalPath(data);
                                bool succeedConfig = false;
                                if (data.UseServerConfig)
                                {
                                    var tab = GetTabModelByProduct(data.ProductType);
                                    if (tab != null && data.ServerConfig != null)
                                    {
                                        string configPath = tab.ServerConfig.DefaultConfigPath + "/" + data.ServerConfig;
                                        if (System.IO.File.Exists(configPath))
                                        {
                                            succeedConfig = true;
                                            System.IO.File.Copy(configPath, _currentLocalPath + "/config.json");
                                        }
                                    }
                                }
                                else
                                {
                                    AlgorithmTestJson algorithmTestJson = TransClientToJson(data);
                                    if (algorithmTestJson != null)
                                    {
                                        try
                                        {
                                            JsonSerializerOptions options = new JsonSerializerOptions();
                                            options.WriteIndented = true;
                                            string jsonString = JsonSerializer.Serialize(algorithmTestJson, options);
                                            System.IO.File.WriteAllText(_currentLocalPath + "/config.json", jsonString);
                                            succeedConfig = true;
                                        }
                                        catch (Exception e)
                                        {
                                            MessageBox.Show(e.Message);
                                            succeedConfig = false;
                                        }
                                    }
                                }
                             
                                serverData.ProductType = data.ProductType.ToString();
                                if (succeedConfig)
                                {
                                    message = "Start test " + data.VersionInfo;
                                    LogMessage(message);
                                    serverData.Message = message;                                 
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
                                }
                                else
                                {
                                    message = "error test config";
                                    serverData.Message = message;
                                    LogMessage(message);
                                    SendToAllClients(serverData);                                   
                                }
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                                message = e.Message;
                                serverData.Message = message;
                                SendToAllClients(serverData);
                                continue;
                            }
                        }
                        else
                        {
                            message = "Server DownLoadDllFromFtp fail " + data.VersionInfo;
                            serverData.Message = message;           
                            SendToAllClients(serverData);
                            LogMessage(message);                          
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
                string logFilePath = _currentLocalPath + "/" + "result";
                if (System.IO.Directory.Exists(logFilePath))
                {
                    DirectoryInfo logDir = new DirectoryInfo(logFilePath);
                    var errFiles = logDir.GetFiles("*error.log");
                    Array.Sort(errFiles, delegate (FileInfo f1, FileInfo f2)
                    {
                        if (f1.LastWriteTime == f2.LastWriteTime)
                            return 0;
                        else if (f1.LastWriteTime > f2.LastWriteTime)
                            return 1;
                        else
                            return -1;
                    });
                    if (errFiles.Length > 0)
                    {
                        var file = errFiles.Last();
                        string dstPath = CrashPath + "/" + DateTime.Now.ToString("yyMMdd_HHmmss");
                        DirectoryInfo dstDir = new DirectoryInfo(dstPath);
                        if (!dstDir.Exists)
                        {
                            dstDir.Create();
                        }

                        using (StreamReader sr = file.OpenText())
                        {
                            var s = "";
                            while ((s = sr.ReadLine()) != null)
                            {
                                FileInfo tmp = new FileInfo(s);
                                tmp.CopyTo(dstPath + "/" + tmp.Name);
                            }
                        }
                        FTPHelper ftpHelper = new FTPHelper();
                        string path = "Testing/" + data.UserName;
                        if (ftpHelper.MakeDirectory(path))
                        {
                            ftpHelper.UploadDirectory(dstDir, path);
                        }                       
                    }
                }
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
                    _currentLocalPath = DllPath + "/" + data.VersionInfo;
                    if (!System.IO.Directory.Exists(_currentLocalPath))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(_currentLocalPath);
                        dirInfo.Create();
                        CopyContentToLocalPath(data);
                    }
                    serverData.CompareWaitingList.RemoveAt(0);
                    serverData.CompareListChanged = true;
                    message = "Start compare " + data.TestVersion;
                    LogMessage(message);
                    serverData.Message = message;
                    serverData.ProductType = data.ProductType.ToString();
                    SendToAllClients(serverData);

                    bool compareFinished = false;
                    string stdRootPath = ResultPath;
                    var productTab = GetTabModelByProduct(data.ProductType);
                    if (System.IO.Directory.Exists(productTab.StdVersionPath))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(productTab.StdVersionPath);
                        var subDirList = dirInfo.GetDirectories();
                        foreach (var subDir in subDirList)
                        {
                            if (subDir.Name == data.StandardVersion)
                            {
                                stdRootPath = productTab.StdVersionPath;
                                break;
                            }
                        }
                    }
                    string stdPath = stdRootPath + "\\" + data.StandardVersion;
                    string cmpPath = ResultPath + "\\" + data.TestVersion;
                    if (!System.IO.Directory.Exists(stdPath))
                    {
                        message += "Version " + data.StandardVersion + "can not be found\n";
                    }
                    else if (!System.IO.Directory.Exists(cmpPath))
                    {
                        message += "Version" + data.TestVersion + "can not be found\n";
                    }
                    else
                    {
                        string args = ConclusionPath + "/" + " " + stdPath + " " + cmpPath;
                        ProcessStartInfo processStartInfo = new ProcessStartInfo();
                        processStartInfo.UseShellExecute = false;
                        processStartInfo.WorkingDirectory = _currentLocalPath;
                        processStartInfo.Arguments = args;
                        processStartInfo.FileName = _currentLocalPath + "\\" + "CSVConclusion.exe";
                        Process compareProcess = Process.Start(processStartInfo);
                        if (compareProcess == null)
                        {
                            message += "Fail to start Compare";
                        }
                        else
                        {
                            int exitCode = WaitProcess(compareProcess);

                            if (exitCode == 0)
                            {
                                CallGetPicture(data);
                                compareFinished = true;
                            }
                            else
                            {
                                message += "Call compare failed";
                            }
                        }
                    }

                    if (compareFinished)
                    {
                        message = "Compare " + data.TestVersion + " finished";
                    }
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
                string path = "Testing/" + data.UserName;
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
            try
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
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
                string dstPath = Path.Combine(_currentLocalPath, file.Name);
                if (!System.IO.File.Exists(dstPath))
                    file.CopyTo(Path.Combine(_currentLocalPath, file.Name));                
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
