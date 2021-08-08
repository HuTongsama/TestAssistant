using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class ServerData
    {
        public string DataType { get; set; }

        public string Message { get; set; }
        public List<string> PictureSetList { get; set; }
        public List<string> TemplateList { get; set; }
        public Dictionary<string, List<string>> KeyToPictureSet { get; set; }

        public ServerData()
        {
            DataType = string.Empty;
            Message = string.Empty;
            PictureSetList = new List<string>();
            TemplateList = new List<string>();
            KeyToPictureSet = new Dictionary<string, List<string>>();
        }
    }
}
