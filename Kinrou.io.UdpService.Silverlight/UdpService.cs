using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace Kinrou.io
{
    public class UdpServiceDataUpdateEventArgs : EventArgs
    {
        public byte[] data { get; internal set; }

        public UdpServiceDataUpdateEventArgs(byte[] data)
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

        private UdpClient _udpClient = null;

        private IPEndPoint _broacastEndPoint;
        private IPEndPoint _receiveEndpoint;

        public bool isJoined { get; set; }

        private int _port;
        public int port 
        { 
            get { return _port; } 
            set { _port=value; } 
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

        public UdpService(int p , string ga)
        {
            _port = p;
            _groupAddress = ga;
            isJoined = false;
        }


        public void initialise()
        {
            _broacastEndPoint = new IPEndPoint(IPAddress.Broadcast, _port);
            _receiveEndpoint = new IPEndPoint(IPAddress.Any, _port);
            _udpClient = new UdpClient(_port, AddressFamily.InterNetwork);
        }


        public void join()
        {
           try
            {
                if (!isJoined)
                {
                    if (_groupAddress != null)
                    {
                        _udpClient.MulticastLoopback = false;
                        _udpClient.JoinMulticastGroup(IPAddress.Parse(_groupAddress));
                    }
                    _udpClient.BeginReceive(receiveHandler, null);
                    isJoined = true;
                }
            }
           catch (SocketException)
           {
           }
        }


        public void close()
        {
            if (_udpClient != null && isJoined)
            {
                try
                {
                    isJoined = false;
                    _udpClient.Close();
                 }
                catch (SocketException)
                {
                }
            }
        }


        public void send(string message)
        {
            byte[] bytemsg = Encoding.ASCII.GetBytes(message);
            send(bytemsg);
        }


        public void send(byte[] bytemsg)
        {
            try
            {
                _udpClient.Send(bytemsg, bytemsg.Length, _broacastEndPoint);
            }
            catch (SocketException)
            {
            }
        }



        private void receiveHandler(IAsyncResult result)
        {            
            try
            {
                byte[] receivedBytes = _udpClient.EndReceive(result, ref _receiveEndpoint);
                //string message = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);

                OnDataUpdate(this, new UdpServiceDataUpdateEventArgs(receivedBytes));
  
                _udpClient.BeginReceive(receiveHandler, null);
            }
            catch (Exception)
            {
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
