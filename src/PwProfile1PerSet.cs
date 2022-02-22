using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Security;
using PluginTools;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PasswordChangeAssistant
{
	internal class PwProfile1PerSet : CustomPwGenerator
	{
		private static string MyName = PluginTranslation.PluginTranslate.ProfileAtLeast1PerSet;
		public override string Name { get { return MyName; } }
		public override bool SupportsOptions { get { return true; } }

		public static PwUuid MyUuid = new PwUuid(KeePassLib.Utility.StrUtil.Utf8.GetBytes("PCA_Gen_1PerSet!")); //Must be 16 bytes

		public override PwUuid Uuid { get { return MyUuid; } }

		public override ProtectedString Generate(PwProfile prf, CryptoRandomStream crsRandomSource)
		{
			PwProfile p = InitProfile(prf);

			string sPattern = MapCharsets2Pattern(prf);
			p.Pattern = sPattern;
			int iLength = GetPatternLength(p.Pattern);

			//Add additionaly specified characters if any
			//Increase overall length of generated password by 1: 1 per set = at least 1 out of the additinaly specified characters
			string sAdditionalChars = GetAdditionalChars(prf);
			if (AddAdditionalChars(p, sAdditionalChars)) iLength++;

			//Calculate remaining length
			iLength = (int)p.Length - iLength;

			bool bSuccess;
			FinalizePattern(p, sPattern, sAdditionalChars, iLength, out bSuccess);

			if (!bSuccess) return ProtectedString.Empty;

			ProtectedString ps = ProtectedString.Empty;
			if (PwGenerator.Generate(out ps, p, crsRandomSource.GetRandomBytes(32), null) == PwgError.Success) return ps;
			return ProtectedString.Empty;
		}

		private int GetPatternLength(string sPattern)
		{
			//Some character classes will be escaped
			//Those \ must be ignored when calculating character length
			var sHelp = sPattern.Replace("\\\\", string.Empty);
			int l = (sPattern.Length - sHelp.Length) / 2;
			sHelp = sHelp.Replace("\\", string.Empty);
			l += sHelp.Length;
			return l;
		}

		private void FinalizePattern(PwProfile p, string sPattern, string sAdditionalChars, int iLength, out bool bSuccess)
		{
			bSuccess = false;
			if (iLength < 0) return; //Already longer than allowed
			bSuccess = true;
			if (iLength > 0)
			{
				sAdditionalChars = "[" + sPattern + sAdditionalChars + "]{" + iLength.ToString().Trim() + "}";
				p.Pattern += sAdditionalChars;
			}
		}

		private bool AddAdditionalChars(PwProfile p, string sAdditionalChars)
		{
			if (!string.IsNullOrEmpty(sAdditionalChars))
			{
				p.Pattern += "[" + sAdditionalChars + "]{1}";
				return true;
			}
			return false;
		}

		private string MapCharsets2Pattern(PwProfile prf)
		{
			string sPattern = string.Empty;
			if (prf.CharSetRanges[0] != '_') sPattern += "u";
			if (prf.CharSetRanges[1] != '_') sPattern += "l";
			if (prf.CharSetRanges[2] != '_') sPattern += "d";
			if (prf.CharSetRanges[3] != '_') sPattern += "s";
			if (prf.CharSetRanges[4] != '_') sPattern += "p";
			if (prf.CharSetRanges[5] != '_') sPattern += "\\-";
			if (prf.CharSetRanges[6] != '_') sPattern += "\\_";
			if (prf.CharSetRanges[7] != '_') sPattern += "\\ ";
			if (prf.CharSetRanges[8] != '_') sPattern += "b";
			if (prf.CharSetRanges[9] != '_') sPattern += "x";
			return sPattern;
		}

		private string GetAdditionalChars(PwProfile prf)
		{
			string sAdditionalChars = string.Empty;
			for (int i = 0; i < prf.CharSetAdditional.Length; i++)
				sAdditionalChars += "\\" + prf.CharSetAdditional[i]; //In a pattern, every character has to be escaped
			return sAdditionalChars;
		}

		private PwProfile InitProfile(PwProfile prf)
		{
			PwProfile p = prf.CloneDeep();

			p.GeneratorType = PasswordGeneratorType.Pattern;
			p.PatternPermutePassword = true;
			p.Pattern = string.Empty;

			return p;
		}

		public override string GetOptions(string strCurrentOptions)
		{
			if (KeePass.UI.GlobalWindowManager.TopWindow is KeePass.Forms.PwGeneratorForm)
			{
				Control c = Tools.GetControl("m_rbStandardCharSet", KeePass.UI.GlobalWindowManager.TopWindow);
				if (c != null)
				{
					string s = c.Text.Replace(":", string.Empty);
					s = s.Replace("&&", "\n");
					s = s.Replace("&", string.Empty);
					s = s.Replace("\n", "&");
					Tools.ShowInfo(string.Format(PluginTranslation.PluginTranslate.ProfileUsageHint, s));
				}
			}
			return base.GetOptions(strCurrentOptions);
		}

		public static void EnablePwGeneratorControls(object sender, EventArgs e)
		{
			Form f = (sender as Control).FindForm();
			if (f == null) return;
			ComboBox cmbCustomAlgo = Tools.GetControl("m_cmbCustomAlgo", f) as ComboBox;
			if (cmbCustomAlgo == null) return;
			if (!cmbCustomAlgo.Enabled) return;
			if (cmbCustomAlgo.SelectedIndex < 0) return;
			if ((cmbCustomAlgo.Items[cmbCustomAlgo.SelectedIndex] as string) != PwProfile1PerSet.MyName) return;

			Control cGroup = Tools.GetControl("m_grpCurOpt", f);
			foreach (Control c in cGroup.Controls)
			{
				if (c is RadioButton) (c as RadioButton).CheckedChanged -= EnablePwGeneratorControls;
				else if (c is CheckBox) (c as CheckBox).CheckedChanged -= EnablePwGeneratorControls;
				else if (c is ComboBox) (c as ComboBox).SelectedIndexChanged -= EnablePwGeneratorControls;
				else if (c is TextBox) (c as TextBox).TextChanged -= EnablePwGeneratorControls;
				else if (c is NumericUpDown) (c as NumericUpDown).ValueChanged -= EnablePwGeneratorControls;
				c.Enabled = true;
				if ((sender is TextBox) && (c is TextBox) && ((sender as TextBox).Name == (c as TextBox).Name)
					|| (sender is NumericUpDown) && (c is NumericUpDown) && ((sender as NumericUpDown).Name == (c as NumericUpDown).Name))
				{
					f.ActiveControl = c;
					c.Focus();
				}
				if (c is RadioButton) (c as RadioButton).CheckedChanged += EnablePwGeneratorControls;
				else if (c is CheckBox) (c as CheckBox).CheckedChanged += EnablePwGeneratorControls;
				else if (c is ComboBox) (c as ComboBox).SelectedIndexChanged += EnablePwGeneratorControls;
				else if (c is TextBox) (c as TextBox).TextChanged += PwProfile1PerSet.EnablePwGeneratorControls;
				else if (c is NumericUpDown) (c as NumericUpDown).ValueChanged += EnablePwGeneratorControls;
			}
		}
	}
}
