using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using MaterialSkin;
using MoveCursorByHand.App_Code;
using System.Threading;
using MetroFramework;
using System.Drawing.Imaging;

namespace MoveCursorByHand
{
    public partial class MainForm : MaterialForm
    {
        private static MainForm mainFormInstance = null;
        private Camera camera = null;
        private SynchronizationContext context = null;
        private bool normalTransparent = false, maximizedTransparent = false;
        private Thread handOverlayThread = null;
        private Bitmap handOverlayBitmap = null;
        private bool abort = false;
        private double opacityValue = 0.20;
        private bool isAscending = false;

        public MainForm()
        {
            InitializeComponent();
        }

        public static MainForm GetInstance()
        {
            if (mainFormInstance == null)
            {
                mainFormInstance = new MainForm();
            }
            return mainFormInstance;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            captureImageBox.Tag = "Main_Capture";
            context = SynchronizationContext.Current;
            loadingGIFPictureBox.Visible = false;
            loadingGIFPictureBox.Size = new Size(captureImageBox.Width, captureImageBox.Height);
            loadingGIFPictureBox.Location = new Point(captureImageBox.Location.X, captureImageBox.Location.Y);

            backgroundPictureBox.Image = Properties.Resources.background4;
            backgroundPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            backgroundPictureBox.Location = new Point(0, 60);
            backgroundPictureBox.Size = new Size(Width, Height - 60);

            //metroLink.BackgroundImage = Properties.Resources.background4;
            //availableCamerasListView.BackgroundImage = Properties.Resources.background4;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            handOverlayBitmap = new Bitmap(Properties.Resources.hand_overlay);
            handOverlayPictureBox.Parent = captureImageBox;
            handOverlayPictureBox.BackColor = Color.Transparent;

            macroComboBox.Items.Add("ESC Key");
            macroComboBox.Items.Add("LSHIFT Key");
            macroComboBox.Items.Add("LCTRL Key");
            macroComboBox.Items.Add("LALT Key");
            macroComboBox.Items.Add("RALT Key");
            macroComboBox.Items.Add("UP Key");
            macroComboBox.Items.Add("DOWN Key");
            macroComboBox.Items.Add("RIGHT Key");
            macroComboBox.Items.Add("LEFT Key");
            macroComboBox.Items.Add("PRTSRC Key");
            macroComboBox.Items.Add("NUMLOCK Key");
            macroComboBox.Items.Add("CAPSLOCK Key");
            macroComboBox.Items.Add("SCROLLLOCK Key");
            macroComboBox.Items.Add("PGUP Key");
            macroComboBox.Items.Add("PDN Key");
            macroComboBox.Items.Add("APPSKEY Key");

            macroComboBox2.Items.Add("ESC Key");
            macroComboBox2.Items.Add("LSHIFT Key");
            macroComboBox2.Items.Add("LCTRL Key");
            macroComboBox2.Items.Add("LALT Key");
            macroComboBox2.Items.Add("RALT Key");
            macroComboBox2.Items.Add("UP Key");
            macroComboBox2.Items.Add("DOWN Key");
            macroComboBox2.Items.Add("RIGHT Key");
            macroComboBox2.Items.Add("LEFT Key");
            macroComboBox2.Items.Add("PRTSRC Key");
            macroComboBox2.Items.Add("NUMLOCK Key");
            macroComboBox2.Items.Add("CAPSLOCK Key");
            macroComboBox2.Items.Add("SCROLLLOCK Key");
            macroComboBox2.Items.Add("PGUP Key");
            macroComboBox2.Items.Add("PDN Key");
            macroComboBox2.Items.Add("APPSKEY Key");

            macroComboBox3.Items.Add("ESC Key");
            macroComboBox3.Items.Add("LSHIFT Key");
            macroComboBox3.Items.Add("LCTRL Key");
            macroComboBox3.Items.Add("LALT Key");
            macroComboBox3.Items.Add("RALT Key");
            macroComboBox3.Items.Add("UP Key");
            macroComboBox3.Items.Add("DOWN Key");
            macroComboBox3.Items.Add("RIGHT Key");
            macroComboBox3.Items.Add("LEFT Key");
            macroComboBox3.Items.Add("PRTSRC Key");
            macroComboBox3.Items.Add("NUMLOCK Key");
            macroComboBox3.Items.Add("CAPSLOCK Key");
            macroComboBox3.Items.Add("SCROLLLOCK Key");
            macroComboBox3.Items.Add("PGUP Key");
            macroComboBox3.Items.Add("PDN Key");
            macroComboBox3.Items.Add("APPSKEY Key");

            MaterialSkinManager materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Blue700, Primary.Blue900, Primary.Blue500, Accent.Green700, TextShade.WHITE);

