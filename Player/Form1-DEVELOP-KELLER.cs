// Simple Player sample application
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © AForge.NET, 2006-2011
// contacts@aforgenet.com
//

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Diagnostics;

using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;

namespace Kamerawagen
{
    public partial class Form1 : Form
    {
        enum videoKind
        {
            stream, playvideo
        }
        enum playKind
        {
            save, format1, format2, format3, undef
        }
        playKind play;
        enum framesize_t
        {
            FRAMESIZE_QQVGA,    // 160x120
            FRAMESIZE_QQVGA2,   // 128x160
            FRAMESIZE_QCIF,     // 176x144
            FRAMESIZE_HQVGA,    // 240x176
            FRAMESIZE_QVGA,     // 320x240
            FRAMESIZE_CIF,      // 400x296
            FRAMESIZE_VGA,      // 640x480
            FRAMESIZE_SVGA,     // 800x600
            FRAMESIZE_XGA,      // 1024x768
            FRAMESIZE_SXGA,     // 1280x1024
            FRAMESIZE_UXGA,     // 1600x1200
            FRAMESIZE_QXGA,     // 2048*1536
            FRAMESIZE_INVALID
        }
        string[] formatArray = new string[]{
    "QQVGA - 160x120",
    "QQVGA2 - 128x160",
    "QCIF - 176x144",
    "HQVGA - 240x176",
    "QVGA - 320x240",
    "CIF - 400x296",
    "VGA - 640x480",
    "SVGA - 800x600",
    "XGA - 1024x768",
    "SXGA - 1280x1024",
    "UXGA - 1600x1200",
    "QXGA - 2048*1536",
    };
    private Stopwatch stopWatch = null;
        String Url = string.Empty;
        byte[] M_SEND = { 0x00, 0x88, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        string IP_CameraBoard = "192.168.178.1";
        bool capturePicture;
        bool isSaving;
        private SaveFileDialog saveAvi;
        VideoFileWriter FileWriter;
        Bitmap video_image;
        int framerate = 10;

        private SnapshotForm snapshotForm = null;
        public const byte lngFrame = 13;
        // IN is even
        static public Int32 localPortDelta = 4;      // local port to listen on
        static public Int32 portinCAN = 15730 + localPortDelta;
        static public Int32 portoutCAN = 15731 + localPortDelta;

        Thread threadCamera;
        public UdpClient CameraServer;
        public UdpClient CameraClient;
        //
        delegate void EnableDisableButtonRunDeleg(byte no, bool value);
        delegate void setTextBoxDeleg(String txt);
        delegate void setToolTipDeleg(byte no, String txt);
        //
        // Class constructor
        public Form1()
        {
            InitializeComponent();
            mainPanel.Size = new Size(mainPanel.Size.Width, mainPanel.Size.Height - 30);
            mainPanel.Location = new Point(mainPanel.Location.X, mainPanel.Location.Y - 30);
            // CameraServer empfängt vom Cameramodul
            CameraServer = new UdpClient(portinCAN);
            // CameraClient sendet zum Cameramodul
            CameraClient = new UdpClient();

            // CAN
            threadCamera = new Thread(new ThreadStart(fromCamera));
            threadCamera.IsBackground = true;
            threadCamera.Start();

            ScanCamera();
        }
        enum buttonNo
        {
            disconnect = 0, cif, vga, svga, saveButton, single, video, finish, info, label, txtBox
        };

        private void EnableDisableButtonRun(byte no, bool value)
        {
            if (toolStrip1.InvokeRequired)
            {
                this.toolStrip1.Invoke(new EnableDisableButtonRunDeleg
                 (EnableDisableButtonRun), no, value);
            }
            else
            {
                ToolStripItem ts = toolStrip1.Items[no];
                ((ToolStripButton)ts).Enabled = value;
            }
        }

        private void setTextBox(String txt)
        {
            if (toolStrip1.InvokeRequired)
            {
                this.toolStrip1.Invoke(new setTextBoxDeleg
                 (setTextBox), txt);
            }
            else
            {
                ToolStripItem ts = toolStrip1.Items[(int) buttonNo.txtBox];
                ((ToolStripTextBox)ts).Text = txt;
            }
        }

        private void setToolTip(byte no, String txt)
        {
            if (toolStrip1.InvokeRequired)
            {
                this.toolStrip1.Invoke(new setToolTipDeleg
                 (setToolTip), no, txt);
            }
            else
            {
                ToolStripItem ts = toolStrip1.Items[no];
                ((ToolStripButton)ts).ToolTipText = txt;
            }
        }

        static string NetworkGateway()
        {
            string ip = null;
            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (f.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
                    {
                        ip = d.Address.ToString();
                    }
                }
            }
            return ip;
        }

