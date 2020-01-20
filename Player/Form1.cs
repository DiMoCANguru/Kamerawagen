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
            save, format_CIF, format_VGA, format_SVGA, format_XGA, format_SXGA, undef
        }
        playKind play;
        enum buttonNo
        {
            disconnect = 0, cif, vga, svga, xga, sxga, saveButton, single, video, finish, info, label, txtBox
        };
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
        bool PSRAM;

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
        delegate void EnableDisableButtonOneDeleg(buttonNo no, bool value);
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
            PSRAM = false;

            // tooltips
            setToolTip((byte)buttonNo.cif, formatArray[(int)framesize_t.FRAMESIZE_CIF]);
            setToolTip((byte)buttonNo.vga, formatArray[(int)framesize_t.FRAMESIZE_VGA]);
            setToolTip((byte)buttonNo.svga, formatArray[(int)framesize_t.FRAMESIZE_SVGA]);
            setToolTip((byte)buttonNo.xga, formatArray[(int)framesize_t.FRAMESIZE_XGA]);
            setToolTip((byte)buttonNo.sxga, formatArray[(int)framesize_t.FRAMESIZE_SXGA]);
            // CAN
            threadCamera = new Thread(new ThreadStart(fromCamera));
            threadCamera.IsBackground = true;
            threadCamera.Start();

            ScanCamera();
        }

        private void EnableDisableButtonOne(buttonNo no, bool value)
        {
            if (toolStrip1.InvokeRequired)
            {
                this.toolStrip1.Invoke(new EnableDisableButtonOneDeleg
                 (EnableDisableButtonOne), no, value);
            }
            else
            {
                ToolStripItem ts = toolStrip1.Items[(int) no];
                ((ToolStripButton)ts).Enabled = value;
            }
        }
        private void EnableDisableButtons(int buttonStatus)
        {
            int mask = 1;
            // disconnect = 0, cif, vga, svga, xga, sxga, saveButton, single, video, finish, info, label, txtBox
            for (buttonNo button = buttonNo.disconnect; button < buttonNo.info; button++)
            {
                bool onoff = (buttonStatus & mask) != 0;
                if ((button == buttonNo.xga || button == buttonNo.sxga) && (PSRAM == false))
                    onoff = false;
                EnableDisableButtonOne(button, onoff);
                mask <<= 1;
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
                ToolStripItem ts = toolStrip1.Items[(int)buttonNo.txtBox];
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
                                if (content[0x06] == 0x00)
                                {
                                    setToolTip((byte)buttonNo.xga, "Format nur mit PSRAM verfügbar");
                                    setToolTip((byte)buttonNo.sxga, "Format nur mit PSRAM verfügbar");
                                    PSRAM = false;
                                }
                                else
                                {
                                    setToolTip((byte)buttonNo.xga, formatArray[(int)framesize_t.FRAMESIZE_XGA]);
                                    setToolTip((byte)buttonNo.sxga, formatArray[(int)framesize_t.FRAMESIZE_SXGA]);
                                    PSRAM = true;
                               }
                                // open it
                                switch (content[0x05])
                                {
                                    case 0xFF:
                                    case 0x05: //CIF
                                        play = playKind.format_CIF;
                                        break;
                                    case 0x06: //VGA
                                        play = playKind.format_VGA;
                                        break;
                                    case 0x07: //SVGA
                                        play = playKind.format_SVGA;
                                        break;
                                    case 0x08: //XGA
                                        play = playKind.format_XGA;
                                        break;
                                    case 0x09: //SXGA
                                        play = playKind.format_SXGA;
                                        break;
                                    default:
                                        MessageBox.Show("Falsches Format");
                                        break;
                                }
                                setTextBox(formatArray[content[0x05]]);
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
            // info, finish, video, single, savebutton, sxga, xga, svga, vga, cif, diconnect
            //  0      0      0       1        1          1    1    1     1    0      1
            switch (play)
            {
                case playKind.save:
                    EnableDisableButtons(0b0000000000000001);
                    break;
                case playKind.format_CIF:
                    EnableDisableButtons(0b0000000011111101);
                    break;
                case playKind.format_VGA:
                    EnableDisableButtons(0b0000000011111011);
                    break;
                case playKind.format_SVGA:
                    EnableDisableButtons(0b0000000011110111);
                    break;
                case playKind.format_XGA:
                    EnableDisableButtons(0b0000000011101111);
                    break;
                case playKind.format_SXGA:
                    EnableDisableButtons(0b00000000111011111);
                    break;
                default:
                    EnableDisableButtons(0b00000000111111111);
                    break;
            }
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
                    FileWriter.Open(saveAvi.FileName, width, height, framerate, VideoCodec.MPEG4);
                    // create a bitmap to save into the video file
                    video_image = new Bitmap(width, height, PixelFormat.Format1bppIndexed);
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
                    framerate = (framerate + (int)fps) / 2;

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
                if (snapshot != null)
                    snapshotForm.SetImage(snapshot);
            }
        }

        private void snapshotForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            snapshotForm = null;
            // info, finish, video, single, savebutton, sxga, xga, svga, vga, cif, diconnect
            //  1      1      0       1        0          1    1    1     1    1      0
            int btns = 0b0000011010111111;
            switch (play)
            {
                case playKind.format_CIF:
                    btns = 0b0000011010111101;
                    break;
                case playKind.format_VGA:
                    btns = 0b0000011010111011;
                    break;
                case playKind.format_SVGA :
                    btns = 0b0000011010110111;
                    break;
                case playKind.format_XGA:
                    btns = 0b0000011010101111;
                    break;
                case playKind.format_SXGA:
                    btns = 0b0000011010011111;
                    break;
            }
            EnableDisableButtons(btns);
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // get new frame
            Bitmap bitmap = eventArgs.Frame;
            if (capturePicture == true)
            {
                if (bitmap != null)
                {
                    capturePicture = false;
                    ShowSnapshot(bitmap);
                }
            }
            // process the frame
        }


        private void singlePicture_Click(object sender, EventArgs e)
        {
            capturePicture = true;
            // info, finish, video, single, savebutton, sxga, xga, svga, vga, cif, diconnect
            EnableDisableButtons(0b0000000000000000);
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
                // info, finish, video, single, savebutton, sxga, xga, svga, vga, cif, diconnect
                //  0     0       0       1        0         0     0    0     0   0      1
                EnableDisableButtons(0b0000000010000001);
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            // info, finish, video, single, savebutton, sxga, xga, svga, vga, cif, diconnect
            //  1     1       1       0        0         1     1    1     1   1      0
            EnableDisableButtons(0b0000011100111110);
            // stop current video source
            CloseCurrentVideoSource();
        }

        private void format_CIF_Click(object sender, EventArgs e)
        {
            try
            {
                CameraClient.Connect(IP_CameraBoard, portoutCAN);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Format CIF: " + exception.Message);
            }
            M_SEND[0x05] = (byte)framesize_t.FRAMESIZE_CIF;
            CameraClient.Send(M_SEND, lngFrame);
            play = playKind.format_CIF;
        }

        private void format_VGA_Click(object sender, EventArgs e)
        {
            try
            {
                CameraClient.Connect(IP_CameraBoard, portoutCAN);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Format VGA: " + exception.Message);
            }
            M_SEND[0x05] = (byte)framesize_t.FRAMESIZE_VGA;
            CameraClient.Send(M_SEND, lngFrame);
            play = playKind.format_VGA;
        }

        private void format_SVGA_Click(object sender, EventArgs e)
        {
            try
            {
                CameraClient.Connect(IP_CameraBoard, portoutCAN);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Format SVGA: " + exception.Message);
            }
            M_SEND[0x05] = (byte)framesize_t.FRAMESIZE_SVGA;
            CameraClient.Send(M_SEND, lngFrame);
            play = playKind.format_SVGA;
        }

        private void format_XGA_Click(object sender, EventArgs e)
        {
            try
            {
                CameraClient.Connect(IP_CameraBoard, portoutCAN);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Format XGA: " + exception.Message);
            }
            M_SEND[0x05] = (byte)framesize_t.FRAMESIZE_XGA;
            CameraClient.Send(M_SEND, lngFrame);
            play = playKind.format_XGA;
        }

        private void format_SXGA_Click(object sender, EventArgs e)
        {
            try
            {
                CameraClient.Connect(IP_CameraBoard, portoutCAN);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Format SXGA: " + exception.Message);
            }
            M_SEND[0x05] = (byte)framesize_t.FRAMESIZE_SXGA;
            CameraClient.Send(M_SEND, lngFrame);
            play = playKind.format_SXGA;
        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            string message = "Kamerawagen für DiMo Version 1.00\n2020";
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

