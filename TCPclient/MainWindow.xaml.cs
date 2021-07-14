using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TCPclient
{
    public partial class MainWindow : Window
    {
        TcpClient client = new TcpClient();
        NetworkStream ns;
        Thread thread;
        string name;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConDiscon_Click(object sender, RoutedEventArgs e)
        {
            if(tbName.Text == "") return;

            try
            {
                if ((string)btnConDiscon.Content == "Connect") Connect();
                else Disconnect();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                Disconnect();
            }
        }

        private void tbName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (tbName.Text == "") return;

                try
                {
                    if ((string)btnConDiscon.Content == "Connect") Connect();
                    else Disconnect();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Disconnect();
                }
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (tbMessage.Text == "") return;

            SendMessage();
        }

        private void tbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (tbMessage.Text == "") return;

            if (e.Key == Key.Enter) SendMessage();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void Connect()
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 5000;
            client = new TcpClient();
            client.Connect(ip, port);
            name = tbName.Text;
            rtbChat.AppendText($"{name} connected\n");
            rtbChat.ScrollToEnd();
            ns = client.GetStream();
            thread = new Thread(o => ReceiveData((TcpClient)o));
            thread.Start(client);
            tbName.IsReadOnly = true;
            btnConDiscon.Content = "Disconnect";
            tbMessage.IsReadOnly = false;
            btnSend.IsEnabled = true;
        }

        private void Disconnect()
        {
            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            ns.Close();
            client.Close();
            rtbChat.AppendText($"{name} disconnected\n");
            rtbChat.ScrollToEnd();
            tbName.IsReadOnly = false;
            btnConDiscon.Content = "Connect";
            tbMessage.IsReadOnly = true;
            btnSend.IsEnabled = false;
        }

        private void ReceiveData(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;
            string s;

            while ((byte_count = stream.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                s = Encoding.Unicode.GetString(receivedBytes, 0, byte_count);
                Dispatcher.Invoke(() =>
                {
                    rtbChat.AppendText(s);
                    rtbChat.ScrollToEnd();
                });
            }
        }

        private void SendMessage()
        {
            string s = $"{name:b}:  {tbMessage.Text}";
            tbMessage.Text = "";
            byte[] buffer = Encoding.Unicode.GetBytes(s);
            ns.Write(buffer, 0, buffer.Length);
        }
    }
}
