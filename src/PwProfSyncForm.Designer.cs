namespace PasswordChangeAssistant
{
	partial class PwProfSyncForm
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
            this.lError = new System.Windows.Forms.Label();
            this.tlpProfiles = new System.Windows.Forms.TableLayoutPanel();
            this.lbProfilesDB = new System.Windows.Forms.ListBox();
            this.cbModeCopy = new System.Windows.Forms.CheckBox();
            this.lActiveDB = new System.Windows.Forms.Label();
            this.cbProfileOther = new System.Windows.Forms.ComboBox();
            this.pButtons = new System.Windows.Forms.Panel();
            this.bDB2Other = new System.Windows.Forms.Button();
            this.pbPluginPicture = new System.Windows.Forms.PictureBox();
            this.bOther2DB = new System.Windows.Forms.Button();
            this.lbProfilesOther = new System.Windows.Forms.ListBox();
            this.tpOptions = new System.Windows.Forms.TabControl();
            this.tpPCA = new System.Windows.Forms.TabPage();
            this.gHideBuildInProfiles = new System.Windows.Forms.GroupBox();
            this.cbHideBuiltInProfiles = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.gPWFormShown = new System.Windows.Forms.GroupBox();
            this.lAutotypeDelay = new System.Windows.Forms.Label();
            this.nupAutotypeDelay = new System.Windows.Forms.NumericUpDown();
            this.lOpenUrlForPwChangeShift = new System.Windows.Forms.Label();
            this.cbOpenUrlForPwChange = new System.Windows.Forms.CheckBox();
            this.tpPWSync = new System.Windows.Forms.TabPage();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.tlpProfiles.SuspendLayout();
            this.pButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPluginPicture)).BeginInit();
            this.tpOptions.SuspendLayout();
            this.tpPCA.SuspendLayout();
            this.gHideBuildInProfiles.SuspendLayout();
            this.gPWFormShown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nupAutotypeDelay)).BeginInit();
            this.tpPWSync.SuspendLayout();
            this.SuspendLayout();
            // 
            // lError
            // 
            this.lError.AutoSize = true;
            this.lError.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lError.ForeColor = System.Drawing.Color.Red;
            this.lError.Location = new System.Drawing.Point(53, 34);
            this.lError.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lError.Name = "lError";
            this.lError.Size = new System.Drawing.Size(79, 31);
            this.lError.TabIndex = 15;
            this.lError.Text = "Error";
            // 
            // tlpProfiles
            // 
            this.tlpProfiles.ColumnCount = 3;
            this.tlpProfiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpProfiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 256F));
            this.tlpProfiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpProfiles.Controls.Add(this.lbProfilesDB, 0, 1);
            this.tlpProfiles.Controls.Add(this.cbModeCopy, 0, 2);
            this.tlpProfiles.Controls.Add(this.lActiveDB, 0, 0);
            this.tlpProfiles.Controls.Add(this.cbProfileOther, 2, 0);
            this.tlpProfiles.Controls.Add(this.pButtons, 1, 1);
            this.tlpProfiles.Controls.Add(this.lbProfilesOther, 2, 1);
            this.tlpProfiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpProfiles.Location = new System.Drawing.Point(3, 3);
            this.tlpProfiles.Margin = new System.Windows.Forms.Padding(5);
            this.tlpProfiles.Name = "tlpProfiles";
            this.tlpProfiles.RowCount = 3;
            this.tlpProfiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            this.tlpProfiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpProfiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 93F));
            this.tlpProfiles.Size = new System.Drawing.Size(1173, 772);
            this.tlpProfiles.TabIndex = 0;
            // 
            // lbProfilesDB
            // 
            this.lbProfilesDB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbProfilesDB.FormattingEnabled = true;
            this.lbProfilesDB.IntegralHeight = false;
            this.lbProfilesDB.ItemHeight = 31;
            this.lbProfilesDB.Location = new System.Drawing.Point(0, 62);
            this.lbProfilesDB.Margin = new System.Windows.Forms.Padding(0);
            this.lbProfilesDB.Name = "lbProfilesDB";
            this.lbProfilesDB.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbProfilesDB.Size = new System.Drawing.Size(458, 617);
            this.lbProfilesDB.Sorted = true;
            this.lbProfilesDB.TabIndex = 1;
            // 
            // cbModeCopy
            // 
            this.cbModeCopy.AutoSize = true;
            this.tlpProfiles.SetColumnSpan(this.cbModeCopy, 3);
            this.cbModeCopy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbModeCopy.Location = new System.Drawing.Point(0, 679);
            this.cbModeCopy.Margin = new System.Windows.Forms.Padding(0);
            this.cbModeCopy.Name = "cbModeCopy";
            this.cbModeCopy.Size = new System.Drawing.Size(1173, 93);
            this.cbModeCopy.TabIndex = 5;
            // 
            // lActiveDB
            // 
            this.lActiveDB.AutoSize = true;
            this.tlpProfiles.SetColumnSpan(this.lActiveDB, 2);
            this.lActiveDB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lActiveDB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lActiveDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lActiveDB.Location = new System.Drawing.Point(5, 5);
            this.lActiveDB.Margin = new System.Windows.Forms.Padding(5);
            this.lActiveDB.Name = "lActiveDB";
            this.lActiveDB.Size = new System.Drawing.Size(704, 52);
            this.lActiveDB.TabIndex = 12;
            this.lActiveDB.Text = "Active database";
            // 
            // cbProfileOther
            // 
            this.cbProfileOther.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbProfileOther.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProfileOther.FormattingEnabled = true;
            this.cbProfileOther.Location = new System.Drawing.Point(719, 5);
            this.cbProfileOther.Margin = new System.Windows.Forms.Padding(5);
            this.cbProfileOther.Name = "cbProfileOther";
            this.cbProfileOther.Size = new System.Drawing.Size(449, 39);
            this.cbProfileOther.TabIndex = 0;
            this.cbProfileOther.SelectedIndexChanged += new System.EventHandler(this.profileOther_SelectedIndexChanged);
            // 
            // pButtons
            // 
            this.pButtons.Controls.Add(this.bDB2Other);
            this.pButtons.Controls.Add(this.pbPluginPicture);
            this.pButtons.Controls.Add(this.bOther2DB);
            this.pButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pButtons.Location = new System.Drawing.Point(458, 62);
            this.pButtons.Margin = new System.Windows.Forms.Padding(0);
            this.pButtons.Name = "pButtons";
            this.pButtons.Size = new System.Drawing.Size(256, 617);
            this.pButtons.TabIndex = 1;
            // 
            // bDB2Other
            // 
            this.bDB2Other.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bDB2Other.Location = new System.Drawing.Point(0, 555);
            this.bDB2Other.Margin = new System.Windows.Forms.Padding(0);
            this.bDB2Other.Name = "bDB2Other";
            this.bDB2Other.Size = new System.Drawing.Size(256, 62);
            this.bDB2Other.TabIndex = 3;
            this.bDB2Other.Tag = "";
            this.bDB2Other.Text = "==>";
            this.bDB2Other.UseVisualStyleBackColor = true;
            this.bDB2Other.Click += new System.EventHandler(this.MoveProfiles);
            // 
            // pbPluginPicture
            // 
            this.pbPluginPicture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbPluginPicture.Image = global::PasswordChangeAssistant.Resources.passwordsync;
            this.pbPluginPicture.Location = new System.Drawing.Point(0, 62);
            this.pbPluginPicture.Margin = new System.Windows.Forms.Padding(0);
            this.pbPluginPicture.Name = "pbPluginPicture";
            this.pbPluginPicture.Size = new System.Drawing.Size(256, 555);
            this.pbPluginPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbPluginPicture.TabIndex = 36;
            this.pbPluginPicture.TabStop = false;
            // 
            // bOther2DB
            // 
            this.bOther2DB.Dock = System.Windows.Forms.DockStyle.Top;
            this.bOther2DB.Location = new System.Drawing.Point(0, 0);
            this.bOther2DB.Margin = new System.Windows.Forms.Padding(0);
            this.bOther2DB.Name = "bOther2DB";
            this.bOther2DB.Size = new System.Drawing.Size(256, 62);
            this.bOther2DB.TabIndex = 2;
            this.bOther2DB.Tag = "";
            this.bOther2DB.Text = "<==";
            this.bOther2DB.UseVisualStyleBackColor = true;
            this.bOther2DB.Click += new System.EventHandler(this.MoveProfiles);
            // 
            // lbProfilesOther
            // 
            this.lbProfilesOther.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbProfilesOther.FormattingEnabled = true;
            this.lbProfilesOther.IntegralHeight = false;
            this.lbProfilesOther.ItemHeight = 31;
            this.lbProfilesOther.Location = new System.Drawing.Point(714, 62);
            this.lbProfilesOther.Margin = new System.Windows.Forms.Padding(0);
            this.lbProfilesOther.Name = "lbProfilesOther";
            this.lbProfilesOther.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbProfilesOther.Size = new System.Drawing.Size(459, 617);
            this.lbProfilesOther.Sorted = true;
            this.lbProfilesOther.TabIndex = 4;
            // 
            // tpOptions
            // 
            this.tpOptions.Controls.Add(this.tpPCA);
            this.tpOptions.Controls.Add(this.tpPWSync);
            this.tpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tpOptions.Location = new System.Drawing.Point(18, 0);
            this.tpOptions.Name = "tpOptions";
            this.tpOptions.SelectedIndex = 0;
            this.tpOptions.Size = new System.Drawing.Size(1199, 836);
            this.tpOptions.TabIndex = 16;
            // 
            // tpPCA
            // 
            this.tpPCA.Controls.Add(this.gHideBuildInProfiles);
            this.tpPCA.Controls.Add(this.groupBox1);
            this.tpPCA.Controls.Add(this.gPWFormShown);
            this.tpPCA.Location = new System.Drawing.Point(10, 48);
            this.tpPCA.Name = "tpPCA";
            this.tpPCA.Padding = new System.Windows.Forms.Padding(3);
            this.tpPCA.Size = new System.Drawing.Size(1179, 778);
            this.tpPCA.TabIndex = 0;
            this.tpPCA.Text = "tabPage1";
            this.tpPCA.UseVisualStyleBackColor = true;
            // 
            // gHideBuildInProfiles
            // 
            this.gHideBuildInProfiles.Controls.Add(this.cbHideBuiltInProfiles);
            this.gHideBuildInProfiles.Dock = System.Windows.Forms.DockStyle.Top;
            this.gHideBuildInProfiles.Location = new System.Drawing.Point(3, 244);
            this.gHideBuildInProfiles.Name = "gHideBuildInProfiles";
            this.gHideBuildInProfiles.Size = new System.Drawing.Size(1173, 100);
            this.gHideBuildInProfiles.TabIndex = 2;
            this.gHideBuildInProfiles.TabStop = false;
            this.gHideBuildInProfiles.Text = "groupBox2";
            // 
            // cbHideBuiltInProfiles
            // 
            this.cbHideBuiltInProfiles.AutoSize = true;
            this.cbHideBuiltInProfiles.Location = new System.Drawing.Point(12, 46);
            this.cbHideBuiltInProfiles.Name = "cbHideBuiltInProfiles";
            this.cbHideBuiltInProfiles.Size = new System.Drawing.Size(318, 36);
            this.cbHideBuiltInProfiles.TabIndex = 0;
            this.cbHideBuiltInProfiles.Text = "cbHideBuiltInProfiles";
            this.cbHideBuiltInProfiles.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(-66, -86);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 100);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // gPWFormShown
            // 
            this.gPWFormShown.Controls.Add(this.lAutotypeDelay);
            this.gPWFormShown.Controls.Add(this.nupAutotypeDelay);
            this.gPWFormShown.Controls.Add(this.lOpenUrlForPwChangeShift);
            this.gPWFormShown.Controls.Add(this.cbOpenUrlForPwChange);
            this.gPWFormShown.Dock = System.Windows.Forms.DockStyle.Top;
            this.gPWFormShown.Location = new System.Drawing.Point(3, 3);
            this.gPWFormShown.Name = "gPWFormShown";
            this.gPWFormShown.Size = new System.Drawing.Size(1173, 241);
            this.gPWFormShown.TabIndex = 0;
            this.gPWFormShown.TabStop = false;
            this.gPWFormShown.Text = "gPWFormShown";
            // 
            // lAutotypeDelay
            // 
            this.lAutotypeDelay.AutoSize = true;
            this.lAutotypeDelay.Location = new System.Drawing.Point(159, 159);
            this.lAutotypeDelay.Name = "lAutotypeDelay";
            this.lAutotypeDelay.Size = new System.Drawing.Size(222, 32);
            this.lAutotypeDelay.TabIndex = 3;
            this.lAutotypeDelay.Text = "Auto-Type delay";
            // 
            // nupAutotypeDelay
            // 
            this.nupAutotypeDelay.Location = new System.Drawing.Point(12, 157);
            this.nupAutotypeDelay.Name = "nupAutotypeDelay";
            this.nupAutotypeDelay.Size = new System.Drawing.Size(120, 38);
            this.nupAutotypeDelay.TabIndex = 2;
            // 
            // lOpenUrlForPwChangeShift
            // 
            this.lOpenUrlForPwChangeShift.AutoSize = true;
            this.lOpenUrlForPwChangeShift.Location = new System.Drawing.Point(6, 104);
            this.lOpenUrlForPwChangeShift.Name = "lOpenUrlForPwChangeShift";
            this.lOpenUrlForPwChangeShift.Size = new System.Drawing.Size(93, 32);
            this.lOpenUrlForPwChangeShift.TabIndex = 1;
            this.lOpenUrlForPwChangeShift.Text = "label1";
            // 
            // cbOpenUrlForPwChange
            // 
            this.cbOpenUrlForPwChange.AutoSize = true;
            this.cbOpenUrlForPwChange.Location = new System.Drawing.Point(12, 50);
            this.cbOpenUrlForPwChange.Name = "cbOpenUrlForPwChange";
            this.cbOpenUrlForPwChange.Size = new System.Drawing.Size(370, 36);
            this.cbOpenUrlForPwChange.TabIndex = 0;
            this.cbOpenUrlForPwChange.Text = "cbOpenUrlForPwChange";
            this.cbOpenUrlForPwChange.UseVisualStyleBackColor = true;
            // 
            // tpPWSync
            // 
            this.tpPWSync.Controls.Add(this.lError);
            this.tpPWSync.Controls.Add(this.tlpProfiles);
            this.tpPWSync.Location = new System.Drawing.Point(10, 48);
            this.tpPWSync.Name = "tpPWSync";
            this.tpPWSync.Padding = new System.Windows.Forms.Padding(3);
            this.tpPWSync.Size = new System.Drawing.Size(1179, 778);
            this.tpPWSync.TabIndex = 1;
            this.tpPWSync.Text = "tabPage2";
            this.tpPWSync.UseVisualStyleBackColor = true;
            // 
            // PwProfSyncForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tpOptions);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "PwProfSyncForm";
            this.Padding = new System.Windows.Forms.Padding(18, 0, 18, 0);
            this.Size = new System.Drawing.Size(1235, 836);
            this.tlpProfiles.ResumeLayout(false);
            this.tlpProfiles.PerformLayout();
            this.pButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbPluginPicture)).EndInit();
            this.tpOptions.ResumeLayout(false);
            this.tpPCA.ResumeLayout(false);
            this.gHideBuildInProfiles.ResumeLayout(false);
            this.gHideBuildInProfiles.PerformLayout();
            this.gPWFormShown.ResumeLayout(false);
            this.gPWFormShown.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nupAutotypeDelay)).EndInit();
            this.tpPWSync.ResumeLayout(false);
            this.tpPWSync.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Label lError;
		private System.Windows.Forms.TableLayoutPanel tlpProfiles;
		private System.Windows.Forms.Panel pButtons;
		private System.Windows.Forms.Button bDB2Other;
		private System.Windows.Forms.PictureBox pbPluginPicture;
		private System.Windows.Forms.Button bOther2DB;
		private System.Windows.Forms.ListBox lbProfilesOther;
		private System.Windows.Forms.ComboBox cbProfileOther;
		private System.Windows.Forms.Label lActiveDB;
		private System.Windows.Forms.CheckBox cbModeCopy;
		private System.Windows.Forms.ListBox lbProfilesDB;
		private System.Windows.Forms.TabControl tpOptions;
		private System.Windows.Forms.TabPage tpPCA;
		private System.Windows.Forms.TabPage tpPWSync;
		private System.Windows.Forms.GroupBox gPWFormShown;
		internal System.Windows.Forms.CheckBox cbOpenUrlForPwChange;
		private System.Windows.Forms.Label lOpenUrlForPwChangeShift;
        private System.Windows.Forms.Label lAutotypeDelay;
        internal System.Windows.Forms.NumericUpDown nupAutotypeDelay;
        private System.Windows.Forms.GroupBox gHideBuildInProfiles;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        internal System.Windows.Forms.CheckBox cbHideBuiltInProfiles;
    }
}