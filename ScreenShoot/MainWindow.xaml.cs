﻿using System;
using System.Collections.Generic;
using System.IO;
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
using System.Drawing.Imaging;
using System.Collections;

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
            Thread threadReciev = new Thread(new ThreadStart(Reciev));
            threadReciev.Start();
        }

        void Reciev()
        {
            bool all = true;
            List<byte[]> messages = new List<byte[]>();

            while (all)
            {
                UdpClient recievMess = new UdpClient(1025);
                IPEndPoint get = null;
                var recievByty = recievMess.Receive(ref get);

                string s = Encoding.Unicode.GetString(recievByty);
                if (s != "break")
                {
                    messages.Add(recievByty);
                }else if (s == "break")
                {
                    all = false;
                }
                recievMess.Close();
            }

            int lengthMessage= int.Parse(Encoding.Unicode.GetString(messages.Last()));
            messages.Remove(messages.Last());

            ArrayList list = new ArrayList();

            for (int i = 0; i < messages.Count; i++)
            {
                var buf = new byte[8191];
                Array.Copy(messages[i], 1, buf, 0, 8191);
                list.AddRange(buf);
                if (i == messages.Count - 1)
                {
                    int end = lengthMessage - 8191*i;
                    buf = new byte[end];
                    Array.Copy(messages[i], 1, buf, 0, end);
                    list.AddRange(buf);
                }
               
            }
            var result = (byte[])list.ToArray(typeof(byte));

            using(MemoryStream streamImg= new MemoryStream(result))
            {
                System.Drawing.Image img = System.Drawing.Image.FromStream(streamImg);
                img.Save("SendImg.jpg", ImageFormat.Jpeg);
            }
            Receive_Completed();
            
        }

        private void Receive_Completed()
        {
            Dispatcher.Invoke(() =>
            {
                textBox.Text ="Image reciev and saved";
              
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
