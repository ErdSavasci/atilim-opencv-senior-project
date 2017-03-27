using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin.Controls;
using MaterialSkin;
using MoveCursorByHand.App_Code;
using System.Threading;
using DirectShowLib;
using System.Runtime.InteropServices;
using MetroFramework;

namespace MoveCursorByHand
{
    public partial class MainForm : MaterialForm
    {
        private static MainForm mainFormInstance = null;
        private Camera camera = null;
        private SynchronizationContext context = null;
        private bool itemSelectedManually = false;

        public MainForm()
        {
            InitializeComponent();  
        }

        public static MainForm GetInstance()
        {
            if(mainFormInstance == null)
            {
                mainFormInstance = new MainForm();
            }
            return mainFormInstance;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            captureImageBox.Tag = "Main_Capture";
            context = SynchronizationContext.Current;
            loadingGIFPicureBox.Visible = false;
            loadingGIFPicureBox.Size = new Size(captureImageBox.Width, captureImageBox.Height);
            loadingGIFPicureBox.Location = new Point(captureImageBox.Location.X, captureImageBox.Location.Y);

            backgroundPictureBox.Image = Properties.Resources.background4;
            backgroundPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            backgroundPictureBox.Location = new Point(0, 60);
            backgroundPictureBox.Size = new Size(Width, Height - 60);

            //metroLink.BackgroundImage = Properties.Resources.background4;
            //availableCamerasListView.BackgroundImage = Properties.Resources.background4;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            leftRadioButton.BackgroundImage = Properties.Resources.background4;

            MakeTransparent(label1);
            MakeTransparent(label2);
            MakeTransparent(label3);
            MakeTransparent(metroLink);
            MakeTransparent(availableCamerasListView);
            MakeTransparent(loadingGIFPicureBox);

            macroComboBox.Items.Add("ESC Key");
            macroComboBox.Items.Add("Shift Key");
            macroComboBox.Items.Add("Control Key");
            macroComboBox.Items.Add("Alt Key");
            macroComboBox.Items.Add("AltGr Key");
            macroComboBox.Items.Add("AltGr Key");
            macroComboBox.Items.Add("Up Key");
            macroComboBox.Items.Add("Down Key");
            macroComboBox.Items.Add("Right Key");
            macroComboBox.Items.Add("Left Key");
            macroComboBox.Items.Add("Prt Scr Key");
            macroComboBox.Items.Add("Num Lock Key");
            macroComboBox.Items.Add("Caps Lock Key");
            macroComboBox.Items.Add("Scroll Lock Key");
            macroComboBox.Items.Add("Page Up Key");
            macroComboBox.Items.Add("Page Down Key");

            macroComboBox2.Items.Add("ESC Key");
            macroComboBox2.Items.Add("Shift Key");
            macroComboBox2.Items.Add("Control Key");
            macroComboBox2.Items.Add("Alt Key");
            macroComboBox2.Items.Add("AltGr Key");
            macroComboBox2.Items.Add("AltGr Key");
            macroComboBox2.Items.Add("Up Key");
            macroComboBox2.Items.Add("Down Key");
            macroComboBox2.Items.Add("Right Key");
            macroComboBox2.Items.Add("Left Key");
            macroComboBox2.Items.Add("Prt Scr Key");
            macroComboBox2.Items.Add("Num Lock Key");
            macroComboBox2.Items.Add("Caps Lock Key");
            macroComboBox2.Items.Add("Scroll Lock Key");
            macroComboBox2.Items.Add("Page Up Key");
            macroComboBox2.Items.Add("Page Down Key");

            macroComboBox3.Items.Add("ESC Key");
            macroComboBox3.Items.Add("Shift Key");
            macroComboBox3.Items.Add("Control Key");
            macroComboBox3.Items.Add("Alt Key");
            macroComboBox3.Items.Add("AltGr Key");
            macroComboBox3.Items.Add("AltGr Key");
            macroComboBox3.Items.Add("Up Key");
            macroComboBox3.Items.Add("Down Key");
            macroComboBox3.Items.Add("Right Key");
            macroComboBox3.Items.Add("Left Key");
            macroComboBox3.Items.Add("Prt Scr Key");
            macroComboBox3.Items.Add("Num Lock Key");
            macroComboBox3.Items.Add("Caps Lock Key");
            macroComboBox3.Items.Add("Scroll Lock Key");
            macroComboBox3.Items.Add("Page Up Key");
            macroComboBox3.Items.Add("Page Down Key");

            MaterialSkinManager materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Blue700, Primary.Blue900, Primary.Blue500, Accent.Green700, TextShade.WHITE);            

            //Fills the listview elements with video capture device names
            int cameraIndex = -1;
            Devices devices = new Devices();
            availableCamerasListView.Items.Clear();
            for (int i = 0; i < devices.Count(); i++)
            {                
                availableCamerasListView.Items.Add(new ListViewItem(devices.ElementAt(i).Name));
                cameraIndex = 0;             
            }

            captureImageBox.SizeMode = PictureBoxSizeMode.StretchImage;
            captureImageBox.VerticalScrollBar.Visible = false;
            captureImageBox.HorizontalScrollBar.Visible = false;
            captureImageBox.Width = (int)(Width / 1.81);
            camera = new Camera(captureImageBox, devices.First(), cameraIndex);
            camera.Start();

