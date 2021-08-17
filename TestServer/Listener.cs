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
        public void SendToAllClients(byte[] data)
        {
            byte[] message = PackData(data);
            for (int socketId = 0; socketId < _clientSockets.Count; ++socketId)
            {
                if (IsConnected(_clientSockets[socketId]))
                {
                    SyncSend(_clientSockets[socketId], message);
                }
                else
                {
                    _clientSockets.RemoveAt(socketId);
                    socketId--;
                }
            }
        }
        private void AcceptCallback(IAsyncResult ar)
        {
            Socket acceptSocket = _listenSocket.EndAccept(ar);
            _clientSockets.Add(acceptSocket);
            string message = GennerateMessage();
            byte[] messageBuf = System.Text.Encoding.ASCII.GetBytes(message);
            byte[] finalBuf = PackData(messageBuf);          
            SyncSend(acceptSocket, finalBuf);
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
                    
                    int receiveLength = SyncReceive(socket);
                    if (receiveLength > 0)
                        _recceiveString = _stringBuilder.ToString();
                    if (_recceiveString != null)
                    {
                        TestItem item = TestItem.GenerateTestItem(_recceiveString);
                        _itemList.Add(item);
                    }
                }
            }
            _itemList.Sort();
        }
      
    }
}
