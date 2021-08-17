using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public enum TestType
    {
        Performance = 0,
        Stability
    }
    public enum TestDataType
    {
        File = 0,
        Buffer,
        Memory
    }
    public enum ProductType
    {
        DBR = 0,
        DLR,
        DCN
    }
    public class ClientData
    {
        public string TestType { get; set; } = string.Empty;
        public string TestDataType { get; set; } = string.Empty;
        //public string ImageSourcePath { get; set; } = string.Empty;
        //public string TemplatePath { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string DefaultTemplate { get; set; } = string.Empty;
        public List<string> ImageCsvList { get; set; } = new List<string>();
        public Dictionary<string, List<string>> TemplateToCsvSet = new Dictionary<string, List<string>>(); 
        
    }
}
