namespace MoveCursorByHand
{
    partial class MinimizedForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MinimizedForm));
            this.returnPictureBox = new System.Windows.Forms.PictureBox();
            this.captureImageBoxMinimized = new Emgu.CV.UI.ImageBox();
            this.exitButton = new MaterialSkin.Controls.MaterialRaisedButton();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.returnPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.captureImageBoxMinimized)).BeginInit();
            this.SuspendLayout();
            // 
            // returnPictureBox
            // 
            this.returnPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.returnPictureBox.BackColor = System.Drawing.SystemColors.Control;
            this.returnPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.returnPictureBox.Image = global::MoveCursorByHand.Properties.Resources.maximize;
            this.returnPictureBox.Location = new System.Drawing.Point(270, 26);
            this.returnPictureBox.Name = "returnPictureBox";
            this.returnPictureBox.Size = new System.Drawing.Size(50, 37);
            this.returnPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.returnPictureBox.TabIndex = 0;
            this.returnPictureBox.TabStop = false;
            this.returnPictureBox.Click += new System.EventHandler(this.returnPictureBox_Click);
            // 
            // captureImageBoxMinimized
            // 
            this.captureImageBoxMinimized.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.captureImageBoxMinimized.Location = new System.Drawing.Point(0, 26);
            this.captureImageBoxMinimized.Name = "captureImageBoxMinimized";
            this.captureImageBoxMinimized.Size = new System.Drawing.Size(320, 118);
            this.captureImageBoxMinimized.TabIndex = 2;
            this.captureImageBoxMinimized.TabStop = false;
            // 
            // exitButton
            // 
            this.exitButton.AutoSize = true;
            this.exitButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.exitButton.Depth = 0;
            this.exitButton.Icon = null;
            this.exitButton.Location = new System.Drawing.Point(269, 108);
            this.exitButton.MouseState = MaterialSkin.MouseState.HOVER;
            this.exitButton.Name = "exitButton";
            this.exitButton.Primary = true;
            this.exitButton.Size = new System.Drawing.Size(50, 36);
            this.exitButton.TabIndex = 3;
            this.exitButton.Text = "EXIT";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Visible = false;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "notifyIcon";
            // 
            // MinimizedForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 144);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.returnPictureBox);
            this.Controls.Add(this.captureImageBoxMinimized);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MinimizedForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Sizable = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MinimizedForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MinimizedForm_FormClosed);
            this.Load += new System.EventHandler(this.MinimizedForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.returnPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.captureImageBoxMinimized)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox returnPictureBox;
        private Emgu.CV.UI.ImageBox captureImageBoxMinimized;
        private MaterialSkin.Controls.MaterialRaisedButton exitButton;
        private System.Windows.Forms.NotifyIcon notifyIcon;
    }
}