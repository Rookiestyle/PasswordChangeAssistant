using KeePass;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;
using KeePassLib;
using KeePassLib.Security;
using PluginTools;
using PluginTranslation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Collections;

namespace PasswordChangeAssistant
{
	public class PasswordChangeAssistantExt : Plugin
	{
		private IPluginHost m_host = null;
		private ToolStripMenuItem m_ContextMenuPCA = null;
		private ToolStripMenuItem m_MainMenuPCA = null;

		private PwEntryForm m_pweForm = null;
		private Button m_btnPCA = null;

		public static PwEntry SelectedEntry;
		internal static ProtectedString m_oldPW;

		private ToolStripMenuItem m_menu = null;
		private List<PwProfile> m_profiles = new List<PwProfile>();
		private PwGeneratorForm m_pwgForm = null;

		private PCADialog m_pcaForm = null;

		private MethodInfo m_miSprCompileFn = null;

		public override bool Initialize(IPluginHost host)
		{
			Terminate();
			if (host == null) return false;
			m_host = host;

			PluginTranslate.TranslationChanged += PluginTranslate_TranslationChanged;
			PluginTranslate.Init(this, KeePass.Program.Translation.Properties.Iso6391Code);
			Tools.DefaultCaption = PluginTranslate.PluginName;
			Tools.PluginURL = "https://github.com/rookiestyle/passwordchangeassistant/";

			GlobalWindowManager.WindowAdded += OnWindowAdded;
			CreateContextMenu();

			m_menu = new ToolStripMenuItem();
			m_menu.Text = PluginTranslate.PluginName + "...";
			m_menu.Click += (o, e) => ShowSyncForm();
			m_menu.Image = SmallIcon;
			m_host.MainWindow.ToolsMenu.DropDownItems.Add(m_menu);
			m_host.MainWindow.DocumentManager.ActiveDocumentSelected += (o, e) => LoadDBProfiles();

			Tools.OptionsFormShown += OptionsFormShown;
			Tools.OptionsFormClosed += OptionsFormClosed;

			Type t = typeof(KeePass.Program).Assembly.GetType("KeePass.UI.AsyncPwListUpdate");
			m_miSprCompileFn = t.GetMethod("SprCompileFn", BindingFlags.Static | BindingFlags.NonPublic);

			return true;
		}

		private void PluginTranslate_TranslationChanged(object sender, TranslationChangedEventArgs e)
		{
			LoadPCASequences();
		}

		private void OnWindowAdded(object sender, GwmWindowEventArgs e)
		{
			if (e.Form is PwGeneratorForm)
			{
				m_pwgForm = (PwGeneratorForm)e.Form;
				m_pwgForm.FormClosed += OnFormClosed;
				LoadDBProfiles();
				return;
			}
			if (!(e.Form is PwEntryForm)) return; //Not the entry form => exit
			m_pweForm = (PwEntryForm)e.Form;
			PrepareEntryForm();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			if (sender is PwGeneratorForm)
			{
				ProfileClicked(sender, new ToolStripItemClickedEventArgs(new ToolStripMenuItem(m_pwgForm.SelectedProfile.Name)));
				List<PwProfile> dbProfiles = Program.Config.PasswordGenerator.UserProfiles.GetDBProfiles();
				bool changed = false;
				UpdateDBProfiles(m_profiles, dbProfiles, out changed);
				if (changed) m_host.MainWindow.UpdateUI(false, null, false, null, false, null, true);
			}
			else if (sender is PwEntryForm)
			{
				m_pweForm = null;
			}
			m_pwgForm = null;
		}

		#region PCA form
		private void CreateContextMenu()
		{
			m_host.MainWindow.EntryContextMenu.Opening += OnEntryMenuOpening;
			m_ContextMenuPCA = new ToolStripMenuItem(PluginTranslate.PluginName + "...");
			m_ContextMenuPCA.Click += OnShowPCAForm;
			m_ContextMenuPCA.Image = DPIAwareness.Scale(Resources.pca, 16, 16);
			m_host.MainWindow.EntryContextMenu.Items.Insert(m_host.MainWindow.EntryContextMenu.Items.Count, m_ContextMenuPCA);

			m_MainMenuPCA = new ToolStripMenuItem(PluginTranslate.PluginName + "...");
			m_MainMenuPCA.Click += OnShowPCAForm;
			m_MainMenuPCA.Image = DPIAwareness.Scale(Resources.pca, 16, 16);
			try
			{
				ToolStripMenuItem entryMenu = m_host.MainWindow.MainMenu.Items["m_menuEntry"] as ToolStripMenuItem;
				entryMenu.DropDownOpening += OnEntryMenuOpening;
				entryMenu.DropDown.Items.Add(m_MainMenuPCA);
			}
			catch (Exception) { }
		}

