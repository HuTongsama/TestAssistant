using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    class AutoTestJson
    {
        
    }

    class AlgorithmTestJson
    {
        public string FilePath { get; set; } = string.Empty;
        public string TemplatePath { get; set; } = string.Empty;
        public string DecodeType { get; set; } = string.Empty;
        public Dictionary<string, string> DefaultTemplate { get; set; } = new Dictionary<string, string>();
        public List<string> ImageCsvSet { get; set; } = new List<string>();
        public Dictionary<string, Dictionary<string, List<string>>> Template { get; set; } =
            new Dictionary<string, Dictionary<string, List<string>>>();
    }
}
