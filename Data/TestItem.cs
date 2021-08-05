using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class TestItem : IComparable<TestItem>
    {
        public string UpLoader { set; get; }
        public DateTime UploadTime { set; get; }
        public string TestType { set; get; }
        public TestItem(string upLoader = "", 
            DateTime dateTime = new DateTime(),
            string testType = "")
        {
            UpLoader = upLoader;
            UploadTime = dateTime;
            TestType = testType;
        }

        public override string ToString()
        {
            string tmp = UpLoader + " " + UploadTime.ToString() + " " + TestType;
            return tmp;
        }
        public static TestItem GenerateTestItem(string itemString)
        {
            var strArr = itemString.Split(' ');
            if (strArr.Length != 3)
                return new TestItem();
            TestItem tmpItem = new TestItem(strArr[0], DateTime.Parse(strArr[1]), strArr[2]);
            return tmpItem;
        }
        public int CompareTo(TestItem other)
        {
            return UploadTime.CompareTo(other.UploadTime);
        }
    }
}
