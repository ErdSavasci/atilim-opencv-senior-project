using MaterialSkin.Controls;
using MoveCursorByHand.App_Code;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MoveCursorByHand
{
    public partial class MinimizedForm : MaterialForm
    {
        private static MinimizedForm minimizedFormInstance;
        private Camera camera;
        private ScreenProperties screenProperties;        

        public MinimizedForm()
        {
            InitializeComponent();                    
        }

        public static MinimizedForm GetInstance()
        {
            if (minimizedFormInstance == null)
            {
                minimizedFormInstance = new MinimizedForm();
            }
            return minimizedFormInstance;
        }

        private void MinimizedForm_Load(object sender, EventArgs e)
        {
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Exit");
            contextMenuStrip.ItemClicked += ContextMenuStrip_ItemClicked;

            returnPictureBox.BackColor = ColorTranslator.FromHtml("#1976D2");
            notifyIcon.Visible = true;
            notifyIcon.Icon = Properties.Resources.hovercursor;
            notifyIcon.BalloonTipTitle = "Control is activated";
            notifyIcon.BalloonTipText = "Now, you can control your screen";
            notifyIcon.Text = "HoverMouse © 2017";
            //notifyIcon.ShowBalloonTip(1000);

            notifyIcon.ContextMenuStrip = contextMenuStrip;

            screenProperties = new ScreenProperties(this);
            Location = new Point(screenProperties.getWidth() - Width, screenProperties.getHeight() - Height);

            Devices devices = new Devices();
            captureImageBoxMinimized.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void ContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if(e.ClickedItem.Text.Equals("Exit"))
            {
                Application.Exit(null);
            }
        }

        public void SetCamera(Camera camera)
        {
            this.camera = camera;
            camera.ChangeImageBox(captureImageBoxMinimized);
            camera.Start();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            camera.Stop();
            camera.ReleaseResources();
            Application.Exit(null);
        }

        private void returnPictureBox_Click(object sender, EventArgs e)
        {
            if (camera.IsActive())
            {
                camera.Pause();
            }

            Hide();       
            MainForm mainForm = Application.OpenForms.OfType<MainForm>().First();
            camera.SetIsActivated(false);
            mainForm.SetCamera(camera);           
            mainForm.Show();

            ScreenProperties screenProperties = new ScreenProperties(this);

            Process currentProcess = Process.GetCurrentProcess();
            IntPtr hWnd = currentProcess.MainWindowHandle;
            if (hWnd != IntPtr.Zero)
            {
                Native.SetForegroundWindow(hWnd);
                Native.ShowWindow(hWnd, 9);
                Native.MoveWindow(hWnd, screenProperties.getWidth() / 2 - (mainForm.Width / 2), screenProperties.getHeight() / 2 - (mainForm.Height / 2), mainForm.Width, mainForm.Height, true);
            }

            notifyIcon.Visible = false;
        }

        public NotifyIcon getNotifyIcon()
        {
            return notifyIcon;
        }

        private void MinimizedForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (camera.IsActive())
            {
                camera.Stop();
                camera.ReleaseResources();
            }

            if (Application.OpenForms.Count > 1)
            {
                try
                {
                    Environment.Exit(Environment.ExitCode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Application.Exit(null);                   
                }
            }
        }

        private void MinimizedForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }       
    }
}