        private void ScanCamera()
        {
            //    txtbHost_CAN.Text = Cnames.IP_CameraBoard;
            string gate_ip = NetworkGateway();
            if (gate_ip.Length < 4)
                return;
            string[] array = gate_ip.Split('.');

            IP_CameraBoard = array[0] + "." + array[1] + "." + array[2] + "." + 255;
            try
            {
                CameraClient.Connect(IP_CameraBoard, portoutCAN);
            }
            catch (Exception e)
            {
                MessageBox.Show("Scan-Exception: " + e.Message);
            }
            M_SEND[0x05] = 0xFF;
            CameraClient.Send(M_SEND, lngFrame);
        }

        private void fromCamera()
        {
            while (true)
            {
                IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, portinCAN);
                try
                {
                    byte[] content = CameraServer.Receive(ref remoteIPEndPoint);
                    if (content.Length > 0)
                    {
                        // Das Programm reagiert auf die Erkennung 
                        switch (content[0x01])
                        {
                            case 0x89:
                                Url = "http://" + remoteIPEndPoint.Address.ToString() + ":81/stream";
                                // create video source
                                MJPEGStream mjpegSource = new MJPEGStream(Url);
                                capturePicture = false;
                                // open it
                                switch (content[0x05])
                                {
                                    case 0xFF:
                                    case 0x05: //CIF
                                    case 0x85: //CIF
                                        play = playKind.format1;
                                        break;
                                    case 0x06: //VGA
                                    case 0x86: //VGA
                                        play = playKind.format2;
                                        break;
                                    case 0x07: //SVGA
                                    case 0x87: //SVGA
                                        play = playKind.format3;
                                        break;
                                    default:
                                        MessageBox.Show("Falsches Format");
                                        break;
                                }
                                setTextBox(formatArray[content[0x06+(byte)(play-1)]]);
                                setToolTip((byte)buttonNo.cif, formatArray[content[0x06]]);
                                setToolTip((byte)buttonNo.vga, formatArray[content[0x07]]);
                                setToolTip((byte)buttonNo.svga, formatArray[content[0x08]]);
                                this.videoSourcePlayer.Invoke(new MethodInvoker(() => OpenVideoSource(mjpegSource, videoKind.stream, false)));
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Threat-Exception: " + e.Message);
                }
            }
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseCurrentVideoSource();
        }

        // Open video source
        private void OpenVideoSource(IVideoSource source, videoKind vk, bool save)
        {
            // set busy cursor
            this.Cursor = Cursors.WaitCursor;
            switch (play)
            {
                case playKind.save:
                    EnableDisableButtonRun((byte)(buttonNo.disconnect), true);
                    EnableDisableButtonRun((byte)(buttonNo.cif), false);
                    EnableDisableButtonRun((byte)(buttonNo.vga), false);
                    EnableDisableButtonRun((byte)(buttonNo.svga), false);
                    EnableDisableButtonRun((byte)(buttonNo.saveButton), false);
                    EnableDisableButtonRun((byte)(buttonNo.single), false);
                    break;
                case playKind.format1:
                    EnableDisableButtonRun((byte)(buttonNo.disconnect), true);
                    EnableDisableButtonRun((byte)(buttonNo.cif), false);
                    EnableDisableButtonRun((byte)(buttonNo.vga), true);
                    EnableDisableButtonRun((byte)(buttonNo.svga), true);
                    EnableDisableButtonRun((byte)(buttonNo.saveButton), true);
                    EnableDisableButtonRun((byte)(buttonNo.single), true);
                    break;
                case playKind.format2:
                    EnableDisableButtonRun((byte)(buttonNo.disconnect), true);
                    EnableDisableButtonRun((byte)(buttonNo.cif), true);
                    EnableDisableButtonRun((byte)(buttonNo.vga), false);
                    EnableDisableButtonRun((byte)(buttonNo.svga), true);
                    EnableDisableButtonRun((byte)(buttonNo.saveButton), true);
                    EnableDisableButtonRun((byte)(buttonNo.single), true);
                    break;
                case playKind.format3:
                    EnableDisableButtonRun((byte)(buttonNo.disconnect), true);
                    EnableDisableButtonRun((byte)(buttonNo.cif), true);
                    EnableDisableButtonRun((byte)(buttonNo.vga), true);
                    EnableDisableButtonRun((byte)(buttonNo.svga), false);
                    EnableDisableButtonRun((byte)(buttonNo.saveButton), true);
                    EnableDisableButtonRun((byte)(buttonNo.single), true);
                    break;
                default:
                    EnableDisableButtonRun((byte)(buttonNo.disconnect), true);
                    EnableDisableButtonRun((byte)(buttonNo.cif), true);
                    EnableDisableButtonRun((byte)(buttonNo.vga), true);
                    EnableDisableButtonRun((byte)(buttonNo.svga), true);
                    EnableDisableButtonRun((byte)(buttonNo.saveButton), true);
                    EnableDisableButtonRun((byte)(buttonNo.single), true);
                    break;
            }
            EnableDisableButtonRun((byte)(buttonNo.video), false);
            EnableDisableButtonRun((byte)(buttonNo.finish), false);
            EnableDisableButtonRun((byte)(buttonNo.info), true);
            // stop current video source
            CloseCurrentVideoSource();
            if (vk == videoKind.stream && save == true)
            {
                int width = videoSourcePlayer.Width;
                int height = videoSourcePlayer.Height;

                // create instance of video writer
                isSaving = false;
                FileWriter = new VideoFileWriter();
                // create new video file
                saveAvi = new SaveFileDialog();
                saveAvi.Filter = "Avi Files (*.avi)|*.avi";
                if (saveAvi.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileWriter.Open(saveAvi.FileName, width, height, (int) (1.5*framerate), VideoCodec.MPEG4);
                    // create a bitmap to save into the video file
                    video_image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                    isSaving = true;
                }
            }

            // start new video source
            videoSourcePlayer.VideoSource = source;
            videoSourcePlayer.Start();

            // reset stop watch
            stopWatch = null;

            // start timer
            timer.Start();

            this.Cursor = Cursors.Default;
        }

        // Close video source if it is running
        private void CloseCurrentVideoSource()
        {
            if (videoSourcePlayer.VideoSource != null)
            {
                videoSourcePlayer.SignalToStop();

                // wait ~ 3 seconds
                for (int i = 0; i < 30; i++)
                {
                    if (!videoSourcePlayer.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                if (videoSourcePlayer.IsRunning)
                {
                    videoSourcePlayer.Stop();
                }

                videoSourcePlayer.VideoSource = null;
                if (isSaving == true)
                {
                    FileWriter.Close();
                    isSaving = false;
                }
            }
        }
        private Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }

            return result;
        }

        // New frame received by the player
        private void videoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            if (capturePicture == true)
            {
                capturePicture = false;
                ShowSnapshot(image);
            }

            if (isSaving == true)
            {
                Bitmap newImage = ResizeBitmap(image, video_image.Width, video_image.Height);
                FileWriter.WriteVideoFrame(newImage);
            }

            DateTime now = DateTime.Now;
            Graphics g = Graphics.FromImage(image);

            // paint current time
            SolidBrush brush = new SolidBrush(Color.Red);
            g.DrawString(now.ToString(), this.Font, brush, new PointF(5, 5));
            brush.Dispose();

            g.Dispose();
        }

