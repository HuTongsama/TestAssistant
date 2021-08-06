using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class DataHead
    {
        public int DataLength { get; set; }
        private static int _dataHeadLen = 0;

        public byte[] ToByteArray()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms,this);
            return ms.ToArray();
        }

        static public int GetDataHeadLength()
        {
            if (_dataHeadLen != 0)
                return _dataHeadLen;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, new DataHead());
            _dataHeadLen = ms.ToArray().Length;
            return _dataHeadLen;
        }
        static public DataHead ArrayToHead(byte[] arrBytes, int headLen)
        {
            if (arrBytes.Length < headLen)
                return new DataHead();
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            ms.Write(arrBytes, 0, headLen);
            ms.Seek(0, SeekOrigin.Begin);
            DataHead head = (DataHead)bf.Deserialize(ms);
            return head;
            
        }
    }
}