            captureImageBox.SizeMode = PictureBoxSizeMode.StretchImage;
            captureImageBox.VerticalScrollBar.Visible = false;
            captureImageBox.HorizontalScrollBar.Visible = false;
            captureImageBox.Width = (int)(Width / 1.81);

            captureImageBox.Visible = false;
            loadingGIFPictureBox.Visible = true;

            //Fills the listview elements with video capture device names
            int cameraIndex = -1;
            Devices devices = new Devices();
            availableCamerasListView.Items.Clear();
            for (int i = 0; i < devices.Count(); i++)
            {
                availableCamerasListView.Items.Add(new ListViewItem(devices.ElementAt(i).Name));
                cameraIndex = 0;
            }

            camera = new Camera(captureImageBox, devices.First(), cameraIndex, handOverlayPictureBox, loadingGIFPictureBox, new[] { macroComboBox.SelectedIndex != -1 ? (string)macroComboBox.SelectedItem : null, macroComboBox2.SelectedIndex != -1 ? (string)macroComboBox2.SelectedItem : null, macroComboBox3.SelectedIndex != -1 ? (string)macroComboBox3.SelectedItem : null });
            camera.Start();

            handOverlayThread = new Thread(() =>
            {
                while (camera == null) ;
                while (!camera.IsActive()) ;
                SetLocationAndSizeOfHandPictureBox();
            });
            handOverlayThread.IsBackground = true;
            handOverlayThread.Start();
            ChangeOpacityRepeatedly();

            //Initially check right radio button
            rightRadioButton.Checked = true;
        }

        private void MakeTransparent(Control control)
        {
            Thread t = new Thread(() =>
            {
                Action action = () =>
                {
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
                };
                control.Invoke(action);
            });
            t.IsBackground = true;
            if (control.Width > 0 && control.Height > 0)
                t.Start();
        }

        public void SetCamera(Camera camera)
        {
            this.camera = camera;
            camera.ChangeImageBox(captureImageBox);
            camera.Start();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            if (camera.IsActive())
            {
                camera.Stop();
                camera.ReleaseResources();
            }

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

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            backgroundPictureBox.Size = new Size(Width, Height - 60);
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
            captureImageBox.Width = (int)(Width / 1.81);

            optionsPictureBox.Size = new Size(103, 92);
            optionsPictureBox.Location = new Point(leftRadioButton.Location.X - 5, leftRadioButton.Location.Y - 25);

            if (camera != null)
            {
                SetLocationAndSizeOfHandPictureBox();
            }

            if (WindowState == FormWindowState.Maximized && !maximizedTransparent)
            {
                MakeTransparent(label1);
                MakeTransparent(label2);
                MakeTransparent(label3);
                MakeTransparent(metroLink);
                MakeTransparent(availableCamerasListView);
                MakeTransparent(loadingGIFPictureBox);
                normalTransparent = false;
                maximizedTransparent = true;
            }
            else if (WindowState == FormWindowState.Normal && !normalTransparent)
            {
                MakeTransparent(label1);
                MakeTransparent(label2);
                MakeTransparent(label3);
                MakeTransparent(metroLink);
                MakeTransparent(availableCamerasListView);
                MakeTransparent(loadingGIFPictureBox);
                normalTransparent = true;
                maximizedTransparent = false;
            }
        }

