using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    class AutoTestJson
    {
        public string localWorkingPath { get; set; } = string.Empty;
        public string csvHome { get; set; } = string.Empty;
        public bool GetNewVersionOrNot { get; set; } = false;
        public bool DebugOrNot { get; set; } = false;
        public string runnerName { get; set; } = string.Empty;
        public string testType { get; set; } = string.Empty;
        public int TimeOutMin { get; set; } = 10;
        public string DebugCrashPath { get; set; } = string.Empty;
        public string DebugTimeoutPath { get; set; } = string.Empty;
        public string MemorySharePath { get; set; } = string.Empty;
    }

    class AlgorithmTestJson
    {
        public string FilePath { get; set; } = string.Empty;
        public string TemplatePath { get; set; } = string.Empty;
        public string DecodeType { get; set; } = string.Empty;
        public string DefaultTemplate { get; set; } = string.Empty;
        public string DefaultClass { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public List<TestObject> TestObjects { get; set; } = new List<TestObject>();
    }
}