		private void OnEntryMenuOpening(object sender, EventArgs e)
		{
			m_ContextMenuPCA.Enabled = m_host.MainWindow.GetSelectedEntriesCount() == 1;
			m_MainMenuPCA.Enabled = m_ContextMenuPCA.Enabled;
		}

		private void OnShowPCAForm(object sender, EventArgs e)
		{
			if (!SaveOldPassword()) return;
			m_pcaForm = new PCADialog();
			PCAInitData pcadata = new PCAInitData(SelectedEntry);
			DerefStrings(pcadata, SelectedEntry);
			m_pcaForm.Init(pcadata, ProfilesOpening);
			if (m_pcaForm.ShowDialog(m_host.MainWindow) == DialogResult.OK)
			{
				SelectedEntry.CreateBackup(Program.MainForm.ActiveDatabase);
				SelectedEntry.Strings.Set(PwDefs.PasswordField, m_pcaForm.NewPassword);
				SelectedEntry.Expires = m_pcaForm.EntryExpiry.Checked;
				if (SelectedEntry.Expires)
					SelectedEntry.ExpiryTime = m_pcaForm.EntryExpiry.Value.ToUniversalTime();
				if (string.IsNullOrEmpty(m_pcaForm.Sequence))
					SelectedEntry.Strings.Remove(Config.PCASequence);
				else
					SelectedEntry.Strings.Set(Config.PCASequence, new ProtectedString(false, m_pcaForm.Sequence));
				if (string.IsNullOrEmpty(m_pcaForm.URL2))
					SelectedEntry.Strings.Remove(Config.PCAURLField);
				else
					SelectedEntry.Strings.Set(Config.PCAURLField, new ProtectedString(false, m_pcaForm.URL2));
				SelectedEntry.Touch(true);
				Program.MainForm.UpdateUI(false, null, false, null, true, null, true);
			}
			m_pcaForm.CleanupEx();
			m_pcaForm = null;
		}

		private void RemoveContextMenu()
		{
			m_host.MainWindow.EntryContextMenu.Opening -= OnEntryMenuOpening;
			m_host.MainWindow.EntryContextMenu.Items.Remove(m_ContextMenuPCA);
			m_ContextMenuPCA.Dispose();
			try
			{
				ToolStripMenuItem entryMenu = m_host.MainWindow.MainMenu.Items["m_menuEntry"] as ToolStripMenuItem;
				entryMenu.DropDownOpening -= OnEntryMenuOpening;
				entryMenu.DropDown.Items.Remove(m_MainMenuPCA);
			}
			catch (Exception) { }
			m_MainMenuPCA.Dispose();
		}
		#endregion

		#region Entry form
		private void PrepareEntryForm()
		{
			m_pweForm.PasswordGeneratorContextMenu.Opening += ProfilesOpening;
			m_pweForm.PasswordGeneratorContextMenu.ItemClicked += ProfileClicked;
			m_pweForm.FormClosed += OnFormClosed;
			m_pwgForm = null;
			LoadDBProfiles();
			SaveOldPassword();
			//try to get the edit mode
			PwEditMode m = EditMode();
			if (m == PwEditMode.Invalid) m_pweForm.Shown += OnEntryFormShown; //Reading the edit mode failed, try this as fallback
			else if (m != PwEditMode.EditExistingEntry) return; // we're not in edit mode

			m_pweForm.Resize += OnEntryFormResize;

			//create plugin button right below the button for generating a new password
			CreatePCAButton();

			#region add context menu to plugin button
			ContextMenuStrip cmsPCA = new ContextMenuStrip();
			ToolStripMenuItem menuitem = new ToolStripMenuItem(PluginTranslate.OldPWCopy, m_btnPCA.Image, PasswordCopyClick);
			menuitem.Name = PluginTranslate.PluginName + "OldCopy";
			cmsPCA.Items.Add(menuitem);
			menuitem = new ToolStripMenuItem(PluginTranslate.OldPWType, m_btnPCA.Image, PasswordTypeClick);
			menuitem.Name = PluginTranslate.PluginName + "OldType";
			cmsPCA.Items.Add(menuitem);
			menuitem = new ToolStripMenuItem(PluginTranslate.NewPWCopy, m_btnPCA.Image, PasswordCopyClick);
			menuitem.Name = PluginTranslate.PluginName + "NewCopy";
			cmsPCA.Items.Add(menuitem);
			menuitem = new ToolStripMenuItem(PluginTranslate.NewPWType, m_btnPCA.Image, PasswordTypeClick);
			menuitem.Name = PluginTranslate.PluginName + "NewType";
			cmsPCA.Items.Add(menuitem);
			cmsPCA.Items.Add(new ToolStripSeparator());
			menuitem = new ToolStripMenuItem(PluginTranslate.PluginName + "...", m_btnPCA.Image, ShowPCAFormFromEntry);
			menuitem.Name = PluginTranslate.PluginName + "ShowPCAFormFromEntry";
			cmsPCA.Items.Add(menuitem);
			m_btnPCA.ContextMenuStrip = cmsPCA;
			m_btnPCA.Click += (o, x) => m_btnPCA.ContextMenuStrip.Show(m_btnPCA, 0, m_btnPCA.Height);
			#endregion

			//finally add the button to the form
			m_btnPCA.BringToFront();
		}

