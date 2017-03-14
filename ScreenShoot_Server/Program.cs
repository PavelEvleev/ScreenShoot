using System;
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

namespace ScreenShoot_Server
{
    class Program
    {
        public static int x,y;
        static void Main(string[] args)
        {
             x = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
             y = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;
            Console.WriteLine(x + " x " + y);
            //по не понятным причинам при подстановке System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width/Height он дает не правильные значения пришлось через переменные

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
                    Console.WriteLine("Send buffer = {0}", buffer.Length);
                    //FileStream fs = new FileStream("Fi.jpeg", FileMode.Create);
                    //fs.Write(buffer, 0, buffer.Length);
                    //fs.Flush();
                    //fs.Close();
                }
            }
            Thread.Sleep(1000);
            Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);

            var arg = new SocketAsyncEventArgs()
            {
                RemoteEndPoint= new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1025)
            };
            arg.SetBuffer(buffer, 0, buffer.Length);
            arg.Completed += Send_Completed;
            sendSocket.SendToAsync(arg);
            //отправка проходит успешно
        }

        private static void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            (sender as Socket).Shutdown(SocketShutdown.Send);
            (sender as Socket).Close();
            Console.WriteLine("Send completed");
        }
    }
}
