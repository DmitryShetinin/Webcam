using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Consumer
{
    public partial class Consumer : Form
    {
        public Consumer()
        {
            InitializeComponent();
        }

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        [DllImport("kernel32.dll")]

        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static private IPEndPoint consumerEndPoint;
        private static UdpClient updClient = new UdpClient();


        private async void Form1_Load(object sender, EventArgs e)
        {
            var port = int.Parse(ConfigurationManager.AppSettings.Get("port")); 
            var client = new UdpClient(port); 

            while (true)
            {
                var data = await client.ReceiveAsync();
                using (var ms = new MemoryStream(data.Buffer))
                {
                    pictureBox1.Image = new Bitmap(ms); 
                }
            }
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            MessageBox.Show(String.Join("\n", host.AddressList.Where(i => i.AddressFamily == AddressFamily.InterNetwork
            ).Select(i => i.ToString()))); 

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var consumerIp = "127.0.0.1";
            var consumerPort = 48654;
            consumerEndPoint = new IPEndPoint(IPAddress.Parse(consumerIp), consumerPort);


            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
            Console.WriteLine("Press Enter to hide the console...");
            Console.ReadLine();
            ShowWindow(GetConsoleWindow(), SW_HIDE);
        }

        private static void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var bmp = new Bitmap(eventArgs.Frame, 800, 1000);
            try
            {
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    var bytes = ms.ToArray();
                    updClient.Send(bytes, bytes.Length, consumerEndPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