		private void CreatePCAButton()
		{
			Control btnHide = Tools.GetControl("m_cbHidePassword", m_pweForm);
			Control btnPwGen = Tools.GetControl("m_btnGenPw", m_pweForm);
			m_btnPCA = new Button();
			m_btnPCA.Name = "m_btnPCA";
			ToolTip tt = new ToolTip();
			tt.SetToolTip(m_btnPCA, PluginTranslate.PluginName);
			if ((btnPwGen != null) && (btnHide != null))
			{
				m_btnPCA.Top = btnPwGen.Top + (btnPwGen.Top - btnHide.Top);
				m_btnPCA.Left = btnPwGen.Left;
				m_btnPCA.Width = btnPwGen.Width;
				m_btnPCA.Height = btnPwGen.Height;
			}
			else
			{
				m_btnPCA.Top = 179;
				m_btnPCA.Left = 634;
				m_btnPCA.Width = 48;
				m_btnPCA.Height = 35;
			}
			m_btnPCA.Image = DPIAwareness.Scale(Resources.pca, 16, 16);

			if (btnPwGen != null) btnPwGen.Parent.Controls.Add(m_btnPCA);
			else if (btnHide != null) btnHide.Parent.Controls.Add(m_btnPCA);
		}

		private void ShowPCAFormFromEntry(object sender, EventArgs e)
		{
			m_pcaForm = new PCADialog();
			m_pweForm.UpdateEntryStrings(true, true, true);
			PCAInitData pcadata = new PCAInitData(m_pweForm.EntryRef);
			ExpiryControlGroup ecg = (ExpiryControlGroup)Tools.GetField("m_cgExpiry", m_pweForm);
			pcadata.Strings = m_pweForm.EntryStrings;
			if (ecg != null)
			{
				pcadata.Expires = ecg.Checked;
				pcadata.Expiry = ecg.Value;
				pcadata.SetExpiry = (pcadata.Expires != m_pweForm.EntryRef.Expires) || (pcadata.Expiry != m_pweForm.EntryRef.ExpiryTime);
			}
			pcadata.URL2 = pcadata.Strings.ReadSafe(Config.PCAURLField);
			DerefStrings(pcadata, m_pweForm.EntryRef);
			m_pcaForm.Init(pcadata, ProfilesOpening);
			if (m_pcaForm.ShowDialog(m_pweForm) == DialogResult.OK)
			{
				m_pweForm.EntryStrings.Set(PwDefs.PasswordField, m_pcaForm.NewPassword);
				if (ecg != null)
				{
					ecg.Checked = m_pcaForm.EntryExpiry.Checked;
					if (ecg.Checked) ecg.Value = m_pcaForm.EntryExpiry.Value.ToUniversalTime();
				}
				if (string.IsNullOrEmpty(m_pcaForm.Sequence))
					m_pweForm.EntryStrings.Remove(Config.PCASequence);
				else
					m_pweForm.EntryStrings.Set(Config.PCASequence, new ProtectedString(false, m_pcaForm.Sequence));
				if (string.IsNullOrEmpty(m_pcaForm.URL2))
					m_pweForm.EntryStrings.Remove(Config.PCAURLField);
				else
					m_pweForm.EntryStrings.Set(Config.PCAURLField, new ProtectedString(false, m_pcaForm.URL2));
				m_pweForm.UpdateEntryStrings(false, true, true);
			}
			m_pcaForm.CleanupEx();
			m_pcaForm = null;
		}