        private void ChangeOpacityRepeatedly()
        {
            Thread changeThread = new Thread(() =>
            {
                while (!abort)
                {
                    if (camera != null && camera.IsActive())
                    {
                        ChangeOpacityOfHandPictureBox(opacityValue);
                        Thread.Sleep(100);

                        if (isAscending)
                        {
                            opacityValue = opacityValue + 0.05;
                        }
                        else
                        {
                            opacityValue = opacityValue - 0.05;
                        }

                        if (opacityValue <= 0)
                            isAscending = true;
                        else if (opacityValue >= 0.20)
                            isAscending = false;
                    }
                }
            });
            changeThread.IsBackground = true;
            changeThread.Start();
        }

        private void ChangeOpacityOfHandPictureBox(double opacityValue)
        {
            Bitmap newImage = new Bitmap(handOverlayBitmap.Width, handOverlayBitmap.Height);
            Graphics graphics = Graphics.FromImage(newImage);
            ColorMatrix colormatrix = new ColorMatrix();
            colormatrix.Matrix33 = (float)opacityValue;
            ImageAttributes imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            graphics.DrawImage(handOverlayBitmap, new Rectangle(0, 0, handOverlayBitmap.Width, handOverlayBitmap.Height), 0, 0, handOverlayBitmap.Width, handOverlayBitmap.Height, GraphicsUnit.Pixel, imgAttribute);
            Action action = new Action(() => { handOverlayPictureBox.Image = newImage; });
            handOverlayPictureBox.Invoke(action);
            graphics.Dispose();
        }

        private void SetLocationAndSizeOfHandPictureBox()
        {
            if (InvokeRequired)
            {
                this.Invoke(new MethodInvoker(SetLocationAndSizeOfHandPictureBox));
            }

            if (rightRadioButton.Checked && camera.IsActive())
            {
                Size frameSize = camera.GetFrameSize();
                double widthDiff = (double)captureImageBox.Width / frameSize.Width;
                double heightDiff = (double)captureImageBox.Height / frameSize.Height;
                int croppedFrameX = frameSize.Width - (frameSize.Width / 26) - (frameSize.Width / 2);
                int croppedFrameY = frameSize.Width / 26;
                int croppedFrameWidth = frameSize.Width / 2;
                int croppedFrameHeight = frameSize.Height / 2;

                handOverlayPictureBox.Size = new Size((int)(frameSize.Width / 2 * widthDiff), (int)(frameSize.Height / 2 * heightDiff));

                int extraAreaX = (int)((croppedFrameWidth / 40) * widthDiff);
                int extraAreaY = (int)((croppedFrameHeight / 8) * heightDiff);      

                handOverlayPictureBox.Location = new Point((int)(croppedFrameX * widthDiff + extraAreaX), (int)(croppedFrameY * heightDiff + extraAreaY));
            }
            else if (camera.IsActive())
            {
                Size frameSize = camera.GetFrameSize();
                double widthDiff = (double)captureImageBox.Width / frameSize.Width;
                double heightDiff = (double)captureImageBox.Height / frameSize.Height;
                int croppedFrameX = frameSize.Width / 26;
                int croppedFrameY = frameSize.Width / 26;
                int croppedFrameWidth = frameSize.Width / 2;
                int croppedFrameHeight = frameSize.Height / 2;

                handOverlayPictureBox.Size = new Size((int)(frameSize.Width / 2 * widthDiff), (int)(frameSize.Height / 2 * heightDiff));

                int extraAreaX = (int)((croppedFrameWidth / 40) * widthDiff);
                int extraAreaY = (int)((croppedFrameHeight / 8) * heightDiff);

                handOverlayPictureBox.Location = new Point((int)(croppedFrameX * widthDiff + extraAreaX), (int)(croppedFrameY * heightDiff + extraAreaY));
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                if (camera.IsActive())
                {
                    camera.Pause();
                }

                Hide();
                MinimizedForm form = Application.OpenForms.OfType<MinimizedForm>().Count() > 0 ? Application.OpenForms.OfType<MinimizedForm>().First() : new MinimizedForm();
                form.TopMost = true;
                camera.SetIsActivated(false);
                form.SetCamera(camera);
                form.Show();

                normalTransparent = false;
                maximizedTransparent = false;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            abort = true;

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
            if (camera != null)
            {
                camera.ChangeHandPosition("Left");
                SetLocationAndSizeOfHandPictureBox();
            }
        }

        private void rightRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (camera != null)
            {
                camera.ChangeHandPosition("Right");
                SetLocationAndSizeOfHandPictureBox();
            }
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
            Devices devices = new Devices();

            if (devices.Count() > 1 && camera.GetActiveDeviceIndex() != e.ItemIndex)
            {
                SetLoadingAnimationPictureBox(e.ItemIndex);
                timer1.Start();
            }
        }

