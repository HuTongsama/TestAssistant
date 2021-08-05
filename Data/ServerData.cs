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
    }
}