		private PwEditMode EditMode()
		{
			PwEditMode m = PwEditMode.Invalid;
			if (m_pweForm == null) return m;
			PropertyInfo pi = typeof(PwEntryForm).GetProperty("EditModeEx");
			if (pi != null)
			{ //will work starting with KeePass 2.41, preferred way as it's a public attribute
				m = (PwEditMode)pi.GetValue(m_pweForm, null);
			}
			else
			{ // try reading private field
				m = (PwEditMode)Tools.GetField("m_pwEditMode", m_pweForm);
			}
			return m;
		}

		private void OnEntryFormShown(object sender, EventArgs e)
		{
			Tools.ShowInfo("a");
			m_btnPCA.Visible = (m_pweForm.Text == KPRes.EditEntry);
			m_pweForm.Shown -= OnEntryFormShown;
		}

		private void OnEntryFormResize(object sender, EventArgs e)
		{
			if (m_btnPCA == null) return;
			Control btnPwGen = Tools.GetControl("m_btnGenPw", m_pweForm);
			if (btnPwGen == null) return;
			Control btnHide = Tools.GetControl("m_cbHidePassword", m_pweForm);
			if (btnHide == null) return;
			m_btnPCA.Top = btnPwGen.Top + (btnPwGen.Top - btnHide.Top);
			m_btnPCA.Left = btnPwGen.Left;
			m_btnPCA.Width = btnPwGen.Width;
			m_btnPCA.Height = btnPwGen.Height;
		}

		private void PasswordTypeClick(object sender, EventArgs e)
		{
			if ((sender as ToolStripMenuItem).Name.Contains("Old"))
				PasswordType(m_oldPW);
			else if ((sender as ToolStripMenuItem).Name.Contains("New"))
			{
				SecureTextBoxEx newPassword = (SecureTextBoxEx)Tools.GetControl("m_tbPassword", m_pweForm);
				PasswordType(newPassword.TextEx);
			}
		}

		private void PasswordCopyClick(object sender, EventArgs e)
		{
			if ((sender as ToolStripMenuItem).Name.Contains("Old"))
				PasswordCopy(m_oldPW);
			else if ((sender as ToolStripMenuItem).Name.Contains("New"))
			{
				SecureTextBoxEx newPassword = (SecureTextBoxEx)Tools.GetControl("m_tbPassword", m_pweForm);
				PasswordCopy(newPassword.TextEx);
			}
		}
		#endregion

		#region password change functionality
		//Dereference placeholders in URL fields
		private void DerefStrings(PCAInitData pcadata, PwEntry pe)
		{
			if (m_miSprCompileFn == null) return;
			if (pcadata.URL.Contains("{"))
			{
				PwListItem pli = new PwListItem(pe);
				pcadata.URL = (string)m_miSprCompileFn.Invoke(null, new object[] { pcadata.URL, pli });
			}
			if (pcadata.URL2.Contains("{"))
			{
				PwListItem pli = new PwListItem(pe);
				pcadata.URL2 = (string)m_miSprCompileFn.Invoke(null, new object[] { pcadata.URL2, pli });
			}
		}

		private bool SaveOldPassword()
		{
			if (m_host.MainWindow.GetSelectedEntriesCount() != 1) return false;
			SelectedEntry = m_host.MainWindow.GetSelectedEntry(true);
			m_oldPW = SelectedEntry.Strings.GetSafe(PwDefs.PasswordField);
			return true;
		}

		internal static void PasswordType(ProtectedString ps)
		{
			/* The password string may contain special chars and/or field references
			 * Easiest way is to set the provided value as new string field
			 * and have KeePass do the rest
			*/
			SetPasswordField(ps);
			AutoType.PerformIntoPreviousWindow(Program.MainForm, SelectedEntry,
				Program.MainForm.ActiveDatabase, Config.PCAPluginFieldRef);
			SetPasswordField(null);
		}