        public void ClearLoadingAnimationPictureBox()
        {
            Thread clearThread = new Thread(() =>
            {
                Action clearImageAction = () =>
                {
                    loadingGIFPictureBox.Visible = false;
                    loadingGIFPictureBox.Image = null;
                };
                loadingGIFPictureBox.Invoke(clearImageAction);
            });
            clearThread.IsBackground = true;
            clearThread.Start();
        }

        public void SetLoadingAnimationPictureBox(int itemIndex)
        {
            Thread setThread = new Thread(() =>
            {
                Action setImageAction = () =>
                {
                    loadingGIFPictureBox.Tag = itemIndex;
                    loadingGIFPictureBox.Image = Properties.Resources.hand_overlay;
                    loadingGIFPictureBox.Visible = true;
                };
                loadingGIFPictureBox.Invoke(setImageAction);
            });
            setThread.IsBackground = true;
            setThread.Start();
        }

        private void handOverlayPictureBox_Paint(object sender, PaintEventArgs e)
        {
            Image handOverlayBitmap = ((PictureBox)sender).Image;
            Graphics graphics = Graphics.FromImage(handOverlayBitmap);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {

        }

        private void macroComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            camera.SetMacroComboBoxSelections(new[] { macroComboBox.SelectedIndex != -1 ? (string)macroComboBox.SelectedItem : null, macroComboBox2.SelectedIndex != -1 ? (string)macroComboBox2.SelectedItem : null, macroComboBox3.SelectedIndex != -1 ? (string)macroComboBox3.SelectedItem : null });
        }

        private void macroComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            camera.SetMacroComboBoxSelections(new[] { macroComboBox.SelectedIndex != -1 ? (string)macroComboBox.SelectedItem : null, macroComboBox2.SelectedIndex != -1 ? (string)macroComboBox2.SelectedItem : null, macroComboBox3.SelectedIndex != -1 ? (string)macroComboBox3.SelectedItem : null });
        }

        private void macroComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            camera.SetMacroComboBoxSelections(new[] { macroComboBox.SelectedIndex != -1 ? (string)macroComboBox.SelectedItem : null, macroComboBox2.SelectedIndex != -1 ? (string)macroComboBox2.SelectedItem : null, macroComboBox3.SelectedIndex != -1 ? (string)macroComboBox3.SelectedItem : null });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            if (camera.IsActive())
            {
                camera.Stop();
                camera.ReleaseResources();
                camera = null;
            }

            Devices devices = new Devices();
            camera = new Camera(captureImageBox, devices.ElementAt((int)loadingGIFPictureBox.Tag), (int)loadingGIFPictureBox.Tag, handOverlayPictureBox, loadingGIFPictureBox, new[] { macroComboBox.SelectedIndex != -1 ? (string)macroComboBox.SelectedItem : null, macroComboBox2.SelectedIndex != -1 ? (string)macroComboBox2.SelectedItem : null, macroComboBox3.SelectedIndex != -1 ? (string)macroComboBox3.SelectedItem : null });
            SetCamera(camera);
        }
    }
}
