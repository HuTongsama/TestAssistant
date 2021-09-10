using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class ServerData
    {
        public string Message { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public List<string> PictureSetList { get; set; } = new List<string>();
        public bool PictureSetChanged { get; set; } = false;
        public List<string> TemplateList { get; set; } = new List<string>();
        public bool TemplateSetChanged { get; set; } = false;
        public List<string> DecodeTypeList { get; set; } = new List<string>();
        public List<string> StdVersionList { get; set; } = new List<string>();
        public bool StdVersionListChanged { get; set; } = false;
        public List<string> TestWaitingList { get; set; } = new List<string>();
        public bool TestListChanged { get; set; } = false;
        public List<string> CompareWaitingList { get; set; } = new List<string>();
        public bool CompareListChanged { get; set; } = false;
        public List<string> ConfigList { get; set; } = new List<string>();
        public bool ConfigListChanged { get; set; } = false;
        public Dictionary<string, List<string>> KeyToPictureSet { get; set; } = new Dictionary<string, List<string>>();
        public string FinishedVersionInfo { get; set; } = string.Empty;
    }
    public class ProductConfig
    {
        public string DefaultConfigPath { get; set; } = string.Empty;
        public List<string> DefaultConfig { get; set; } = new List<string>();
        public Dictionary<string, List<string>> TagToImageSet { get; set; } = new Dictionary<string, List<string>>();
    }
    public class ServerConfig
    {
        public Dictionary<string, ProductConfig> TagToProduct { get; set; } = new Dictionary<string, ProductConfig>();
        public string StabilityType { get; set; } = "x64";
        public string PicturePath { get; set; } = string.Empty;
    }
}
