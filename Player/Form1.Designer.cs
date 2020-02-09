namespace Kamerawagen
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.fpsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.disconnectButton = new System.Windows.Forms.ToolStripButton();
            this.format_CIF = new System.Windows.Forms.ToolStripButton();
            this.format_VGA = new System.Windows.Forms.ToolStripButton();
            this.format_SVGA = new System.Windows.Forms.ToolStripButton();
            this.format_XGA = new System.Windows.Forms.ToolStripButton();
            this.format_SXGA = new System.Windows.Forms.ToolStripButton();
            this.saveButton = new System.Windows.Forms.ToolStripButton();
            this.singlePicture = new System.Windows.Forms.ToolStripButton();
            this.rotatePicture = new System.Windows.Forms.ToolStripButton();
            this.playVideo = new System.Windows.Forms.ToolStripButton();
            this.exitButton = new System.Windows.Forms.ToolStripButton();
            this.infoButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.formatTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.videoSourcePlayer = new AForge.Controls.VideoSourcePlayer();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip.SuspendLayout();
            this.mainPanel.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fpsLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 711);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(994, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // fpsLabel
            // 
            this.fpsLabel.Name = "fpsLabel";
            this.fpsLabel.Size = new System.Drawing.Size(979, 17);
            this.fpsLabel.Spring = true;
            this.fpsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.toolStrip1);
            this.mainPanel.Controls.Add(this.videoSourcePlayer);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(994, 733);
            this.mainPanel.TabIndex = 2;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(45, 25);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.disconnectButton,
            this.format_CIF,
            this.format_VGA,
            this.format_SVGA,
            this.format_XGA,
            this.format_SXGA,
            this.saveButton,
            this.singlePicture,
            this.rotatePicture,
            this.playVideo,
            this.exitButton,
            this.infoButton,
            this.toolStripLabel1,
            this.formatTextBox});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(994, 32);
            this.toolStrip1.TabIndex = 6;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // disconnectButton
            // 
            this.disconnectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.disconnectButton.Image = ((System.Drawing.Image)(resources.GetObject("disconnectButton.Image")));
            this.disconnectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(49, 29);
            this.disconnectButton.Text = "disconnectButton";
            this.disconnectButton.ToolTipText = "Von der Kamera trennen";
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // format_CIF
            // 
            this.format_CIF.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.format_CIF.Image = ((System.Drawing.Image)(resources.GetObject("format_CIF.Image")));
            this.format_CIF.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.format_CIF.Name = "format_CIF";
            this.format_CIF.Size = new System.Drawing.Size(49, 29);
            this.format_CIF.Text = "Format CIF";
            this.format_CIF.Click += new System.EventHandler(this.format_CIF_Click);
            // 
            // format_VGA
            // 
            this.format_VGA.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.format_VGA.Image = ((System.Drawing.Image)(resources.GetObject("format_VGA.Image")));
            this.format_VGA.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.format_VGA.Name = "format_VGA";
            this.format_VGA.Size = new System.Drawing.Size(49, 29);
            this.format_VGA.Text = "Format VGA";
            this.format_VGA.Click += new System.EventHandler(this.format_VGA_Click);
            // 
            // format_SVGA
            // 
            this.format_SVGA.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.format_SVGA.Image = ((System.Drawing.Image)(resources.GetObject("format_SVGA.Image")));
            this.format_SVGA.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.format_SVGA.Name = "format_SVGA";
            this.format_SVGA.Size = new System.Drawing.Size(49, 29);
            this.format_SVGA.Text = "Format SVGA";
            this.format_SVGA.Click += new System.EventHandler(this.format_SVGA_Click);
            // 
            // format_XGA
            // 
            this.format_XGA.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.format_XGA.Image = ((System.Drawing.Image)(resources.GetObject("format_XGA.Image")));
            this.format_XGA.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.format_XGA.Name = "format_XGA";
            this.format_XGA.Size = new System.Drawing.Size(49, 29);
            this.format_XGA.Text = "Format XGA";
            this.format_XGA.Click += new System.EventHandler(this.format_XGA_Click);
            // 
            // format_SXGA
            // 
            this.format_SXGA.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.format_SXGA.Image = ((System.Drawing.Image)(resources.GetObject("format_SXGA.Image")));
            this.format_SXGA.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.format_SXGA.Name = "format_SXGA";
            this.format_SXGA.Size = new System.Drawing.Size(49, 29);
            this.format_SXGA.Text = "Format SXGS";
            this.format_SXGA.Click += new System.EventHandler(this.format_SXGA_Click);
            // 
            // saveButton
            // 
            this.saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveButton.Image = ((System.Drawing.Image)(resources.GetObject("saveButton.Image")));
            this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(49, 29);
            this.saveButton.Text = "Video speichern";
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // singlePicture
            // 
            this.singlePicture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.singlePicture.Image = ((System.Drawing.Image)(resources.GetObject("singlePicture.Image")));
            this.singlePicture.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.singlePicture.Name = "singlePicture";
            this.singlePicture.Size = new System.Drawing.Size(49, 29);
            this.singlePicture.Text = "Einzelbild";
            this.singlePicture.Click += new System.EventHandler(this.singlePicture_Click);
            // 
            // rotatePicture
            // 
            this.rotatePicture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rotatePicture.Image = ((System.Drawing.Image)(resources.GetObject("rotatePicture.Image")));
            this.rotatePicture.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.rotatePicture.Name = "rotatePicture";
            this.rotatePicture.Size = new System.Drawing.Size(49, 29);
            this.rotatePicture.Text = "rotatePicture";
            this.rotatePicture.Click += new System.EventHandler(this.rotatePicture_Click);
            // 
            // playVideo
            // 
            this.playVideo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.playVideo.Image = ((System.Drawing.Image)(resources.GetObject("playVideo.Image")));
            this.playVideo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.playVideo.Name = "playVideo";
            this.playVideo.Size = new System.Drawing.Size(49, 29);
            this.playVideo.Text = "Video abspielen";
            this.playVideo.Click += new System.EventHandler(this.playVideo_Click);
            // 
            // exitButton
            // 
            this.exitButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.exitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.exitButton.Image = ((System.Drawing.Image)(resources.GetObject("exitButton.Image")));
            this.exitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(49, 29);
            this.exitButton.Text = "Das Programm beenden";
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // infoButton
            // 
            this.infoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.infoButton.Image = ((System.Drawing.Image)(resources.GetObject("infoButton.Image")));
            this.infoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.infoButton.Name = "infoButton";
            this.infoButton.Size = new System.Drawing.Size(49, 29);
            this.infoButton.Text = "Info Kamerawagen";
            this.infoButton.Click += new System.EventHandler(this.infoButton_Click);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(51, 29);
            this.toolStripLabel1.Text = "Format: ";
            // 
            // formatTextBox
            // 
            this.formatTextBox.Name = "formatTextBox";
            this.formatTextBox.Size = new System.Drawing.Size(100, 32);
            // 
            // videoSourcePlayer
            // 
            this.videoSourcePlayer.AutoSizeControl = true;
            this.videoSourcePlayer.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.videoSourcePlayer.ForeColor = System.Drawing.Color.White;
            this.videoSourcePlayer.Location = new System.Drawing.Point(336, 245);
            this.videoSourcePlayer.Name = "videoSourcePlayer";
            this.videoSourcePlayer.Size = new System.Drawing.Size(322, 242);
            this.videoSourcePlayer.TabIndex = 0;
            this.videoSourcePlayer.VideoSource = null;
            this.videoSourcePlayer.NewFrame += new AForge.Controls.VideoSourcePlayer.NewFrameHandler(this.videoSourcePlayer_NewFrame);
            // 
            // timer
            // 
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "AVI Dateien (*.avi)|*.avi|Alle Dateien (*.*)|*.*";
            this.openFileDialog.Title = "Ein Video öffnen";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(994, 733);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.mainPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Kamerawagen";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.Panel mainPanel;
        private AForge.Controls.VideoSourcePlayer videoSourcePlayer;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ToolStripStatusLabel fpsLabel;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton disconnectButton;
        private System.Windows.Forms.ToolStripButton singlePicture;
        private System.Windows.Forms.ToolStripButton playVideo;
        private System.Windows.Forms.ToolStripButton exitButton;
        private System.Windows.Forms.ToolStripButton format_CIF;
        private System.Windows.Forms.ToolStripButton format_VGA;
        private System.Windows.Forms.ToolStripButton format_SVGA;
        private System.Windows.Forms.ToolStripButton saveButton;
        private System.Windows.Forms.ToolStripButton infoButton;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox formatTextBox;
        private System.Windows.Forms.ToolStripButton format_XGA;
        private System.Windows.Forms.ToolStripButton format_SXGA;
        private System.Windows.Forms.ToolStripButton rotatePicture;
    }
}