            //Initially check right radio button
            rightRadioButton.Checked = true;
        }

        private void MakeTransparent(Control control)
        {
            Thread t = new Thread(() =>
            {
                Action action = () =>
                {
                    control.Visible = false;

                    control.Refresh();
                    Application.DoEvents();

                    Rectangle screenRectangle = RectangleToScreen(ClientRectangle);
                    int titleHeight = screenRectangle.Top - Top;
                    int right = screenRectangle.Left - Left;

                    Bitmap bitmap = new Bitmap(Width, Height);
                    DrawToBitmap(bitmap, new Rectangle(0, 0, Width, Height));
                    Bitmap bitmapImage = new Bitmap(bitmap);
                    bitmap = bitmapImage.Clone(new Rectangle(control.Location.X + right, control.Location.Y, control.Width, control.Height), bitmapImage.PixelFormat);

                    control.BackgroundImage = bitmap;

                    control.Visible = true;
                };
                control.Invoke(action);
            });
            if(control.Width > 0 && control.Height > 0)
                t.Start();
        }

        public void SetCamera(Camera camera)
        {
            this.camera = camera;
            camera.changeImageBox(captureImageBox);
            camera.Start();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            if (camera.isActive())
            {
                camera.Stop();
                camera.ReleaseResources();
            }
            Application.Exit(null);
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            captureImageBox.SizeMode = PictureBoxSizeMode.StretchImage;
            captureImageBox.VerticalScrollBar.Maximum = 0;
            captureImageBox.HorizontalScrollBar.Maximum = 0;
            captureImageBox.VerticalScrollBar.Visible = false;
            captureImageBox.HorizontalScrollBar.Visible = false;
            captureImageBox.VerticalScrollBar.Enabled = false;
            captureImageBox.HorizontalScrollBar.Enabled = false;
            captureImageBox.VerticalScrollBar.Size = new Size(0, 0);
            captureImageBox.HorizontalScrollBar.Size = new Size(0, 0);
            captureImageBox.Height = (int)(Height / 1.22);
            captureImageBox.Width = (int) (Width / 1.81);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            backgroundPictureBox.Size = new Size(Width, Height - 60);

            MakeTransparent(label1);
            MakeTransparent(label2);
            MakeTransparent(label3);
            MakeTransparent(metroLink);
            MakeTransparent(availableCamerasListView);
            MakeTransparent(loadingGIFPicureBox);

            if (WindowState == FormWindowState.Minimized)
            {
                if (camera.isActive())
                {
                    camera.Pause();
                }

                Hide();
                MinimizedForm form = Application.OpenForms.OfType<MinimizedForm>().Count() > 0 ? Application.OpenForms.OfType<MinimizedForm>().First() : new MinimizedForm();
                form.TopMost = true;                
                camera.SetIsActivated(true);
                form.SetCamera(camera);
                form.Show();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (camera.isActive())
            {
                camera.Stop();
                camera.ReleaseResources();
            }

            if (Application.OpenForms.Count > 1)
                Application.Exit(null);
        }

        private void MainForm_VisibleChanged(object sender, EventArgs e)
        {           
            ScreenProperties screenProperties = new ScreenProperties(this);
            Location = new Point(screenProperties.getWidth() / 2 - (Width / 2), screenProperties.getHeight() / 2 - (Height / 2));         
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void leftRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            camera.ChangeHandPosition("Left");
        }

        private void rightRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            camera.ChangeHandPosition("Right");
        }

        private void metroLink_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "When you show your hand with 5 fingers open, then the application will be minimized and the mouse control with hand gestures will be started.", "Help Page", MessageBoxButtons.OK, MessageBoxIcon.Information, (int)(Height / 1.5));
        }

        private void availableCamerasListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("SelectedIndexChanged");
        }

        private void availableCamerasListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            Console.WriteLine("ItemChecked");
        }

        private void availableCamerasListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if(camera.getActiveDeviceIndex() == e.ItemIndex && !itemSelectedManually)
            {               
                loadingGIFPicureBox.Image = Properties.Resources.loading;
                loadingGIFPicureBox.Tag = e.ItemIndex;
                loadingGIFPicureBox.Visible = true;
                timer1.Start();
                itemSelectedManually = true;
            }           
        }

        public void clearLoadingAnimationPictureBox()
        {
            Thread clearThread = new Thread(() =>
            {
                Action clearImageAction = () => {
                    loadingGIFPicureBox.Visible = false;
                    loadingGIFPicureBox.Image = null;                   
                };
                loadingGIFPicureBox.Invoke(clearImageAction);
            });
            clearThread.Start();
            itemSelectedManually = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            if (camera.isActive())
            {
                camera.Stop();
                camera.ReleaseResources();
                camera = null;
            }

            Devices devices = new Devices();
            camera = new Camera(captureImageBox, devices.ElementAt((int)loadingGIFPicureBox.Tag), (int)loadingGIFPicureBox.Tag);
            camera.setFirstFrameCaptured(true);
            SetCamera(camera);

            itemSelectedManually = true;            
        }
    }
}
