using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using KeePassLib;

using PluginTranslation;
using PluginTools;

namespace PasswordChangeAssistant
{
	public partial class PwProfSyncForm : UserControl
	{
		public struct DBInfo
		{
			public string path;
			public string name;
			public override string ToString()
			{
				return name;
			}

			public DBInfo(PwDatabase db)
			{
				path = db.IOConnectionInfo.Path;
				name = FriendlyName(db);
			}
		}

		private PwDatabase m_otherDB = null;
		private List<string> m_ProfilesDB = new List<string>();
		private List<string> m_ProfilesOther = new List<string>();

		private string m_homeDB = string.Empty;

		public PwProfSyncForm()
		{
			InitializeComponent();
			Text = PluginTranslate.Options;
			lActiveDB.Text = PluginTranslate.OptionsActiveDB;
			cbModeCopy.Text = PluginTranslate.ModeCopy;
		}

		public void SetHomeDB(PwDatabase db)
		{
			if ((db == null) || !db.IsOpen)
			{
				Init(false);
				return;
			}
			m_homeDB = db.IOConnectionInfo.Path;
			string dbname = FriendlyName(db);
			if (dbname.Length > 30)
				dbname = dbname.Substring(0, 10) + "..." + dbname.Substring(dbname.Length - 10); ;
			lActiveDB.Text = string.Format(PluginTranslate.OptionsActiveDB, dbname);
			m_ProfilesDB = db.GetDBProfileNames();
			lbProfilesDB.Items.AddRange(m_ProfilesDB.ToArray());
			Init(true);
		}

		private void MoveProfiles(object sender, EventArgs e)
		{
			if ((sender as Button).Name == bDB2Other.Name)
				MoveProfiles(lbProfilesDB, lbProfilesOther, true);
			else
				MoveProfiles(lbProfilesOther, lbProfilesDB, false);
		}

		private void MoveProfiles(ListBox source, ListBox target, bool db2other)
		{
			List<string> removeFromSource = new List<string>();
			List<string> moved = new List<string>();
			bool bRemoveAllowed = true;
			foreach (var item in source.SelectedItems)
			{
				moved.Add(item.ToString());
				if (item.ToString().EndsWith(Config.ProfileCopied))
				{ // remember items to remove that were copied from target in a previously move
					removeFromSource.Add(item.ToString());
					if (cbModeCopy.Checked) continue;
				}
				if (!target.Items.Contains(item) ||
					(bRemoveAllowed = Tools.AskYesNo(string.Format(PluginTranslate.OptionsOverwrite, item.ToString())) == DialogResult.Yes))
				{
					//copy entry to target and append suffix 
					target.Items.Remove(item);
					target.Items.Remove(item.ToString() + Config.ProfileCopied);
					if (item.ToString().EndsWith(Config.ProfileCopied))
						target.Items.Add(item.ToString().Substring(0, item.ToString().Length - Config.ProfileCopied.Length));
					else
						target.Items.Add(item.ToString() + Config.ProfileCopied);
				}
			}
			//remove items from source
			//needs to be outside foreach as otherwise the iterator would change which is not possible
			if (!bRemoveAllowed) return;
			foreach (string remove in removeFromSource)
				source.Items.Remove(remove);

			//re-add missig initial profiles
			foreach (string profile in (db2other ? m_ProfilesDB : m_ProfilesOther))
				if (!source.Items.Contains(profile) && !source.Items.Contains(profile + Config.ProfileCopied)) source.Items.Add(profile);

			if (!cbModeCopy.Checked)
				foreach (var item in moved)
					source.Items.Remove(item);
		}

		public void Init(bool active)
		{
			if (!active)
			{
				tlpProfiles.Visible = false;
				lError.Text = PluginTranslate.NoDB;
				lError.Visible = true;
				return;
			}
			lError.Visible = false;
			cbProfileOther.Items.Add(PluginTranslate.OptionsGlobal);
			foreach (PwDatabase db in KeePass.Program.MainForm.DocumentManager.GetOpenDatabases())
			{
				if (db.IOConnectionInfo.Path == m_homeDB) continue;
				cbProfileOther.Items.Add(new DBInfo(db));
			}
			cbProfileOther.SelectedIndex = 0;

			if (!KeePass.Program.Config.UI.OptimizeForScreenReader)
			{
				bDB2Other.Image = Config.ScaleImage(Resources.arrow, 40, 20);
				bOther2DB.Image = Config.ScaleImage(Resources.arrow, 40, 20);
				bDB2Other.Image.RotateFlip(RotateFlipType.Rotate180FlipY);
				bDB2Other.Text = bOther2DB.Text = string.Empty;
				pbPluginPicture.Image = Config.ScaleImage(pbPluginPicture.Image, Math.Min(96, pbPluginPicture.Image.Width), Math.Min(96, pbPluginPicture.Image.Height));
			}
			else pbPluginPicture.Visible = false;
		}

		private void profileOther_SelectedIndexChanged(object sender, EventArgs e)
		{
			m_ProfilesOther.Clear();
			if (cbProfileOther.SelectedIndex == 0)
			{
				foreach (var profile in KeePass.Program.Config.PasswordGenerator.UserProfiles)
					if (!profile.IsDBOnlyProfile())
						m_ProfilesOther.Add(profile.Name);
			}
			else
			{
				bool found = false;
				for (int i = 0; i < KeePass.Program.MainForm.DocumentManager.DocumentCount; i++)
				{
					m_otherDB = KeePass.Program.MainForm.DocumentManager.Documents[i].Database;
					if (!m_otherDB.IsOpen) continue;
					if (m_otherDB.IOConnectionInfo.Path != ((DBInfo)cbProfileOther.Items[cbProfileOther.SelectedIndex]).path) continue;
					m_ProfilesOther = m_otherDB.GetDBProfileNames();
					found = true;
					break;
				}
				if (!found) m_otherDB = null;
			}
			lbProfilesOther.Items.Clear();
			foreach (string profile in m_ProfilesOther)
				lbProfilesOther.Items.Add(profile);

			int index = lbProfilesDB.SelectedIndex;
			lbProfilesDB.Items.Clear();
			foreach (string profile in m_ProfilesDB)
				lbProfilesDB.Items.Add(profile);
			if (index > -1)
				lbProfilesDB.SelectedIndex = index;
		}

		public void GetWorklist(out List<string> profilesCurrentDB, out List<string> profilesOther, out PwDatabase db, out bool MoveProfiles)
		{
			profilesCurrentDB = new List<string>();
			profilesOther = new List<string>();
			db = m_otherDB;

			MoveProfiles = !cbModeCopy.Checked;

			foreach (var item in lbProfilesDB.Items)
				profilesCurrentDB.Add(item.ToString());

			foreach (var item in this.lbProfilesOther.Items)
				profilesOther.Add(item.ToString());
		}

		internal static string FriendlyName(PwDatabase db)
		{
			if (!db.IsOpen) return string.Empty;
			if (!string.IsNullOrEmpty(db.Name)) return db.Name;
			return KeePassLib.Utility.UrlUtil.GetFileName(db.IOConnectionInfo.Path);
		}
	}
}