		internal static void PasswordCopy(ProtectedString ps)
		{
			/* The password string may contain special chars and/or field references
			 * Easiest way is to set the provided value as new string field
			 * and have KeePass do the rest
			*/

			bool bUseFallback = Tools.KeePassVersion < new Version(2, 43);
			if (bUseFallback)
			{
				//Set MinimizeAfterClipboardCopy to false and restore afterwards
				//to ensure neither PwEntryForm nor PCADialog is closed because of minimizing the main window
				bool bMACC = Program.Config.MainWindow.MinimizeAfterClipboardCopy;
				if (bMACC)
					Program.Config.MainWindow.MinimizeAfterClipboardCopy = false;
				try
				{
					SetPasswordField(ps);
					if (ClipboardUtil.CopyAndMinimize(Config.PCAPluginFieldRef, true, Program.MainForm,
							SelectedEntry, Program.MainForm.ActiveDatabase))
						Program.MainForm.StartClipboardCountdown();
					SetPasswordField(null);
				}
				catch { }
				if (bMACC)
					Program.Config.MainWindow.MinimizeAfterClipboardCopy = bMACC;
			}
			else
			{
				SetPasswordField(ps);
				if (ClipboardUtil.CopyAndMinimize(Config.PCAPluginFieldRef, true, GlobalWindowManager.TopWindow,
						SelectedEntry, Program.MainForm.ActiveDatabase))
					Program.MainForm.StartClipboardCountdown();
				SetPasswordField(null);
			}
		}

		internal static void SequenceType(ProtectedString ps, string sequence)
		{
			/* The password string may contain special chars and/or field references
				* Easiest way is to set the provided value as new string field
				* and have KeePass do the rest
			*/
			SetPasswordField(ps);
			sequence = sequence.Replace(Config.PlaceholderOldPW, "{PASSWORD}");
			sequence = sequence.Replace(Config.PlaceholderNewPW, Config.PCAPluginFieldRef);
			AutoType.PerformIntoPreviousWindow(Program.MainForm, SelectedEntry,
				Program.MainForm.ActiveDatabase, sequence);
			SetPasswordField(null);
		}

		internal static string GetPCASequence(PwEntry e, string sDefault)
		{
			string result = e.Strings.ReadSafe(Config.PCASequence);
			if (string.IsNullOrEmpty(result)) result = sDefault;
			return result;
		}

		internal static void SetPasswordField(ProtectedString ps)
		{
			SelectedEntry.Strings.Remove(Config.PCAPluginField);
			if (ps != null) SelectedEntry.Strings.Set(Config.PCAPluginField, ps);
		}

		private void LoadPCASequences()
		{
			Config.DefaultPCASequences.Clear();
			Config.DefaultPCASequences.Add(PluginTranslate.DefaultSequence01, "{PCA_OldPW}{TAB}{PCA_NewPW}{TAB}{PCA_NewPW}");
			Config.DefaultPCASequences.Add(PluginTranslate.DefaultSequence02, "{PCA_OldPW}{ENTER}{DELAY 5000}{PCA_NewPW}{TAB}{PCA_NewPW}");
			Config.DefaultPCASequences.Add(PluginTranslate.DefaultSequence03, "{PCA_NewPW}{TAB}{PCA_NewPW}");
			Config.DefaultPCASequences = Config.DefaultPCASequences.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

			Config.DefaultPCASequences[PluginTranslate.EntrySpecificSequence] = string.Empty;
		}
		#endregion

		#region Handle multiple databases
		//Add database specific profiles to global configuration
		private void LoadDBProfiles()
		{
			Program.Config.PasswordGenerator.UserProfiles.RemoveDBProfiles();
			if ((m_host.Database == null) || !m_host.Database.IsOpen) return;
			m_profiles = m_host.Database.GetDBProfiles();
			foreach (PwProfile profile in m_profiles)
				Program.Config.PasswordGenerator.UserProfiles.AddDBProfile(profile);
		}

		private void OptionsFormShown(object sender, Tools.OptionsFormsEventArgs e)
		{
			PwProfSyncForm form = new PwProfSyncForm();
			form.SetHomeDB(m_host.Database);
			Tools.AddPluginToOptionsForm(this, form);
		}

