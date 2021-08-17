using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Data;

namespace SocketBase
{
    public class SocketBase
    {
        protected StringBuilder _stringBuilder = new StringBuilder();
        private const int _packetSize = 256;
        private byte[] _packet = new byte[_packetSize];
        private int _packetPos = 0;
        private int _totalRead = 0;
        private DataHead _dataHead = null;

        private void InitPacket()
        {
            for (int i = 0; i < _packetSize; ++i)
            {
                _packet[i] = 0;
            }
            _packetPos = 0;
        }

        public void SyncSend(Socket socket, byte[] byteData)
        {
            int byteSend = socket.Send(byteData);
        }
        public int SyncReceive(Socket socket)
        {
            _stringBuilder.Clear();
            InitPacket();
            _dataHead = null;
            _totalRead = 0;
            while (true)
            {
                int byteRead = socket.Receive(_packet, _packetPos, _packetSize - _packetPos, 0);
                _totalRead += byteRead;
                _packetPos += byteRead;
                if (byteRead > 0)
                {
                    if (_dataHead == null)
                    {
                        int headLength = DataHead.GetDataHeadLength();
                        if (headLength > _packetSize)
                        {
                            MessageBox.Show("Error DataHead Length");
                            break;
                        }

                        if (_totalRead < headLength)
                        {
                            continue;
                        }
                        else
                        {
                            _dataHead = DataHead.ArrayToHead(_packet, headLength);
                            if (_dataHead == null)
                            {
                                MessageBox.Show("Create DataHead Failed");
                                break;
                            }
                            _stringBuilder.Append(Encoding.ASCII.GetString(_packet, headLength, _packetPos - headLength));
                            InitPacket();
                        }
                    }
                    else
                    {
                        _stringBuilder.Append(Encoding.ASCII.GetString(_packet, 0, byteRead));
                        InitPacket();
                    }

                    if (_dataHead != null)
                    {
                        int dataLength = _dataHead.DataLength;
                        if (_stringBuilder.Length >= dataLength)
                        {
                            break;
                        }
                    }
                }              
            }
            return _totalRead;
        }

        protected bool IsConnected(Socket socket)
        {
            if (socket == null)
            {
                return false;
            }
            bool isConnected = true;
            bool blockingState = socket.Blocking;
            try
            {
                byte[] tmp = new byte[1];

                socket.Blocking = false;
                socket.Send(tmp, 0, 0);
                Thread.Sleep(100);
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (!e.NativeErrorCode.Equals(10035))
                {
                    isConnected = false;
                }

            }
            finally
            {
                socket.Blocking = blockingState;
            }

            return isConnected;
        }

        protected byte[] PackData(byte[] originData)
        {
            DataHead head = new DataHead();
            head.DataLength = originData.Length;
            byte[] headBuf = head.ToByteArray();
            byte[] finalbuf = headBuf.Concat<Byte>(originData).ToArray();
            return finalbuf;
        }
        //public void AsyncSend(Socket socket, byte[] byteData)
        //{           
        //    socket.BeginSend(byteData, 0, byteData.Length, 0,
        //        new AsyncCallback(SendCallback), socket);
        //}
        //private void SendCallback(IAsyncResult ar)
        //{
        //    Socket sendSocket = (Socket)ar.AsyncState;
        //    int byteSend = sendSocket.EndSend(ar);
        //}
        //public void AsyncReceive(Socket socket)
        //{
        //    _stringBuilder.Clear();
        //    InitPacket();
        //    _dataHead = null;
        //    socket.BeginReceive(_packet, 0, _packetSize, 0,
        //        new AsyncCallback(ReceiveCallback), socket);
        //}
        //private void ReceiveCallback(IAsyncResult ar)
        //{
        //    Socket receiveSocket = (Socket)ar.AsyncState;
        //    int bytesRead = receiveSocket.EndReceive(ar);
        //    _totalRead += bytesRead;
        //    _packetPos += bytesRead;
        //    if (bytesRead > 0)
        //    {
        //        if (_dataHead == null)
        //        {
        //            int headLength = DataHead.GetDataHeadLength();
        //            if (headLength > _packetSize)
        //            {
        //                MessageBox.Show("Error DataHead Length");
        //                FinishReceive();
        //            }
        //            if (_totalRead < headLength)
        //            {
        //                receiveSocket.BeginReceive(_packet, _packetPos, _packetSize - bytesRead, 0
        //                    , new AsyncCallback(ReceiveCallback), receiveSocket);
        //            }
        //            else
        //            {
        //                _dataHead = DataHead.ArrayToHead(_packet, headLength);
        //                if (_dataHead == null)
        //                {
        //                    MessageBox.Show("Create DataHead Failed");
        //                    FinishReceive();
        //                }
        //                _stringBuilder.Append(Encoding.ASCII.GetString(_packet, headLength, _packetPos - headLength));
        //                InitPacket();
        //            }
        //        }
        //        else
        //        {
        //            _stringBuilder.Append(Encoding.ASCII.GetString(_packet, 0, bytesRead));
        //            InitPacket();
        //        }
        //        if (_dataHead != null)
        //        {
        //            int dataLength = _dataHead.DataLength;
        //            if (_stringBuilder.Length >= dataLength)
        //            {
        //                FinishReceive();
        //            }
        //        }
        //        receiveSocket.BeginReceive(_packet, 0, _packetSize, 0,
        //            new AsyncCallback(ReceiveCallback), receiveSocket);
        //    }
        //    else
        //    {

        //        if (_stringBuilder.Length > 1)
        //        {
        //            FinishReceive();                   
        //        }
        //    }
        //}
        //protected virtual void FinishReceive() 
        //{           
        //    _totalRead = 0;
        //}

    }
}
