namespace MoveCursorByHand
{
    partial class MainForm
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
            this.exitButton = new MaterialSkin.Controls.MaterialRaisedButton();
            this.captureImageBox = new Emgu.CV.UI.ImageBox();
            this.availableCamerasListView = new MaterialSkin.Controls.MaterialListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.rightRadioButton = new MaterialSkin.Controls.MaterialRadioButton();
            this.leftRadioButton = new MaterialSkin.Controls.MaterialRadioButton();
            this.macroComboBox3 = new MetroFramework.Controls.MetroComboBox();
            this.metroLink = new MetroFramework.Controls.MetroLink();
            this.backgroundPictureBox = new System.Windows.Forms.PictureBox();
            this.macroComboBox2 = new MetroFramework.Controls.MetroComboBox();
            this.macroComboBox = new MetroFramework.Controls.MetroComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.optionsPictureBox = new System.Windows.Forms.PictureBox();
            this.loadingGIFPicureBox = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.handOverlayPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.captureImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.backgroundPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.optionsPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.loadingGIFPicureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.handOverlayPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.AutoSize = true;
            this.exitButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.exitButton.Depth = 0;
            this.exitButton.Icon = null;
            this.exitButton.Location = new System.Drawing.Point(722, 413);
            this.exitButton.MouseState = MaterialSkin.MouseState.HOVER;
            this.exitButton.Name = "exitButton";
            this.exitButton.Primary = true;
            this.exitButton.Size = new System.Drawing.Size(50, 36);
            this.exitButton.TabIndex = 0;
            this.exitButton.Text = "Exıt";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // captureImageBox
            // 
            this.captureImageBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.captureImageBox.Location = new System.Drawing.Point(12, 73);
            this.captureImageBox.Name = "captureImageBox";
            this.captureImageBox.Size = new System.Drawing.Size(432, 376);
            this.captureImageBox.TabIndex = 2;
            this.captureImageBox.TabStop = false;
            // 
            // availableCamerasListView
            // 
            this.availableCamerasListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.availableCamerasListView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.availableCamerasListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.availableCamerasListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.availableCamerasListView.Depth = 0;
            this.availableCamerasListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F);
            this.availableCamerasListView.FullRowSelect = true;
            this.availableCamerasListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.availableCamerasListView.HideSelection = false;
            this.availableCamerasListView.Location = new System.Drawing.Point(585, 73);
            this.availableCamerasListView.MouseLocation = new System.Drawing.Point(-1, -1);
            this.availableCamerasListView.MouseState = MaterialSkin.MouseState.OUT;
            this.availableCamerasListView.MultiSelect = false;
            this.availableCamerasListView.Name = "availableCamerasListView";
            this.availableCamerasListView.OwnerDraw = true;
            this.availableCamerasListView.Size = new System.Drawing.Size(187, 137);
            this.availableCamerasListView.TabIndex = 3;
            this.availableCamerasListView.UseCompatibleStateImageBehavior = false;
            this.availableCamerasListView.View = System.Windows.Forms.View.Details;
            this.availableCamerasListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.availableCamerasListView_ItemChecked);
            this.availableCamerasListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.availableCamerasListView_ItemSelectionChanged);
            this.availableCamerasListView.SelectedIndexChanged += new System.EventHandler(this.availableCamerasListView_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Available Cameras";
            this.columnHeader1.Width = 182;
            // 
            // rightRadioButton
            // 
            this.rightRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.rightRadioButton.AutoSize = true;
            this.rightRadioButton.Depth = 0;
            this.rightRadioButton.Font = new System.Drawing.Font("Roboto", 10F);
            this.rightRadioButton.Location = new System.Drawing.Point(457, 412);
            this.rightRadioButton.Margin = new System.Windows.Forms.Padding(0);
            this.rightRadioButton.MouseLocation = new System.Drawing.Point(-1, -1);
            this.rightRadioButton.MouseState = MaterialSkin.MouseState.HOVER;
            this.rightRadioButton.Name = "rightRadioButton";
            this.rightRadioButton.Ripple = true;
            this.rightRadioButton.Size = new System.Drawing.Size(61, 30);
            this.rightRadioButton.TabIndex = 5;
            this.rightRadioButton.TabStop = true;
            this.rightRadioButton.Text = "Right";
            this.rightRadioButton.UseVisualStyleBackColor = true;
            this.rightRadioButton.CheckedChanged += new System.EventHandler(this.rightRadioButton_CheckedChanged);
            // 
            // leftRadioButton
            // 
            this.leftRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.leftRadioButton.AutoSize = true;
            this.leftRadioButton.Depth = 0;
            this.leftRadioButton.Font = new System.Drawing.Font("Roboto", 10F);
            this.leftRadioButton.Location = new System.Drawing.Point(457, 384);
            this.leftRadioButton.Margin = new System.Windows.Forms.Padding(0);
            this.leftRadioButton.MouseLocation = new System.Drawing.Point(-1, -1);
            this.leftRadioButton.MouseState = MaterialSkin.MouseState.HOVER;
            this.leftRadioButton.Name = "leftRadioButton";
            this.leftRadioButton.Ripple = true;
            this.leftRadioButton.Size = new System.Drawing.Size(53, 30);
            this.leftRadioButton.TabIndex = 6;
            this.leftRadioButton.TabStop = true;
            this.leftRadioButton.Text = "Left";
            this.leftRadioButton.UseVisualStyleBackColor = true;
            this.leftRadioButton.CheckedChanged += new System.EventHandler(this.leftRadioButton_CheckedChanged);
            // 
            // macroComboBox3
            // 
            this.macroComboBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.macroComboBox3.FormattingEnabled = true;
            this.macroComboBox3.ItemHeight = 23;
            this.macroComboBox3.Location = new System.Drawing.Point(660, 287);
            this.macroComboBox3.Name = "macroComboBox3";
            this.macroComboBox3.PromptText = "Select a key..";
            this.macroComboBox3.Size = new System.Drawing.Size(112, 29);
            this.macroComboBox3.TabIndex = 10;
            this.macroComboBox3.UseSelectable = true;
            // 
            // metroLink
            // 
            this.metroLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.metroLink.BackColor = System.Drawing.Color.Transparent;
            this.metroLink.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.metroLink.DisplayFocus = true;
            this.metroLink.Location = new System.Drawing.Point(722, 366);
            this.metroLink.Name = "metroLink";
            this.metroLink.Size = new System.Drawing.Size(50, 23);
            this.metroLink.Style = MetroFramework.MetroColorStyle.Red;
            this.metroLink.TabIndex = 7;
            this.metroLink.Text = "Help";
            this.metroLink.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroLink.UseCustomBackColor = true;
            this.metroLink.UseSelectable = true;
            this.metroLink.UseStyleColors = true;
            this.metroLink.Click += new System.EventHandler(this.metroLink_Click);
            // 
            // backgroundPictureBox
            // 
            this.backgroundPictureBox.BackgroundImage = global::MoveCursorByHand.Properties.Resources.background4;
            this.backgroundPictureBox.Location = new System.Drawing.Point(450, 73);
            this.backgroundPictureBox.Name = "backgroundPictureBox";
            this.backgroundPictureBox.Size = new System.Drawing.Size(322, 376);
            this.backgroundPictureBox.TabIndex = 14;
            this.backgroundPictureBox.TabStop = false;
            // 
            // macroComboBox2
            // 
            this.macroComboBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.macroComboBox2.FormattingEnabled = true;
            this.macroComboBox2.ItemHeight = 23;
            this.macroComboBox2.Location = new System.Drawing.Point(660, 252);
            this.macroComboBox2.Name = "macroComboBox2";
            this.macroComboBox2.PromptText = "Select a key..";
            this.macroComboBox2.Size = new System.Drawing.Size(112, 29);
            this.macroComboBox2.TabIndex = 15;
            this.macroComboBox2.UseSelectable = true;
            // 
            // macroComboBox
            // 
            this.macroComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.macroComboBox.FormattingEnabled = true;
            this.macroComboBox.ItemHeight = 23;
            this.macroComboBox.Location = new System.Drawing.Point(660, 216);
            this.macroComboBox.Name = "macroComboBox";
            this.macroComboBox.PromptText = "Select a key..";
            this.macroComboBox.Size = new System.Drawing.Size(112, 29);
            this.macroComboBox.TabIndex = 16;
            this.macroComboBox.UseSelectable = true;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Franklin Gothic Medium Cond", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(582, 292);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 20);
            this.label1.TabIndex = 17;
            this.label1.Text = "Macro #3";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Franklin Gothic Medium Cond", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(582, 221);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 20);
            this.label2.TabIndex = 18;
            this.label2.Text = "Macro #1";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Franklin Gothic Medium Cond", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(582, 258);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 20);
            this.label3.TabIndex = 19;
            this.label3.Text = "Macro #2";
            // 
            // optionsPictureBox
            // 
            this.optionsPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.optionsPictureBox.BackgroundImage = global::MoveCursorByHand.Properties.Resources.optionsPlate3;
            this.optionsPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.optionsPictureBox.Location = new System.Drawing.Point(449, 357);
            this.optionsPictureBox.Name = "optionsPictureBox";
            this.optionsPictureBox.Size = new System.Drawing.Size(103, 92);
            this.optionsPictureBox.TabIndex = 20;
            this.optionsPictureBox.TabStop = false;
            // 
            // loadingGIFPicureBox
            // 
            this.loadingGIFPicureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.loadingGIFPicureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.loadingGIFPicureBox.Image = global::MoveCursorByHand.Properties.Resources.loading;
            this.loadingGIFPicureBox.Location = new System.Drawing.Point(12, 73);
            this.loadingGIFPicureBox.Name = "loadingGIFPicureBox";
            this.loadingGIFPicureBox.Size = new System.Drawing.Size(397, 341);
            this.loadingGIFPicureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.loadingGIFPicureBox.TabIndex = 21;
            this.loadingGIFPicureBox.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Interval = 1500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // handOverlayPictureBox
            // 
            this.handOverlayPictureBox.Image = global::MoveCursorByHand.Properties.Resources.hand_overlay;
            this.handOverlayPictureBox.Location = new System.Drawing.Point(12, 73);
            this.handOverlayPictureBox.Name = "handOverlayPictureBox";
            this.handOverlayPictureBox.Size = new System.Drawing.Size(265, 243);
            this.handOverlayPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.handOverlayPictureBox.TabIndex = 22;
            this.handOverlayPictureBox.TabStop = false;
            this.handOverlayPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.handOverlayPictureBox_Paint);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.handOverlayPictureBox);
            this.Controls.Add(this.loadingGIFPicureBox);
            this.Controls.Add(this.rightRadioButton);
            this.Controls.Add(this.leftRadioButton);
            this.Controls.Add(this.optionsPictureBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.macroComboBox);
            this.Controls.Add(this.macroComboBox2);
            this.Controls.Add(this.macroComboBox3);
            this.Controls.Add(this.metroLink);
            this.Controls.Add(this.availableCamerasListView);
            this.Controls.Add(this.captureImageBox);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.backgroundPictureBox);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "HoverCursor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.VisibleChanged += new System.EventHandler(this.MainForm_VisibleChanged);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.captureImageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.backgroundPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.optionsPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.loadingGIFPicureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.handOverlayPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialRaisedButton exitButton;
        private Emgu.CV.UI.ImageBox captureImageBox;
        private MaterialSkin.Controls.MaterialListView availableCamerasListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private MaterialSkin.Controls.MaterialRadioButton rightRadioButton;
        private MaterialSkin.Controls.MaterialRadioButton leftRadioButton;
        private MetroFramework.Controls.MetroLink metroLink;
        private MetroFramework.Controls.MetroComboBox macroComboBox3;
        private System.Windows.Forms.PictureBox backgroundPictureBox;
        private MetroFramework.Controls.MetroComboBox macroComboBox2;
        private MetroFramework.Controls.MetroComboBox macroComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox optionsPictureBox;
        private System.Windows.Forms.PictureBox loadingGIFPicureBox;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.PictureBox handOverlayPictureBox;
    }
}

