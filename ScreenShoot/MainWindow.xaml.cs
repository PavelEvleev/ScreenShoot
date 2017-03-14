﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

namespace ScreenShoot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            SendSignal();
        }


        private void SendSignal()
        {
            byte[] signal = new byte[1];

            Socket socketToSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);

            var arg = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1024)
            };

            arg.SetBuffer(signal, 0, signal.Length);
            arg.Completed += Send_Completed;

            socketToSend.SendToAsync(arg);

            byte[] reciev = new byte[250000];

            Socket socketToReciev = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
            socketToReciev.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1025));

            var arg1 = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Any,0)
            };

            arg1.SetBuffer(reciev, 0, reciev.Length);
            arg1.Completed += Receive_Completed;

            socketToReciev.ReceiveFromAsync(arg1);
            //клиент не получает обратно сообщение
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                textBox.Text = e.BytesTransferred.ToString();
                foreach (var b in e.Buffer)
                {
                    textBox.Text += b.ToString() + " ";
                }
            });
        }

        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            (sender as Socket).Shutdown(SocketShutdown.Send);
            (sender as Socket).Close();
            MessageBox.Show("Message send");
        }
    }
}