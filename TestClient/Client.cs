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
        private IPAddress _ipAddress;
        private int _port;
        private string _recceiveString = string.Empty;
        public Client()
        {
            try
            {              
                IPHostEntry ipHostInfo = Dns.GetHostEntry("127.0.0.1");
                
                _ipAddress = ipHostInfo.AddressList[0];
               
               _port = 8888;
                _clientSocket = new Socket(_ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("create client failed {0}", e.Message));
                throw;
            }
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
        public void Connect()
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(_ipAddress, _port);
                _clientSocket.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), null);
            }
            catch (Exception)
            {
                MessageBox.Show("connect server failed");
                throw;
            }
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                _clientSocket.EndConnect(ar);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("connect callback fail {0}",e.Message));
                throw;
            }
        }

        
    }
}