		private void OptionsFormClosed(object sender, Tools.OptionsFormsEventArgs e)
		{
			if (e.form.DialogResult != DialogResult.OK) return;
			bool shown = false;
			PwProfSyncForm form = (PwProfSyncForm)Tools.GetPluginFromOptions(this, out shown);
			if (!shown) return;
			List<string> profilesDB = new List<string>();
			List<string> profilesOther = new List<string>();
			PwDatabase otherDB = null;
			bool MoveProfiles = true;
			form.GetWorklist(out profilesDB, out profilesOther, out otherDB, out MoveProfiles);
			form.Dispose();

			//Update password profiles in active database
			bool changed = false;
			bool changedOther = false;
			foreach (string profileName in profilesDB)
				if (profileName.EndsWith(Config.ProfileCopied))
				{
					string profileNameClean = profileName.Substring(0, profileName.Length - Config.ProfileCopied.Length);
					if (otherDB == null)
					{
						PwProfile profile = Program.Config.PasswordGenerator.UserProfiles.GetProfile(profileNameClean);
						changed |= profile.CopyTo(m_host.Database);
						if (MoveProfiles)
							Program.Config.PasswordGenerator.UserProfiles.Remove(profile);
					}
					else
					{
						PwProfile profile = otherDB.GetProfile(profileNameClean);
						if (profile != null)
						{
							changed |= profile.CopyTo(m_host.Database);
							if (MoveProfiles)
							{
								otherDB.RemoveDBProfile(profileNameClean);
								changedOther = true;
							}
						}
					}
				}
			//Update password profiles in global configuration or other database
			foreach (string profileName in profilesOther)
				if (profileName.EndsWith(Config.ProfileCopied))
				{
					string profileNameClean = profileName.Substring(0, profileName.Length - Config.ProfileCopied.Length);
					if (otherDB == null)
					{
						PwProfile profile = Program.Config.PasswordGenerator.UserProfiles.GetProfile(profileNameClean + Config.ProfileDBOnly);
						if (MoveProfiles)
						{
							profile.Name = profileNameClean;
							m_host.Database.RemoveDBProfile(profileNameClean);
							changed = true;
						}
						else
						{
							PwProfile newProfile = profile.CloneDeep();
							newProfile.Name = profileNameClean;
							Program.Config.PasswordGenerator.UserProfiles.Add(newProfile);
						}
					}
					else
					{
						PwProfile profile = m_host.Database.GetProfile(profileNameClean);
						if (profile != null)
						{
							changedOther |= profile.CopyTo(otherDB);
							if (MoveProfiles)
							{
								m_host.Database.RemoveDBProfile(profileNameClean);
								changed = true;
							}
						}
					}
				}
			if (changed)
			{
				m_host.Database.SettingsChanged = DateTime.UtcNow;
				m_host.Database.Modified = true;
			}
			if (changedOther)
			{
				otherDB.SettingsChanged = DateTime.UtcNow;
				otherDB.Modified = true;
			}
			if (changed || changedOther)
				m_host.MainWindow.UpdateUI(false, null, false, null, false, null, false);
		}

		private void ShowSyncForm()
		{
			if ((m_host.Database == null) || !m_host.Database.IsOpen)
			{
				Tools.ShowError(PluginTranslate.NoDB);
				return;
			}
			Tools.ShowOptions();
		}
		#endregion

		#region Show special icon for used password profile
		private void ProfileClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if ((m_pweForm == null) || string.IsNullOrEmpty(e.ClickedItem.Text)) return;
			List<PwProfile> profiles = PwGeneratorUtil.GetAllProfiles(false);
			string prof = e.ClickedItem.Text.Replace("&", "");
			m_pweForm.UpdateEntryStrings(true, true);
			PwProfile profile = profiles.Find(x => x.Name == prof);
			if (profile != null)
				m_pweForm.EntryStrings.Set(Config.ProfileLastUsedProfile, new ProtectedString(false, prof));
			else if (prof == "(" + KPRes.AutoGeneratedPasswordSettings + ")")
				m_pweForm.EntryStrings.Set(Config.ProfileLastUsedProfile, new ProtectedString(false, Config.ProfileAutoGenerated));
			else
				m_pweForm.EntryStrings.Remove(Config.ProfileLastUsedProfile);
			m_pweForm.UpdateEntryStrings(false, true);
		}