        // On timer event - gather statistics
        private void timer_Tick(object sender, EventArgs e)
        {
            IVideoSource videoSource = videoSourcePlayer.VideoSource;

            if (videoSource != null)
            {
                // get number of frames since the last timer tick
                int framesReceived = videoSource.FramesReceived;

                if (stopWatch == null)
                {
                    stopWatch = new Stopwatch();
                    stopWatch.Start();
                }
                else
                {
                    stopWatch.Stop();

                    float fps = 1000.0f * framesReceived / stopWatch.ElapsedMilliseconds;
                    fpsLabel.Text = fps.ToString("F2") + " fps";
                    framerate = (framerate + (int) fps) / 2;

                    stopWatch.Reset();
                    stopWatch.Start();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ShowSnapshot(Bitmap snapshot)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<Bitmap>(ShowSnapshot), snapshot);
            }
            else
            {
                if (snapshotForm == null)
                {
                    snapshotForm = new SnapshotForm();
                    snapshotForm.FormClosed += new FormClosedEventHandler(snapshotForm_FormClosed);
                    snapshotForm.Show();
                }

                snapshotForm.SetImage(snapshot);
            }
        }

        private void snapshotForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            snapshotForm = null;
            EnableDisableButtonRun((byte)(buttonNo.disconnect), false);
            EnableDisableButtonRun((byte)(buttonNo.cif), true);
            EnableDisableButtonRun((byte)(buttonNo.vga), true);
            EnableDisableButtonRun((byte)(buttonNo.svga), true);
            EnableDisableButtonRun((byte)(buttonNo.saveButton), true);
            EnableDisableButtonRun((byte)(buttonNo.single), true);
            EnableDisableButtonRun((byte)(buttonNo.video), true);
            EnableDisableButtonRun((byte)(buttonNo.finish), true);
            EnableDisableButtonRun((byte)(buttonNo.info), true);
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // get new frame
            Bitmap bitmap = eventArgs.Frame;
            if (capturePicture == true)
            {
                capturePicture = false;
                ShowSnapshot(bitmap);
            }
            // process the frame
        }


