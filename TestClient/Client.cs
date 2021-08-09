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
        private ManualResetEvent _receiveDone = new ManualResetEvent(false);
        private string _recceiveString = string.Empty;
        public Client()
        {
            try
            {
               
                IPHostEntry ipHostInfo = Dns.GetHostEntry("127.0.0.1");
                
                _ipAddress = ipHostInfo.AddressList[1];
               
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
            if (_clientSocket == null)
            {
                return false;
            }
            bool isConnected = true;
            bool blockingState = _clientSocket.Blocking;
            try
            {
                byte[] tmp = new byte[1];

                _clientSocket.Blocking = false;
                _clientSocket.Send(tmp, 0, 0);
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
                _clientSocket.Blocking = blockingState;
            }

            return isConnected;
        }
        public string ReceiveData()
        {
            if (_clientSocket == null)
                return string.Empty;
            _receiveDone.Reset();
            AsyncReceive(_clientSocket);
            _receiveDone.WaitOne();
            return _recceiveString;
           
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

        override protected void FinishReceive()
        {
            _recceiveString = _stringBuilder.ToString();
            _receiveDone.Set();
        }
    }
}