		private void ProfilesOpening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			string lastProfile = string.Empty;
			string pcaFormProfile = m_pcaForm == null ? string.Empty : m_pcaForm.Profile;
			if (m_pweForm != null)
			{
				lastProfile = m_pweForm.EntryStrings.ReadSafeEx(Config.ProfileLastUsedProfile);
				PluginDebug.AddInfo("Profiles opened in entry form", 0,
					"Last used profile:" + lastProfile);
			}
			else
			{
				PwEntry entry = m_host.MainWindow.GetSelectedEntry(true);
				if (entry != null) lastProfile = entry.Strings.ReadSafeEx(Config.ProfileLastUsedProfile);
				PluginDebug.AddInfo("Profiles opened in Entry form", 0,
					"Selected profile: " + m_pcaForm.Profile,
					"Last used profile:" + lastProfile);
				if (entry == null) return;
			}
			if (string.IsNullOrEmpty(lastProfile) && string.IsNullOrEmpty(pcaFormProfile)) return;
			List<string> lMsg = new List<string>();
			lMsg.Add("Checking special profiles (selected / last used)");
			foreach (ToolStripItem profile in (sender as ContextMenuStrip).Items)
			{
				if (!(profile is ToolStripMenuItem)) continue;
				string profText = profile.Text.Replace("&", "");
				string msg = "Checking profile '" + profText + "'";
				if (profText == pcaFormProfile)
				{
					msg += "; Selected: true";
					profile.Font = new System.Drawing.Font(profile.Font, profile.Font.Style | System.Drawing.FontStyle.Bold);
				}
				else
				{
					if (m_pcaForm != null) msg += "; Selected: false";
					profile.Font = new System.Drawing.Font(profile.Font, profile.Font.Style & ~System.Drawing.FontStyle.Bold);
				}
				if ((profText == lastProfile) ||
					((profText == "(" + KPRes.AutoGeneratedPasswordSettings + ")") && (lastProfile == Config.ProfileAutoGenerated)))
				{
					profile.Image = SmallIcon;
					msg += "; Last used: true";
					//break;
				}
				else msg += "; Last used: false";
				lMsg.Add(msg);
			}
			PluginDebug.AddInfo("Password profile context menu", 0, lMsg.ToArray());

		}

		//Update list of database specific password profiles after closing password generator form
		private void UpdateDBProfiles(List<PwProfile> oldProfiles, List<PwProfile> newProfiles, out bool changed)
		{
			changed = false;
			if ((m_host.Database == null) || !m_host.Database.IsOpen) return;
			if (oldProfiles.Count != newProfiles.Count)
			{
				changed = true;
				m_host.Database.RemoveDBProfiles();
				foreach (PwProfile profile in newProfiles)
					profile.CopyTo(m_host.Database);
				return;
			}
			for (int i = 0; i < newProfiles.Count; i++)
			{
				PwProfile profile = newProfiles[i].CloneDeep();
				if (profile.IsDBOnlyProfile())
					profile.Name = profile.Name.Substring(0, profile.Name.Length - Config.ProfileDBOnly.Length);
				PwProfile oldProfile = oldProfiles.Find(x => x.Name == profile.Name);
				changed |= !profile.IsEqual(oldProfile);
				if (changed) break;
			}
			for (int i = 0; i < oldProfiles.Count; i++)
			{
				PwProfile profile = oldProfiles[i].CloneDeep();
				if (!profile.IsDBOnlyProfile())
					profile.Name += Config.ProfileDBOnly;
				PwProfile newProfile = newProfiles.Find(x => x.Name == profile.Name);
				changed |= !profile.IsEqual(newProfile);
				if (changed) break;
			}
			if (!changed) return;
			m_host.Database.RemoveDBProfiles();
			foreach (PwProfile profile in newProfiles)
				profile.CopyTo(m_host.Database);
		}
		#endregion

		public override void Terminate()
		{
			if (m_host == null) return;
			PluginTranslate.TranslationChanged -= PluginTranslate_TranslationChanged;
			GlobalWindowManager.WindowAdded -= OnWindowAdded;
			m_host.MainWindow.DocumentManager.ActiveDocumentSelected -= (o, e) => LoadDBProfiles();
			m_host.MainWindow.ToolsMenu.DropDownItems.Remove(m_menu);
			Program.Config.PasswordGenerator.UserProfiles.RemoveDBProfiles();
			RemoveContextMenu();
			PluginDebug.SaveOrShow();
			m_host = null;
		}

		public override string UpdateUrl
		{
			get { return "https://raw.githubusercontent.com/rookiestyle/passwordchangeassistant/master/version.info"; }
		}

		public override System.Drawing.Image SmallIcon
		{
			get
			{
				return DPIAwareness.Scale(Resources.pca, 16, 16);
			}
		}
	}
}