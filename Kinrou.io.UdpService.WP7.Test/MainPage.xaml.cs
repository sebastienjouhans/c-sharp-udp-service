using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Kinrou.io.Udp;
using System.Text;

namespace WP7Test
{
    public partial class MainPage : PhoneApplicationPage
    {
        private UdpService _udpService;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            _udpService = new UdpService();
            _udpService.initialise();
            _udpService.dataUpdate += dataUpdateEventHandler;
        }

        public void open()
        {
            _udpService.join();
        }

        public void close()
        {
            _udpService.close();
        }


        public void send(string message)
        {
            string newMessage = String.Format("{0}{1}{2}", UdpHelper.START_PROTOCOL, message, UdpHelper.END_PROTOCOL);
            _udpService.send(newMessage);
        }


        public void dataUpdateEventHandler(object sender, EventArgs e)
        {
            byte [] receiveBuffer = (e as UdpServiceDataUpdateEventArgs).data;
            string message = Encoding.UTF8.GetString(receiveBuffer, 0, receiveBuffer.Length);
            processMessage(message);
        }


        private void processMessage(string data)
        {
            if (!UdpHelper.isDataValid(data))
            {
                return;
            }

            // trimming /00 and /99
            data = UdpHelper.trimStartAndEnd(data);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    Log(txtInput.Text, false);
                }
                catch (Exception)
                { }
            });

        }


        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(txtInput.Text))
            {
                send(txtInput.Text);
                Log(txtInput.Text, true);
            }
        }

        private void Log(string message, bool isOutgoing)
        {
            if (string.IsNullOrWhiteSpace(message.Trim('\0')))
                return;

            string direction = (isOutgoing) ? ">> " : "<< ";
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            message = timestamp + direction + message;
            lbLog.Items.Add(message);
            lbLog.ScrollIntoView(message);


        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            open();
        }


    }
}