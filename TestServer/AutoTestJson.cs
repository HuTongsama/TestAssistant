using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    class AutoTestJson
    {
        public string FilePath { get; set; } = string.Empty;
        public string TemplatePath { get; set; } = string.Empty;
        public string DecodeType { get; set; } = string.Empty;
        public KeyValuePair<string, string> DefaultTemplate = new KeyValuePair<string, string>();
        public List<string> ImageCsvSet { get; set; } = new List<string>();
        public List<KeyValuePair<string, KeyValuePair<string, List<string>>>> Template = new List<KeyValuePair<string, KeyValuePair<string, List<string>>>>();
    }
}
