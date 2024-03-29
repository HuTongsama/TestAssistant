﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public enum OperateType
    {
        Performance = 0,
        Stability,
        Compare
    }
    public enum TestDataType
    {
        Empty = 0,
        File,
        Buffer,
        Memory
    }
    public enum ProductType
    {
        DBR = 0,
        DLR,
        DDN,
        SCANDIT
    }
    public class TestObject
    {
        public string Csv { get; set; } = string.Empty;
        public string Template { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public TestObject() { }
        public TestObject(string csv = "")
        {
            Csv = csv;
        }
    }
    public class ClientData:IComparable<ClientData>
    {
        public OperateType OperateType { get; set; } = OperateType.Performance;
        public TestDataType TestDataType { get; set; } = TestDataType.Empty;
        public ProductType ProductType { get; set; } = ProductType.DBR;
        public string DefaultTemplate { get; set; } = string.Empty;
        public string TestVersion { get; set; } = string.Empty;
        public string StandardVersion { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime UploadTime { get; set; } = new DateTime();
        public string VersionInfo { get; set; } = string.Empty;
        public string FtpCachePath { get; set; } = string.Empty;
        public List<TestObject> TestObjects { get; set; } = new List<TestObject>();
        public string ServerConfig { get; set; } = string.Empty;
        public bool UseServerConfig { get; set; } = false;
        public int CompareTo(ClientData other)
        {
            return UploadTime.CompareTo(other.UploadTime);
        }
        
    }
}
