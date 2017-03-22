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
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.materialLabel2 = new MaterialSkin.Controls.MaterialLabel();
            this.materialLabel3 = new MaterialSkin.Controls.MaterialLabel();
            this.macroComboBox3 = new MetroFramework.Controls.MetroComboBox();
            this.macroComboBox2 = new MetroFramework.Controls.MetroComboBox();
            this.macroComboBox = new MetroFramework.Controls.MetroComboBox();
            this.metroLink = new MetroFramework.Controls.MetroLink();
            ((System.ComponentModel.ISupportInitialize)(this.captureImageBox)).BeginInit();
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
            this.rightRadioButton.AutoSize = true;
            this.rightRadioButton.Depth = 0;
            this.rightRadioButton.Font = new System.Drawing.Font("Roboto", 10F);
            this.rightRadioButton.Location = new System.Drawing.Point(457, 407);
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
            this.leftRadioButton.AutoSize = true;
            this.leftRadioButton.Depth = 0;
            this.leftRadioButton.Font = new System.Drawing.Font("Roboto", 10F);
            this.leftRadioButton.Location = new System.Drawing.Point(457, 366);
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
            // materialLabel1
            // 
            this.materialLabel1.AutoSize = true;
            this.materialLabel1.Depth = 0;
            this.materialLabel1.Font = new System.Drawing.Font("Roboto", 11F);
            this.materialLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel1.Location = new System.Drawing.Point(581, 220);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(73, 19);
            this.materialLabel1.TabIndex = 11;
            this.materialLabel1.Text = "Macro #1";
            // 
            // materialLabel2
            // 
            this.materialLabel2.AutoSize = true;
            this.materialLabel2.Depth = 0;
            this.materialLabel2.Font = new System.Drawing.Font("Roboto", 11F);
            this.materialLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel2.Location = new System.Drawing.Point(581, 257);
            this.materialLabel2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel2.Name = "materialLabel2";
            this.materialLabel2.Size = new System.Drawing.Size(73, 19);
            this.materialLabel2.TabIndex = 12;
            this.materialLabel2.Text = "Macro #2";
            // 
            // materialLabel3
            // 
            this.materialLabel3.AutoSize = true;
            this.materialLabel3.Depth = 0;
            this.materialLabel3.Font = new System.Drawing.Font("Roboto", 11F);
            this.materialLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel3.Location = new System.Drawing.Point(581, 292);
            this.materialLabel3.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel3.Name = "materialLabel3";
            this.materialLabel3.Size = new System.Drawing.Size(73, 19);
            this.materialLabel3.TabIndex = 13;
            this.materialLabel3.Text = "Macro #3";
            // 
            // macroComboBox3
            // 
            this.macroComboBox3.FormattingEnabled = true;
            this.macroComboBox3.ItemHeight = 23;
            this.macroComboBox3.Location = new System.Drawing.Point(660, 287);
            this.macroComboBox3.Name = "macroComboBox3";
            this.macroComboBox3.PromptText = "Select a key..";
            this.macroComboBox3.Size = new System.Drawing.Size(112, 29);
            this.macroComboBox3.TabIndex = 10;
            this.macroComboBox3.UseSelectable = true;
            // 
            // macroComboBox2
            // 
            this.macroComboBox2.FormattingEnabled = true;
            this.macroComboBox2.ItemHeight = 23;
            this.macroComboBox2.Location = new System.Drawing.Point(660, 252);
            this.macroComboBox2.Name = "macroComboBox2";
            this.macroComboBox2.PromptText = "Select a key..";
            this.macroComboBox2.Size = new System.Drawing.Size(112, 29);
            this.macroComboBox2.TabIndex = 9;
            this.macroComboBox2.UseSelectable = true;
            // 
            // macroComboBox
            // 
            this.macroComboBox.FormattingEnabled = true;
            this.macroComboBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.macroComboBox.ItemHeight = 23;
            this.macroComboBox.Location = new System.Drawing.Point(660, 216);
            this.macroComboBox.Name = "macroComboBox";
            this.macroComboBox.PromptText = "Select a key..";
            this.macroComboBox.Size = new System.Drawing.Size(112, 29);
            this.macroComboBox.TabIndex = 8;
            this.macroComboBox.Theme = MetroFramework.MetroThemeStyle.Light;
            this.macroComboBox.UseSelectable = true;
            // 
            // metroLink
            // 
            this.metroLink.DisplayFocus = true;
            this.metroLink.Location = new System.Drawing.Point(722, 366);
            this.metroLink.Name = "metroLink";
            this.metroLink.Size = new System.Drawing.Size(50, 23);
            this.metroLink.Style = MetroFramework.MetroColorStyle.Red;
            this.metroLink.TabIndex = 7;
            this.metroLink.Text = "Help";
            this.metroLink.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroLink.UseSelectable = true;
            this.metroLink.UseStyleColors = true;
            this.metroLink.Click += new System.EventHandler(this.metroLink_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.materialLabel3);
            this.Controls.Add(this.materialLabel2);
            this.Controls.Add(this.materialLabel1);
            this.Controls.Add(this.macroComboBox3);
            this.Controls.Add(this.macroComboBox2);
            this.Controls.Add(this.macroComboBox);
            this.Controls.Add(this.metroLink);
            this.Controls.Add(this.leftRadioButton);
            this.Controls.Add(this.rightRadioButton);
            this.Controls.Add(this.availableCamerasListView);
            this.Controls.Add(this.captureImageBox);
            this.Controls.Add(this.exitButton);
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
        private MetroFramework.Controls.MetroComboBox macroComboBox;
        private MetroFramework.Controls.MetroComboBox macroComboBox2;
        private MetroFramework.Controls.MetroComboBox macroComboBox3;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private MaterialSkin.Controls.MaterialLabel materialLabel2;
        private MaterialSkin.Controls.MaterialLabel materialLabel3;
    }
}

