using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class ServerData
    {
        public string DataType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<string> PictureSetList { get; set; } = new List<string>();
        public List<string> TemplateList { get; set; } = new List<string>();
        public List<string> DecodeTypeList { get; set; } = new List<string>();
        public Dictionary<string, List<string>> KeyToPictureSet { get; set; } = new Dictionary<string, List<string>>();      
    }
}
