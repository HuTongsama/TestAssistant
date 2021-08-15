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
    enum TestDataType
    {
        File = 0,
        Buffer,
        Memory
    }
    public class ClientData
    {
        public string TestType { get; set; } = string.Empty;
        public string TestDataType { get; set; } = string.Empty;
        public string ImageSourcePath { get; set; } = string.Empty;
        public string TemplatePath { get; set; } = string.Empty;
        public string DefaultTemplate { get; set; } = string.Empty;
        public List<string> ImageCsvList { get; set; } = new List<string>();
        public List<KeyValuePair<string, KeyValuePair<string, List<string>>>> Template =
            new List<KeyValuePair<string, KeyValuePair<string, List<string>>>>();
        ClientData()
        {
            
        }
    }
}
