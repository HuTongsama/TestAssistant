using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SocketBase;

namespace TestClient
{
    public class Client : SocketBase.SocketBase
    {
        private Socket _clientSocket;
        private string _recceiveString = string.Empty;
        ManualResetEvent _connectEvent = new ManualResetEvent(false);
        public Client()
        {
        }
        public bool IsConnected()
        {
            return IsConnected(_clientSocket);
        }
        public string ReceiveData()
        {
            if (_clientSocket == null)
                return string.Empty;
            if (_clientSocket.Available > 0)
            {
                int receiveLength = SyncReceive(_clientSocket);
                if (receiveLength > 0)
                    _recceiveString = _stringBuilder.ToString();
                return _recceiveString;
            }
            else 
            {
                return string.Empty;
            }
           
        }

        public void SendData(byte[] data)
        {
            byte[] finalData = PackData(data);
            SyncSend(_clientSocket, finalData);
        }
        public bool Connect(string ip,int port = 8888)
        {
            try
            {
                MessageBox.Show(string.Format("start connect {0} port: {1}", ip, port));
                var addressArr = Dns.GetHostAddresses(ip);
                var ipAddress = addressArr[0];

                _clientSocket = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                _connectEvent.Reset();
                _clientSocket.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), null);
                if (!_connectEvent.WaitOne(10000))
                {
                    MessageBox.Show("Connect timeout");
                    return false;
                }               
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("connect server failed {0}", e.Message));
                return false;
            }
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                _clientSocket.EndConnect(ar);
                _connectEvent.Set();
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("connect callback fail {0}",e.Message));
                //throw;
            }
        }

        
    }
}
