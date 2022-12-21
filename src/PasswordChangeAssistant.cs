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
using KeePassLib.Cryptography;
using KeePass.Util.Spr;

namespace PasswordChangeAssistant
{
	public class PasswordChangeAssistantExt : Plugin
	{
		private IPluginHost m_host = null;
		private ToolStripMenuItem m_ContextMenuPCA = null;
		private ToolStripMenuItem m_MainMenuPCA = null;

		private PwEntryForm m_pweForm = null;
		private Button m_btnPCA = null;

		private ToolStripMenuItem m_menu = null;
		private List<PwProfile> m_profiles = new List<PwProfile>();
		
		private PCADialog m_pcaForm = null;

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
			m_host.MainWindow.FileOpened += OnFileOpened;

			Tools.OptionsFormShown += OptionsFormShown;
			Tools.OptionsFormClosed += OptionsFormClosed;
			m_host.PwGeneratorPool.Add(new PwProfile1PerSet());

			return true;
		}

		private void OnFileOpened(object sender, FileOpenedEventArgs e)
		{
			DataMigration.CheckAndMigrate(e.Database);
		}

		private void PluginTranslate_TranslationChanged(object sender, TranslationChangedEventArgs e)
		{
			LoadPCASequences();
		}

		private void OnWindowAdded(object sender, GwmWindowEventArgs e)
		{
			if (e.Form is PwGeneratorForm)
			{
				e.Form.Shown += OnPwGeneratorFormShown;
				e.Form.FormClosed += OnFormClosed;
				LoadDBProfiles();
				return;
			}
			if (e.Form is SingleLineEditForm)
			{
				HandleProfileSaveForm(e.Form as SingleLineEditForm);
				return;
			}
			if (!(e.Form is PwEntryForm) || m_pweForm != null) return; //Not the entry form => exit
			PrepareEntryForm((PwEntryForm)e.Form);
		}

		private void OnPwGeneratorFormShown(object sender, EventArgs e)
		{
			List<string> lMsg = new List<string>();
			Control cGroup = Tools.GetControl("m_grpCurOpt", sender as PwGeneratorForm);
			ComboBox cbProfile = Tools.GetControl("m_cmbProfiles", sender as PwGeneratorForm) as ComboBox;
			if (cbProfile == null) lMsg.Add("Could not locate m_cmbProfiles");
			else
			{
				cbProfile.SelectedIndexChanged += PwProfile1PerSet.EnablePwGeneratorControls;
				lMsg.Add("Found and hooked " + cbProfile.Name);
			}
			if (cGroup == null) lMsg.Add("Could not locate m_grpCurOpt");
			else
			{
				foreach (Control c in cGroup.Controls)
				{
					if (c is RadioButton) (c as RadioButton).CheckedChanged += PwProfile1PerSet.EnablePwGeneratorControls;
					else if (c is CheckBox) (c as CheckBox).CheckedChanged += PwProfile1PerSet.EnablePwGeneratorControls;
					else if (c is ComboBox) (c as ComboBox).SelectedIndexChanged += PwProfile1PerSet.EnablePwGeneratorControls;
					else if (c is TextBox) (c as TextBox).TextChanged += PwProfile1PerSet.EnablePwGeneratorControls;
					else if (c is NumericUpDown) (c as NumericUpDown).ValueChanged += PwProfile1PerSet.EnablePwGeneratorControls;
					else continue;
					lMsg.Add("Found and hooked " + c.Name);
				}
			}
			PluginDebug.AddInfo("Prepare PwGenerator", 0, lMsg.ToArray());
			if ((cbProfile != null) && (m_pweForm != null) && !m_pweForm.Disposing && !m_pweForm.IsDisposed)
			{
				string sProfile = m_pweForm.CustomDataGetSafe(Config.ProfileLastUsedProfile);
				if (!string.IsNullOrEmpty(sProfile) && cbProfile.Items.Contains(sProfile))
				{
					cbProfile.SelectedIndex = cbProfile.Items.IndexOf(sProfile);
				}
			}
			PwProfile1PerSet.EnablePwGeneratorControls(sender, e);
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			if (sender is PwGeneratorForm)
			{
				PwGeneratorForm pgf = sender as PwGeneratorForm;
				if (pgf.DialogResult == DialogResult.OK)
				{
					ProfileClicked(sender, new ToolStripItemClickedEventArgs(new ToolStripMenuItem(pgf.SelectedProfile.Name)));
					List<PwProfile> dbProfiles = Program.Config.PasswordGenerator.UserProfiles.GetDBProfiles();
					bool changed = false;
					UpdateDBProfiles(m_profiles, dbProfiles, out changed);
					if (changed) m_host.MainWindow.UpdateUI(false, null, false, null, false, null, true);
				}
			}
			else if (sender is PwEntryForm)
			{
				m_pweForm = null;
			}
		}

