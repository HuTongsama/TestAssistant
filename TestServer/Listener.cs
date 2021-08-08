using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using SocketBase;
using Data;
using System.Windows;
using System.Threading;

namespace TestServer
{
    class Listener : SocketBase.SocketBase
    {
        private Socket _listenSocket;
        private List<Socket> _clientSockets;
        private List<TestItem> _itemList;
        public List<TestItem> ItemList
        {
            get 
            {
                GenerateItemList();
                return _itemList;
            }
        }
        private ManualResetEvent _receiveDone = new ManualResetEvent(false);
        private string _recceiveString = string.Empty;
        public Func<string> GennerateMessage { get; set; }
        public Listener()
        {
            _clientSockets = new List<Socket>();
            _itemList = new List<TestItem>();
            try
            {
                int port = 8888;
                _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                _listenSocket.Listen(5);
            }
            catch (Exception)
            {
                MessageBox.Show("fail to create listenSocket");
                throw;
            }
            
        }
        public void StartListening()
        {
            try
            {
                _listenSocket.BeginAccept(AcceptCallback, _listenSocket);
            }
            catch (Exception)
            {
                MessageBox.Show("listening error");
                throw;
            }
        }
        private void AcceptCallback(IAsyncResult ar)
        {

            Socket acceptSocket = _listenSocket.EndAccept(ar);
            _clientSockets.Add(acceptSocket);
            string message = GennerateMessage();
            byte[] messageBuf = System.Text.Encoding.ASCII.GetBytes(message);
            DataHead head = new DataHead();
            head.DataLength = messageBuf.Length;
            byte[] headBuf = head.ToByteArray();
            byte[] finalBuf = headBuf.Concat<Byte>(messageBuf).ToArray();          
            AsyncSend(acceptSocket, finalBuf);
            _listenSocket.BeginAccept(AcceptCallback, _listenSocket);

        }
        private void GenerateItemList()
        {
            _itemList.Clear();
            for (int index = 0; index < _clientSockets.Count; ++index)
            {
                Socket socket = _clientSockets[index];
                if (socket.Available > 0)
                {
                    _recceiveString = null;
                    _receiveDone.Reset();
                    AsyncReceive(socket);
                    _receiveDone.WaitOne();
                    if (_recceiveString != null)
                    {
                        TestItem item = TestItem.GenerateTestItem(_recceiveString);
                        _itemList.Add(item);
                    }
                }
            }
            _itemList.Sort();
        }

        override protected void  FinishReceive()
        {
            _recceiveString = _stringBuilder.ToString();
            _receiveDone.Set();
        }
    }
}
