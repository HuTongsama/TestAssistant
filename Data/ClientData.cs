using System;
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
        DCN
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
        public List<string> ImageCsvList { get; set; } = new List<string>();
        public Dictionary<string, List<string>> TemplateToCsvSet = new Dictionary<string, List<string>>();

        public int CompareTo(ClientData other)
        {
            return UploadTime.CompareTo(other.UploadTime);
        }
        
    }
}
