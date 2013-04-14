using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Text;

namespace Kinrou.io
{
    public class UdpServiceDataUpdateEventArgs : EventArgs
    {
        public string data { get; internal set; }

        public UdpServiceDataUpdateEventArgs(string data)
        {
            this.data = data;
        }
    }


    public class UdpService
    {
        public static int PORT = 55562;
        public static string MULTICAST_GROUP_ADDRESS = "224.0.0.13";

        public delegate void DataUpdateHandler(object sender, EventArgs data);
        public event DataUpdateHandler dataUpdate;

        private UdpAnySourceMulticastClient _client;

        private byte[] _receiveBuffer;
        private const int MAX_MESSAGE_SIZE = 512;



        public bool isJoined { get; set; }

        private int _port;
        public int port
        {
            get { return _port; }
            set { _port = value; }
        }

        private string _groupAddress = null;
        public string groupAddress
        {
            get { return _groupAddress; }
            set { _groupAddress = value; }
        }




        public UdpService()
        {
            _port = PORT;
            _groupAddress = MULTICAST_GROUP_ADDRESS;
            isJoined = false;
        }

        public UdpService(int p, string ga)
        {
            _port = p;
            _groupAddress = ga;
            isJoined = false;
        }

        public void initialise()
        {
            _client = new UdpAnySourceMulticastClient(IPAddress.Parse(_groupAddress), _port);
            _receiveBuffer = new byte[MAX_MESSAGE_SIZE];
        }

        public void join()
        {
            try
            {
                if (!isJoined)
                {
                    _client.BeginJoinGroup(new AsyncCallback(joinHandler), null);                    
                }
            }
            catch (SocketException)
            {
            }
        }


        public void close()
        {
            if (_client != null && isJoined)
            {
                try
                {
                    isJoined = false;
                    _client.Dispose();
                }
                catch (SocketException)
                {
                }
            }
        }


        public void send(string message)
        {
            byte[] requestData = Encoding.UTF8.GetBytes(message);

            if (isJoined)
            {
                _client.BeginSendToGroup(requestData, 0, requestData.Length,
                    result =>
                    {
                        _client.EndSendToGroup(result);
                    }, null);
            }

        }



        private void joinHandler(IAsyncResult result)
        {
            try
            {
                _client.EndJoinGroup(result);
                _client.MulticastLoopback = false;
                isJoined = true;
                receive();
            }
            catch (SocketException)
            {
                isJoined = false;
            }
        }



        private void receive()
        {
            Array.Clear(_receiveBuffer, 0, _receiveBuffer.Length);
            _client.BeginReceiveFromGroup(_receiveBuffer, 0, _receiveBuffer.Length,
                result =>
                {
                    IPEndPoint source;

                    _client.EndReceiveFromGroup(result, out source);

                    string message = Encoding.UTF8.GetString(_receiveBuffer, 0, _receiveBuffer.Length);

                    OnDataUpdate(this, new UdpServiceDataUpdateEventArgs(message));

                    receive();
                }, null);
        }


        private void OnDataUpdate(object sender, EventArgs data)
        {
            if (dataUpdate != null)
            {
                dataUpdate(this, data);
            }
        }
    }
}
