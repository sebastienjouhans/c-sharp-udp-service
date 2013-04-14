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
using Kinrou.io;

namespace WP7Test
{
    public partial class MainPage : PhoneApplicationPage
    {
        private UdpService updService;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            updService = new UdpService();
            updService.initialise();
            updService.dataUpdate += dataUpdateEventHandler;
        }

        public void open()
        {
            updService.join();
        }

        public void close()
        {
            updService.close();
        }


        public void send(string message)
        {
            string newMessage = String.Format("{0}{1}{2}", UdpHelper.START_PROTOCOL, message, UdpHelper.END_PROTOCOL);
            updService.send(newMessage);
        }


        public void dataUpdateEventHandler(object sender, EventArgs e)
        {
            string message = (e as UdpServiceDataUpdateEventArgs).data;
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