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
using System.Diagnostics;

namespace Kinrou.io
{
    public class UdpServiceDataUpdateEventArgs : EventArgs
    {
        public byte [] data { get; internal set; }

        public UdpServiceDataUpdateEventArgs(byte [] data)
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
            catch (SocketException socketEx)
            {
                handleSocketException(socketEx); 
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

            try
            {
                if (isJoined)
                {
                    _client.BeginSendToGroup(requestData, 0, requestData.Length,
                        result =>
                        {
                            _client.EndSendToGroup(result);
                        }, null);
                }
            }
            catch (SocketException socketEx)
            {
                handleSocketException(socketEx);
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
            catch (SocketException socketEx)
            {
                handleSocketException(socketEx);
                isJoined = false;
            }
        }



        private void receive()
        {
            try
            {
                Array.Clear(_receiveBuffer, 0, _receiveBuffer.Length);
                _client.BeginReceiveFromGroup(_receiveBuffer, 0, _receiveBuffer.Length,
                    result =>
                    {
                        IPEndPoint source;

                        _client.EndReceiveFromGroup(result, out source);

                        OnDataUpdate(this, new UdpServiceDataUpdateEventArgs(_receiveBuffer));

                        receive();
                    }, null);
            }
            catch (SocketException ex)
            {
                handleSocketException(ex);
            }
            catch (Exception)
            {
            }
        }


        private void handleSocketException(SocketException socketEx)
        {
            if (socketEx.SocketErrorCode == SocketError.NetworkDown)
            {
                Debug.WriteLine("--- A SocketExeption has occurred. Please make sure your device is on a Wi-Fi network and the Wi-Fi network is operational");
            }
            else if (socketEx.SocketErrorCode == SocketError.ConnectionReset)
            {
                isJoined = false;
                join();
            }
            else if (socketEx.SocketErrorCode == SocketError.AccessDenied)
            {
                Debug.WriteLine("--- An error occurred with the sockect connection. Try Again.");
            }
            else
            {
                Debug.WriteLine(string.Format("--- {0}", socketEx.Message));
            }
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
