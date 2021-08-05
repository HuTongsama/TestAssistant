using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketBase
{
    public class SocketBase
    {
        protected StringBuilder _stringBuilder = new StringBuilder();
        private const int _packetSize = 256;
        private byte[] _packet = new byte[_packetSize];

        private void InitPacket()
        {
            for (int i = 0; i < _packetSize; ++i)
            {
                _packet[i] = 0;
            }
        }
        public void AsyncSend(Socket socket, string data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            socket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), socket);
        }
        private void SendCallback(IAsyncResult ar)
        {
            Socket sendSocket = (Socket)ar.AsyncState;
            int byteSend = sendSocket.EndSend(ar);
        }
        public void AsyncReceive(Socket socket)
        {
            _stringBuilder.Clear();
            InitPacket();
            socket.BeginReceive(_packet, 0, _packetSize, 0,
                new AsyncCallback(ReceiveCallback), socket);
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket receiveSocket = (Socket)ar.AsyncState;
            int bytesRead = receiveSocket.EndReceive(ar);

            if (bytesRead > 0)
            {
                _stringBuilder.Append(Encoding.ASCII.GetString(_packet, 0, bytesRead));
                InitPacket();
                receiveSocket.BeginReceive(_packet, 0, _packetSize, 0,
                    new AsyncCallback(ReceiveCallback), receiveSocket);
            }
            else
            {

                if (_stringBuilder.Length > 1)
                {
                    FinishReceive();
                }
            }
        }

        protected virtual void FinishReceive() { }
   
    }
}
