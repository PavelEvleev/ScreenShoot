﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace ScreenShoot_Server
{
    class Program
    {
        public static int x,y;
        static void Main(string[] args)
        {
           
            
             x = Screen.PrimaryScreen.Bounds.Width; 
             y = Screen.PrimaryScreen.Bounds.Height;
            Console.WriteLine(x + " x " + y);
            //по не понятным причинам при подстановке System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width/Height  дает не правильные значения пришлось через переменные

            Socket receivSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
            receivSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1024));

            var arg = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Any,0)
            };
            byte[] recive = new byte[1024];
            arg.SetBuffer(recive, 0, recive.Length);
            arg.Completed += Receive_Completed;

            receivSocket.ReceiveFromAsync(arg);

           
            Console.ReadLine();

        }

        private static void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            byte[] buffer;
            Console.WriteLine("Receved {0}", e.BytesTransferred);
            using (Bitmap btm = new Bitmap(x, y))
            {
                using (Graphics g = Graphics.FromImage(btm))
                {

                    g.CopyFromScreen(0, 0, 0, 0, btm.Size,
                                 CopyPixelOperation.SourceCopy);
                }
                using (var stream = new MemoryStream())
                {
                    
                    btm.Save(stream, ImageFormat.Jpeg);
                    buffer = stream.ToArray();
                    Console.WriteLine("send buffer = {0}", buffer.Length);
                    FileStream fs = new FileStream("Fi.jpeg", FileMode.Create);
                    fs.Write(buffer, 0, buffer.Length);
                    fs.Flush();
                    fs.Close();
                }
            }
            Thread.Sleep(1000);
            StartSendToAsync(buffer);
           
        }

        private static void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            (sender as Socket).Shutdown(SocketShutdown.Send);
            (sender as Socket).Close();
            Console.WriteLine("Send completed");
        }
       
            //отправка проходит успешно

        public static void StartSendToAsync(byte[] buffer)
        {
            IPEndPoint receiver = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1025);
            
            //разбивать массив на пакеты длинной в 8192 байта, добавлять первым битом порядковый номер покета , на клиенте вычитывать покет и добавлять его в List, после выставить по порядку и получить массив.
            int Npackets = 0;
            if (0 < buffer.Length % 8191)
            {
                Npackets = buffer.Length / 8191;
                Npackets += 1;
            }
            else
            {
                Npackets = buffer.Length / 8191;
            }
            try
            {
                int beginByte = 0;

                for (int i = 0; i < Npackets; i++)
                {
                    UdpClient sendMes = new UdpClient();
                    byte[] bufferSend = new byte[8192];
                    
                    if (i > 0 && i == Npackets - 1)
                    {
                        int end = buffer.Length - beginByte;
                        Array.Copy(buffer, beginByte , bufferSend, 1, end);
                    }
                    else if (i == 0)
                    {
                        Array.Copy(buffer, beginByte, bufferSend, 1, 8191);
                        beginByte += 8191;
                    }
                    else
                    {
                        Array.Copy(buffer, beginByte, bufferSend, 1, 8191);
                        beginByte += 8191;
                    }

                    bufferSend[0] = Convert.ToByte(i);
                    Thread.Sleep(1000);
                    sendMes.Send(bufferSend, bufferSend.Length, receiver);
                    sendMes.Close();
                }
            }
            catch(ArgumentException ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
            UdpClient SendLength = new UdpClient();
            byte[] lengthBuffer = Encoding.Unicode.GetBytes(buffer.Length.ToString());
            SendLength.Send(lengthBuffer, lengthBuffer.Length, receiver);
            SendLength.Close();
            UdpClient Break = new UdpClient();
            byte[] breakWord = Encoding.Unicode.GetBytes("break");
            Break.Send(breakWord, breakWord.Length, receiver);
            Break.Close();
          
        }

    }
}
