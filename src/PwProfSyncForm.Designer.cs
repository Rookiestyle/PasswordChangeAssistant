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
			this.tlpProfiles.SuspendLayout();
			this.pButtons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbPluginPicture)).BeginInit();
			this.SuspendLayout();
			// 
			// lError
			// 
			this.lError.AutoSize = true;
			this.lError.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lError.ForeColor = System.Drawing.Color.Red;
			this.lError.Location = new System.Drawing.Point(28, 20);
			this.lError.Name = "lError";
			this.lError.Size = new System.Drawing.Size(49, 20);
			this.lError.TabIndex = 15;
			this.lError.Text = "Error";
			// 
			// tlpProfiles
			// 
			this.tlpProfiles.ColumnCount = 3;
			this.tlpProfiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tlpProfiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 144F));
			this.tlpProfiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tlpProfiles.Controls.Add(this.lbProfilesDB, 0, 1);
			this.tlpProfiles.Controls.Add(this.cbModeCopy, 0, 2);
			this.tlpProfiles.Controls.Add(this.lActiveDB, 0, 0);
			this.tlpProfiles.Controls.Add(this.cbProfileOther, 2, 0);
			this.tlpProfiles.Controls.Add(this.pButtons, 1, 1);
			this.tlpProfiles.Controls.Add(this.lbProfilesOther, 2, 1);
			this.tlpProfiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlpProfiles.Location = new System.Drawing.Point(10, 0);
			this.tlpProfiles.Name = "tlpProfiles";
			this.tlpProfiles.RowCount = 3;
			this.tlpProfiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tlpProfiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpProfiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tlpProfiles.Size = new System.Drawing.Size(580, 400);
			this.tlpProfiles.TabIndex = 0;
			// 
			// lbProfilesDB
			// 
			this.lbProfilesDB.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbProfilesDB.FormattingEnabled = true;
			this.lbProfilesDB.IntegralHeight = false;
			this.lbProfilesDB.ItemHeight = 20;
			this.lbProfilesDB.Location = new System.Drawing.Point(0, 40);
			this.lbProfilesDB.Margin = new System.Windows.Forms.Padding(0);
			this.lbProfilesDB.Name = "lbProfilesDB";
			this.lbProfilesDB.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.lbProfilesDB.Size = new System.Drawing.Size(218, 300);
			this.lbProfilesDB.Sorted = true;
			this.lbProfilesDB.TabIndex = 1;
			// 
			// cbModeCopy
			// 
			this.cbModeCopy.AutoSize = true;
			this.tlpProfiles.SetColumnSpan(this.cbModeCopy, 3);
			this.cbModeCopy.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cbModeCopy.Location = new System.Drawing.Point(0, 340);
			this.cbModeCopy.Margin = new System.Windows.Forms.Padding(0);
			this.cbModeCopy.Name = "cbModeCopy";
			this.cbModeCopy.Size = new System.Drawing.Size(580, 60);
			this.cbModeCopy.TabIndex = 5;
			// 
			// lActiveDB
			// 
			this.lActiveDB.AutoSize = true;
			this.tlpProfiles.SetColumnSpan(this.lActiveDB, 2);
			this.lActiveDB.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lActiveDB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.lActiveDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lActiveDB.Location = new System.Drawing.Point(3, 3);
			this.lActiveDB.Margin = new System.Windows.Forms.Padding(3);
			this.lActiveDB.Name = "lActiveDB";
			this.lActiveDB.Size = new System.Drawing.Size(356, 34);
			this.lActiveDB.TabIndex = 12;
			this.lActiveDB.Text = "Active database";
			// 
			// cbProfileOther
			// 
			this.cbProfileOther.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cbProfileOther.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbProfileOther.FormattingEnabled = true;
			this.cbProfileOther.Location = new System.Drawing.Point(365, 3);
			this.cbProfileOther.Name = "cbProfileOther";
			this.cbProfileOther.Size = new System.Drawing.Size(212, 28);
			this.cbProfileOther.TabIndex = 0;
			this.cbProfileOther.SelectedIndexChanged += new System.EventHandler(this.profileOther_SelectedIndexChanged);
			// 
			// pButtons
			// 
			this.pButtons.Controls.Add(this.bDB2Other);
			this.pButtons.Controls.Add(this.pbPluginPicture);
			this.pButtons.Controls.Add(this.bOther2DB);
			this.pButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pButtons.Location = new System.Drawing.Point(218, 40);
			this.pButtons.Margin = new System.Windows.Forms.Padding(0);
			this.pButtons.Name = "pButtons";
			this.pButtons.Size = new System.Drawing.Size(144, 300);
			this.pButtons.TabIndex = 1;
			// 
			// bDB2Other
			// 
			this.bDB2Other.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bDB2Other.Location = new System.Drawing.Point(0, 260);
			this.bDB2Other.Margin = new System.Windows.Forms.Padding(0);
			this.bDB2Other.Name = "bDB2Other";
			this.bDB2Other.Size = new System.Drawing.Size(144, 40);
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
			this.pbPluginPicture.Location = new System.Drawing.Point(0, 40);
			this.pbPluginPicture.Margin = new System.Windows.Forms.Padding(0);
			this.pbPluginPicture.Name = "pbPluginPicture";
			this.pbPluginPicture.Size = new System.Drawing.Size(144, 260);
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
			this.bOther2DB.Size = new System.Drawing.Size(144, 40);
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
			this.lbProfilesOther.ItemHeight = 20;
			this.lbProfilesOther.Location = new System.Drawing.Point(362, 40);
			this.lbProfilesOther.Margin = new System.Windows.Forms.Padding(0);
			this.lbProfilesOther.Name = "lbProfilesOther";
			this.lbProfilesOther.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.lbProfilesOther.Size = new System.Drawing.Size(218, 300);
			this.lbProfilesOther.Sorted = true;
			this.lbProfilesOther.TabIndex = 4;
			// 
			// PwProfSyncForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.tlpProfiles);
			this.Controls.Add(this.lError);
			this.Name = "PwProfSyncForm";
			this.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
			this.Size = new System.Drawing.Size(600, 400);
			this.tlpProfiles.ResumeLayout(false);
			this.tlpProfiles.PerformLayout();
			this.pButtons.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbPluginPicture)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

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
	}
}