        private void singlePicture_Click(object sender, EventArgs e)
        {
            capturePicture = true;
            EnableDisableButtonRun((byte)(buttonNo.disconnect), false);
            EnableDisableButtonRun((byte)(buttonNo.cif), false);
            EnableDisableButtonRun((byte)(buttonNo.vga), false);
            EnableDisableButtonRun((byte)(buttonNo.svga), false);
            EnableDisableButtonRun((byte)(buttonNo.saveButton), false);
            EnableDisableButtonRun((byte)(buttonNo.single), false);
            EnableDisableButtonRun((byte)(buttonNo.video), false);
            EnableDisableButtonRun((byte)(buttonNo.finish), false);
            EnableDisableButtonRun((byte)(buttonNo.info), false);
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            string message = "Wollen Sie wirklich beenden?";
            string caption = "Beenden";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(this, message, caption, buttons);

            if (result == DialogResult.Yes)
            {
                // Aufräumen
                this.Close();
            }
        }

        private void playVideo_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // create video source
                FileVideoSource fileSource = new FileVideoSource(openFileDialog.FileName);

                // open it
                OpenVideoSource(fileSource, videoKind.playvideo, false);
                EnableDisableButtonRun((byte)(buttonNo.disconnect), true);
                EnableDisableButtonRun((byte)(buttonNo.cif), false);
                EnableDisableButtonRun((byte)(buttonNo.vga), false);
                EnableDisableButtonRun((byte)(buttonNo.svga), false);
                EnableDisableButtonRun((byte)(buttonNo.saveButton), false);
                EnableDisableButtonRun((byte)(buttonNo.single), true);
                EnableDisableButtonRun((byte)(buttonNo.video), false);
                EnableDisableButtonRun((byte)(buttonNo.finish), true);
                EnableDisableButtonRun((byte)(buttonNo.info), false);
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            EnableDisableButtonRun((byte)(buttonNo.disconnect), false);
            EnableDisableButtonRun((byte)(buttonNo.cif), true);
            EnableDisableButtonRun((byte)(buttonNo.vga), true);
            EnableDisableButtonRun((byte)(buttonNo.svga), true);
            EnableDisableButtonRun((byte)(buttonNo.saveButton), false);
            EnableDisableButtonRun((byte)(buttonNo.single), false);
            EnableDisableButtonRun((byte)(buttonNo.video), true);
            EnableDisableButtonRun((byte)(buttonNo.finish), true);
            EnableDisableButtonRun((byte)(buttonNo.info), true);
            // stop current video source
            CloseCurrentVideoSource();
        }

        private void format1_Click(object sender, EventArgs e)
        {
            try
            {
                CameraClient.Connect(IP_CameraBoard, portoutCAN);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Format 1: " + exception.Message);
            }
            M_SEND[0x05] = (byte)framesize_t.FRAMESIZE_CIF;
            CameraClient.Send(M_SEND, lngFrame);
            play = playKind.format1;
        }

        private void Format2_Click(object sender, EventArgs e)
        {
            try
            {
                CameraClient.Connect(IP_CameraBoard, portoutCAN);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Format 2: " + exception.Message);
            }
            M_SEND[0x05] = (byte)framesize_t.FRAMESIZE_VGA;
            CameraClient.Send(M_SEND, lngFrame);
            play = playKind.format2;
        }

        private void format3_Click(object sender, EventArgs e)
        {
            try
            {
                CameraClient.Connect(IP_CameraBoard, portoutCAN);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Format 3: " + exception.Message);
            }
            M_SEND[0x05] = (byte)framesize_t.FRAMESIZE_SVGA;
            CameraClient.Send(M_SEND, lngFrame);
            play = playKind.format3;
        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            string message = "Kamerawagen für DiMo Version 1.00";
            string caption = "Info für Kamerawagen";
            MessageBox.Show(this, message, caption, MessageBoxButtons.OK);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            // open it for saving
            MJPEGStream mjpegSource = new MJPEGStream(Url);
            OpenVideoSource(mjpegSource, videoKind.stream, true);
            play = playKind.save;
        }
    }
}

