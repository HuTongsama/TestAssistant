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
using System.Text.Json;

namespace TestServer
{
    class Listener : SocketBase.SocketBase
    {
        private Socket _listenSocket;
        private List<Socket> _clientSockets;
        private readonly object _clientsLock = new object();
       
        private string _recceiveString = string.Empty;
        public Func<List<ServerData>> GetServerData { get; set; }
        public Action<List<ClientData>> TestListCallback { get; set; }
        public Action<List<ClientData>> CompareListCallback { get; set; }
        public Listener()
        {
            _clientSockets = new List<Socket>();
            try
            {
                int port = 8888;
                _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                _listenSocket.Listen(5);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("fail to create listenSocket {0}",e.Message));
                throw;
            }
            
        }
        public void StartListening()
        {
            try
            {
                _listenSocket.BeginAccept(AcceptCallback, _listenSocket);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("listening error {0}",e.Message));
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
        public void SendToClient(Socket client, byte[] data)
        {
            byte[] message = PackData(data);
            if (IsConnected(client))
            {
                SyncSend(client, data);
            }
        }
        public void ReceiveMessage()
        {
            List<ClientData> compareList = new List<ClientData>();
            List<ClientData> testList = new List<ClientData>();
            while (true)
            {
                compareList.Clear();
                testList.Clear();
                for (int index = 0; index < _clientSockets.Count; ++index)
                {
                    Socket socket = _clientSockets[index];
                    if (IsConnected(socket))
                    {
                        if (socket.Available > 0)
                        { 
                            int receiveLength = SyncReceive(socket);
                            if (receiveLength > 0)
                            {
                                string message = _stringBuilder.ToString();
                                ClientData clientData = JsonSerializer.Deserialize<ClientData>(message);
                                if (clientData.OperateType == OperateType.Compare)
                                {
                                    compareList.Add(clientData);
                                }
                                else if (clientData.OperateType == OperateType.Performance ||
                                    clientData.OperateType == OperateType.Stability)
                                {
                                    testList.Add(clientData);
                                }
                            }
                            
                        }
                    }
                    else
                    {
                        lock (_clientsLock)
                        {
                            _clientSockets.RemoveAt(index);
                            index--;
                        }
                    }
                }
                CompareListCallback(compareList);
                TestListCallback(testList);
            }
        }
        private void AcceptCallback(IAsyncResult ar)
        {
            Socket acceptSocket = _listenSocket.EndAccept(ar);
            lock (_clientsLock)
            { 
                _clientSockets.Add(acceptSocket);
            }
            var serverDataList = GetServerData();
            foreach (var serverData in serverDataList)
            {
                string jsonString = JsonSerializer.Serialize(serverData);
                byte[] messageBuf = System.Text.Encoding.ASCII.GetBytes(jsonString);
                byte[] finalBuf = PackData(messageBuf);
                SyncSend(acceptSocket, finalBuf);
            }         
            _listenSocket.BeginAccept(AcceptCallback, _listenSocket);

        }
            
    }
}