		#region PCA form
		private void CreateContextMenu()
		{
			m_host.MainWindow.EntryContextMenu.Opening += OnEntryMenuOpening;
			m_ContextMenuPCA = new ToolStripMenuItem(PluginTranslate.PluginName + "...");
			m_ContextMenuPCA.Click += OnShowPCAForm;
			m_ContextMenuPCA.Image = Config.ScaleImage(Resources.pca);
			m_host.MainWindow.EntryContextMenu.Items.Insert(m_host.MainWindow.EntryContextMenu.Items.Count, m_ContextMenuPCA);

			m_MainMenuPCA = new ToolStripMenuItem(PluginTranslate.PluginName + "...");
			m_MainMenuPCA.Click += OnShowPCAForm;
			m_MainMenuPCA.Image = Config.ScaleImage(Resources.pca);
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
			if (m_host.MainWindow.GetSelectedEntriesCount() != 1) return;
			m_pcaForm = new PCADialog();
			PCAInitData pcadata = new PCAInitData(m_host.MainWindow.GetSelectedEntry(true));
			m_pcaForm.Init(pcadata, ProfilesOpening);
			Config.OpenUrlForPwChange_Init();
			if (Config.OpenUrlForPwChange ^ ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)) 
				pcadata.OpenURL();
			if (m_pcaForm.ShowDialog(m_host.MainWindow) == DialogResult.OK)
			{
				pcadata.Entry.CreateBackup(Program.MainForm.ActiveDatabase);
				pcadata.Entry.Strings.Set(PwDefs.PasswordField, m_pcaForm.NewPassword);
				pcadata.Entry.Expires = m_pcaForm.EntryExpiry.Checked;
				if (pcadata.Entry.Expires)
					pcadata.Entry.ExpiryTime = m_pcaForm.EntryExpiry.Value.ToUniversalTime();
				pcadata.Entry.CustomDataSet(Config.PCASequence, m_pcaForm.Sequence);
				pcadata.Entry.CustomDataSet(Config.PCAURLField, m_pcaForm.PCAURL);
				if (m_pcaForm.Profile == "(" + KPRes.AutoGeneratedPasswordSettings + ")")
					pcadata.Entry.CustomDataSet(Config.ProfileLastUsedProfile, Config.ProfileAutoGenerated);
				else if (m_pcaForm.Profile == "(" + KPRes.GenPwBasedOnPrevious + ")")
					pcadata.Entry.CustomDataRemove(Config.ProfileLastUsedProfile);
				else
					pcadata.Entry.CustomDataSet(Config.ProfileLastUsedProfile, m_pcaForm.Profile);
				pcadata.Entry.Touch(true, false);
				Tools.RefreshEntriesList(true);
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
		private void PrepareEntryForm(PwEntryForm pef)
		{
			pef.Activated += (o, e) =>
			{
				m_pweForm = o as PwEntryForm;
			};
			m_pweForm = pef;
			if (Tools.KeePassVersion < Config.KP_Version_2_50)
			{
				pef.PasswordGeneratorContextMenu.Opening += ProfilesOpening;
				pef.PasswordGeneratorContextMenu.ItemClicked += ProfileClicked;
			}
			else pef.Shown += PreparePwGenButton;
			
			pef.FormClosed += OnFormClosed;
			LoadDBProfiles();

			//try to get the edit mode
			var spb = DoShowPCAButton();
			if (spb == ShowPCAButton.Error) pef.Shown += OnEntryFormShown; //Reading the edit mode failed, try this as fallback
			else if (spb == ShowPCAButton.NoShowMultiple)
			{
				PluginDebug.AddInfo("Multiple entries selected - No PCA button added", 0);
				return;
			}
			else if (spb == ShowPCAButton.NoShowOthers)
			{
				PluginDebug.AddInfo("Neither CREATE nor EDIT mode - No PCA button added", 0);
				return;
			}

			pef.Resize += OnEntryFormResize;

			//create plugin button right below the button for generating a new password
			CreatePCAButton();

			#region add context menu to plugin button
			ProtectedString psOldPw = ProtectedString.EmptyEx;
			if (m_host.MainWindow.GetSelectedEntriesCount() == 1)
			psOldPw = m_host.MainWindow.GetSelectedEntry(true).Strings.GetSafe(PwDefs.PasswordField);
			ContextMenuStrip cmsPCA = new ContextMenuStrip();
			ToolStripMenuItem menuitem = new ToolStripMenuItem(PluginTranslate.OldPWCopy, m_btnPCA.Image, PasswordCopyClick);
			menuitem.Name = PluginTranslate.PluginName + "OldCopy";
			menuitem.Tag = psOldPw; 
			cmsPCA.Items.Add(menuitem);
			menuitem = new ToolStripMenuItem(PluginTranslate.OldPWType, m_btnPCA.Image, PasswordTypeClick);
			menuitem.Name = PluginTranslate.PluginName + "OldType";
			menuitem.Tag = psOldPw;
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
			cmsPCA.Opening += cmsPCAOpening;
			m_btnPCA.Click += (o, x) => m_btnPCA.ContextMenuStrip.Show(m_btnPCA, 0, m_btnPCA.Height);
			#endregion

			//finally add the button to the form
			m_btnPCA.BringToFront();
		}

        private void PreparePwGenButton(object sender, EventArgs e)
        {
			(sender as Form).Shown -= PreparePwGenButton;
			List<string> lMsg = new List<string>();
			Button bGenPw = Tools.GetControl("m_btnGenPw", sender as Form) as Button;
			if (bGenPw == null)
			{
				PluginDebug.AddError("Hooking m_btnGenPw", 0, "Could not locate m_btnGenPw");
				return;
			}
			var oldEventHanderClick = bGenPw.GetEventHandlers("Click");
			if (oldEventHanderClick.Count != 1)
            {
				PluginDebug.AddError("Hooking m_btnGenPw", 0, "m_btnGenPw has more than one click handler");
				return;
			}
			
			bGenPw.RemoveEventHandlers("Click", oldEventHanderClick);
			bGenPw.Click += OnPwGenClick;
			bGenPw.Tag = oldEventHanderClick[0].Target;
			PluginDebug.AddSuccess("Hooking m_btnGenPw", 0);
		}

		//Code taken from PwGeneratorMenu.OnHostBtnClick
		private void OnPwGenClick(object sender, EventArgs e)
		{
			Button bGenPw = sender as Button;
			if (bGenPw == null)
			{
				PluginDebug.AddError("Hooking password profile display", 0, "Invalid source object", sender == null ? "null" : sender.GetType().ToString());
				return;
			}

			object myPwGeneratorMenu = bGenPw.Tag;
			if (myPwGeneratorMenu == null)
			{
				PluginDebug.AddError("Hooking password profile display", 0, "No PwGeneratorMenu found");
				return;
			}

			var mConstructContextMenu = myPwGeneratorMenu.GetType().GetMethod("ConstructContextMenu", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (mConstructContextMenu == null)
			{
				PluginDebug.AddError("Hooking password profile display", 0, "Method ConstructContextMenu not found");
				return;
			}
			mConstructContextMenu.Invoke(myPwGeneratorMenu, null);

			ContextMenuStrip ctx = Tools.GetField("m_ctx", myPwGeneratorMenu) as ContextMenuStrip;
			if (ctx == null)
			{
				PluginDebug.AddError("Hooking password profile display", 0, "Context menu not found");
				return;
			}

			ctx.Opening += ProfilesOpening;
			ctx.ItemClicked += ProfileClicked;
			PluginDebug.AddSuccess("Hooking password profile display", 0);

			if (ctx is CustomContextMenuStripEx) (ctx as CustomContextMenuStripEx).ShowEx(bGenPw);
			else
            {
				int dx = 0;
				if (bGenPw.RightToLeft == RightToLeft.Yes)
				{
					ctx.RightToLeft = RightToLeft.Yes;
					dx = bGenPw.Width;
				}
				ctx.Show(bGenPw, dx, bGenPw.Height);
			}
		}

        private void cmsPCAOpening(object sender, EventArgs e)
		{
			ContextMenuStrip cmsPCA = sender as ContextMenuStrip;
			if (cmsPCA == null) return;
			cmsPCA.Opening -= cmsPCAOpening;
			if (m_pweForm.Text == KPRes.AddEntry)
			{
				cmsPCA.Items.RemoveByKey(PluginTranslate.PluginName + "OldCopy");
				cmsPCA.Items.RemoveByKey(PluginTranslate.PluginName + "OldType");
				cmsPCA.Items.RemoveByKey(PluginTranslate.PluginName + "ShowPCAFormFromEntry");
			}
		}

		private void CreatePCAButton()
		{
			Control btnHide = Tools.GetControl("m_cbHidePassword", m_pweForm);
			Control btnPwGen = Tools.GetControl("m_btnGenPw", m_pweForm);
			Control btnQualityCheck = Tools.GetControl("m_cbQualityCheck", m_pweForm);
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
			if (btnQualityCheck != null)
			{
				btnQualityCheck.Width /= 2;
				m_btnPCA.Width /= 2;
				m_btnPCA.Left += btnQualityCheck.Width + 1;
			}
			m_btnPCA.Image = Config.ScaleImage(Resources.pca, 16, 16);

			if (btnPwGen != null) btnPwGen.Parent.Controls.Add(m_btnPCA);
			else if (btnHide != null) btnHide.Parent.Controls.Add(m_btnPCA);
		}

		private void ShowPCAFormFromEntry(object sender, EventArgs e)
		{
			m_pcaForm = new PCADialog();
			PCAInitData pcadata = new PCAInitData(m_pweForm);
			ExpiryControlGroup ecg = (ExpiryControlGroup)Tools.GetField("m_cgExpiry", m_pweForm);
			if (ecg != null)
			{
				pcadata.Expires = ecg.Checked;
				pcadata.Expiry = ecg.Value;
				pcadata.SetExpiry = (pcadata.Expires != m_pweForm.EntryRef.Expires) || (pcadata.Expiry != m_pweForm.EntryRef.ExpiryTime);
			}
			m_pcaForm.Init(pcadata, ProfilesOpening);
			Config.OpenUrlForPwChange_Init();
			if (Config.OpenUrlForPwChange ^ ((Control.ModifierKeys & Keys.Shift) == Keys.Shift))
				pcadata.OpenURL();
			m_pweForm.UpdateEntryStrings(true, true);
			if (m_pcaForm.ShowDialog(m_pweForm) == DialogResult.OK)
			{
				m_pweForm.EntryStrings.Set(PwDefs.PasswordField, m_pcaForm.NewPassword);
				if (ecg != null)
				{
					ecg.Checked = m_pcaForm.EntryExpiry.Checked;
					if (ecg.Checked) ecg.Value = m_pcaForm.EntryExpiry.Value.ToUniversalTime();
				}
				m_pweForm.UpdateEntryStrings(false, true);
				m_pweForm.CustomDataSet(Config.PCASequence, m_pcaForm.Sequence);
				m_pweForm.CustomDataSet(Config.PCAURLField, m_pcaForm.PCAURL);
				m_pweForm.CustomDataSet(Config.ProfileLastUsedProfile, m_pcaForm.Profile);
			}
			m_pcaForm.CleanupEx();
			m_pcaForm = null;
		}

		private enum ShowPCAButton
		{
			Error,
			ShowEditSingle,
			ShowAddSingle,
			NoShowMultiple,
			NoShowOthers
		}
		private ShowPCAButton DoShowPCAButton()
		{
			ShowPCAButton spb = ShowPCAButton.Error;
			if (m_pweForm == null) return spb;

			PwEditMode m = PwEditMode.Invalid;
			PropertyInfo pi = typeof(PwEntryForm).GetProperty("EditModeEx");
			if (pi != null)
			{ //will work starting with KeePass 2.41, preferred way as it's a public attribute
				m = (PwEditMode)pi.GetValue(m_pweForm, null);
			}
			else
			{ // try reading private field
				m = (PwEditMode)Tools.GetField("m_pwEditMode", m_pweForm);
			}
			if (m == PwEditMode.Invalid) return spb;
			spb = ShowPCAButton.NoShowOthers;
			if (m != PwEditMode.EditExistingEntry && m != PwEditMode.AddNewEntry) return spb;

			spb = ShowPCAButton.NoShowMultiple;
			pi = typeof(PwEntryForm).GetProperty("MultipleValuesEntryContext");
			object mvec = null;
			if (pi != null) mvec = pi.GetValue(m_pweForm, null);
			else mvec = Tools.GetField("m_mvec", m_pweForm);
			if (mvec != null) return spb; //NULL in case only one entry is edited/displayed

			spb = m == PwEditMode.AddNewEntry ? ShowPCAButton.ShowAddSingle : ShowPCAButton.ShowEditSingle;

			return spb;
		}

		private void OnEntryFormShown(object sender, EventArgs e)
		{
			m_btnPCA.Visible = ((sender as Form).Text == KPRes.EditEntry) || ((sender as Form).Text == KPRes.AddEntry);
			m_btnPCA.Visible &= m_host.MainWindow.GetSelectedEntriesCount() == 1;
			(sender as Form).Shown -= OnEntryFormShown;
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
			ToolStripMenuItem c = sender as ToolStripMenuItem;

			PwEntry pe = null;
			Form f = GetFormFromSender(c);
			if (f is PwEntryForm) pe = (f as PwEntryForm).EntryRef;
			else if (f is PCADialog) pe = (f as PCADialog).PCAData.Entry;
			else return;
			
			if (c.Name.Contains("Old"))
				PasswordType(pe, c.Tag as ProtectedString);
			else if (c.Name.Contains("New"))
			{
				SecureTextBoxEx newPassword = (SecureTextBoxEx)Tools.GetControl("m_tbPassword", m_pweForm);
				PasswordType(pe, newPassword.TextEx);
			}
		}

		private void PasswordCopyClick(object sender, EventArgs e)
		{
			ToolStripMenuItem c = sender as ToolStripMenuItem;

			PwEntry pe = null;
			Form f = GetFormFromSender(c);
			if (f is PwEntryForm) pe = (f as PwEntryForm).EntryRef; //New entry
			else if (f is PCADialog) pe = (f as PCADialog).PCAData.Entry;
			else return;

			if (c.Name.Contains("Old"))
				PasswordCopy(pe, c.Tag as ProtectedString);
			else if (c.Name.Contains("New"))
			{
				SecureTextBoxEx newPassword = (SecureTextBoxEx)Tools.GetControl("m_tbPassword", m_pweForm);
				PasswordCopy(pe, newPassword.TextEx);
			}
		}

		private Form GetFormFromSender(ToolStripMenuItem sender)
		{
			try { return (sender.Owner as ContextMenuStrip).SourceControl.FindForm(); }
			catch { return null; }
		}
		#endregion

		#region password change functionality
		internal static void PasswordType(PwEntry pe, ProtectedString ps)
		{
			var bAddedDelay = AddDelay();

			/* The password string may contain special chars and/or field references
			 * Easiest way is to set the provided value as new string field
			 * and have KeePass do the rest
			*/
			SetPasswordField(pe, ps);
			if (bAddedDelay)
				AutoType.PerformIntoCurrentWindow(pe,
				Program.MainForm.DocumentManager.SafeFindContainerOf(pe), Config.PCAPluginFieldRef);
			else
				AutoType.PerformIntoPreviousWindow(Program.MainForm, pe,
					Program.MainForm.DocumentManager.SafeFindContainerOf(pe), Config.PCAPluginFieldRef);
			SetPasswordField(pe, null);
		}

		private static bool AddDelay()
		{
			//https://github.com/Rookiestyle/PasswordChangeAssistant/issues/16
			var iDelay = Config.AutotypeDelay;
			if (iDelay < 1) return false;
			PluginDebug.AddInfo("Adding Auto-Type delay of " + iDelay.ToString() + " seconds");
			System.Threading.Thread.Sleep(iDelay * 1000);
			return true;
		}

		internal static void PasswordCopy(PwEntry pe, ProtectedString ps)
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
					SetPasswordField(pe, ps);
					if (ClipboardUtil.CopyAndMinimize(Config.PCAPluginFieldRef, true, Program.MainForm,
							pe, Program.MainForm.DocumentManager.SafeFindContainerOf(pe)))
						Program.MainForm.StartClipboardCountdown();
					SetPasswordField(pe, null);
				}
				catch { }
				if (bMACC)
					Program.Config.MainWindow.MinimizeAfterClipboardCopy = bMACC;
			}
			else
			{
				SetPasswordField(pe, ps);
				if (ClipboardUtil.CopyAndMinimize(Config.PCAPluginFieldRef, true, GlobalWindowManager.TopWindow,
						pe, Program.MainForm.DocumentManager.SafeFindContainerOf(pe)))
					Program.MainForm.StartClipboardCountdown();
				SetPasswordField(pe, null);
			}
		}

		internal static void SequenceType(PwEntry pe, ProtectedString ps, string sequence)
		{
			AddDelay();
			/* The password string may contain special chars and/or field references
				* Easiest way is to set the provided value as new string field
				* and have KeePass do the rest
			*/
			SetPasswordField(pe, ps);
			sequence = sequence.Replace(Config.PlaceholderOldPW, "{PASSWORD}");
			sequence = sequence.Replace(Config.PlaceholderNewPW, Config.PCAPluginFieldRef);
			AutoType.PerformIntoPreviousWindow(Program.MainForm, pe,
				Program.MainForm.ActiveDatabase, sequence);
			SetPasswordField(pe, null);
		}

		internal static string GetPCASequence(PwEntry e, string sDefault)
		{
			string result = e.CustomDataGetSafe(Config.PCASequence);
			if (string.IsNullOrEmpty(result)) result = sDefault;
			return result;
		}

		internal static void SetPasswordField(PwEntry pe, ProtectedString ps)
		{
			pe.Strings.Remove(Config.PCAPluginField);
			if (ps != null) pe.Strings.Set(Config.PCAPluginField, ps);
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

		private void HandleProfileSaveForm(SingleLineEditForm slef)
		{
			if (slef == null) return;

			//SingleLineEditForm is used multiple times
			//Check whether it's called from with the PwGeneratorForm
			System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
			if (st.GetFrames().SingleOrDefault(x => x.GetMethod().Name.Contains("OnBtnProfileSave")) == null) return;

			ComboBox cb = Tools.GetField("m_cmbEdit", slef) as ComboBox;
			if (cb == null)
			{
				PluginDebug.AddError("Cound not add 'Save db-specific' checkbox", 0, "m_cmbEdit not found");
				return;
			}
			CheckBox cbDB = new CheckBox();
			cbDB.Left = cb.Left;
			cbDB.Top = DpiUtil.ScaleIntX(146);
			Button bOK = Tools.GetField("m_btnOK", slef) as Button;
			if (bOK != null) cbDB.Top = bOK.Top + (int)((bOK.Height - cbDB.Height) / 2);
			cbDB.Text = PluginTranslate.SaveProfileInDB;
			cbDB.AutoSize = true;
			cbDB.Enabled = (m_host.Database != null) && m_host.Database.IsOpen;
			cbDB.CheckedChanged += AdjustProfileName;
			cb.Parent.SuspendLayout();
			cb.Parent.Controls.Add(cbDB);
			cb.Parent.ResumeLayout();
			cb.Parent.PerformLayout();

			cb.TextChanged += (o, e) =>
			{
				cbDB.CheckedChanged -= AdjustProfileName;
				cbDB.Checked = cbDB.Enabled && (cb.Text.EndsWith(Config.ProfileDBOnly));
				cbDB.CheckedChanged += AdjustProfileName;
			};
		}

		private void AdjustProfileName(object sender, EventArgs e)
		{
			CheckBox cbDB = sender as CheckBox;
			if (cbDB == null) return;
			ComboBox cb = Tools.GetField("m_cmbEdit", cbDB.FindForm()) as ComboBox;
			if (cbDB.Checked) cb.Text += Config.ProfileDBOnly;
			else if (cb.Text.EndsWith(Config.ProfileDBOnly)) cb.Text = cb.Text.Substring(0, cb.Text.Length - Config.ProfileDBOnly.Length);
		}

		private void OptionsFormShown(object sender, Tools.OptionsFormsEventArgs e)
		{
			PwProfSyncForm form = new PwProfSyncForm();
			form.SetHomeDB(m_host.Database);
			form.cbOpenUrlForPwChange.Checked = Config.OpenUrlForPwChange;
			form.nupAutotypeDelay.Value = Config.AutotypeDelay;
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
			Config.OpenUrlForPwChange = form.cbOpenUrlForPwChange.Checked;
			Config.AutotypeDelay = (int)form.nupAutotypeDelay.Value;
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
				//Tools.ShowError(PluginTranslate.NoDB);
				//return;
			}
			Tools.ShowOptions();
		}
		#endregion

		#region Show special icon for used password profile
		private void ProfileClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if ((m_pweForm == null) || string.IsNullOrEmpty(e.ClickedItem.Text)) return;
			if (e.ClickedItem.Name == "m_ctxPwGenOpen") return;
			List<PwProfile> profiles = PwGeneratorUtil.GetAllProfiles(false);
			string prof = e.ClickedItem.Text.Replace("&", "");
			PwProfile profile = profiles.Find(x => x.Name == prof);
			if (profile != null)
				m_pweForm.CustomDataSet(Config.ProfileLastUsedProfile, prof);
			else if (prof == "(" + KPRes.AutoGeneratedPasswordSettings + ")")
				m_pweForm.CustomDataSet(Config.ProfileLastUsedProfile, Config.ProfileAutoGenerated);
			else
				m_pweForm.CustomDataSet(Config.ProfileLastUsedProfile, string.Empty);
		}

		private void ProfilesOpening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			string lastProfile = string.Empty;
			string pcaFormProfile = m_pcaForm == null ? string.Empty : m_pcaForm.Profile;
			if (m_pweForm != null)
			{
				lastProfile = m_pweForm.CustomDataGetSafe(Config.ProfileLastUsedProfile);
				PluginDebug.AddInfo("Profiles opened in entry form", 0,
					"Last used profile:" + lastProfile);
			}
			else
			{
				PwEntry entry = m_host.MainWindow.GetSelectedEntry(true);
				if (entry != null) lastProfile = entry.CustomDataGetSafe(Config.ProfileLastUsedProfile);
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
			m_host.MainWindow.FileOpened -= OnFileOpened;
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
				return KeePassLib.Utility.GfxUtil.ScaleImage(Resources.pca, DpiUtil.ScaleIntX(16), DpiUtil.ScaleIntY(16));
			}
		}
	}
}