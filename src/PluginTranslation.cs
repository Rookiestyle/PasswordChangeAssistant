using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

using KeePass.Plugins;
using KeePass.Util;
using KeePassLib.Utility;

using PluginTools;
using System.Windows.Forms;

namespace PluginTranslation
{
	public class TranslationChangedEventArgs : EventArgs
	{
		public string OldLanguageIso6391 = string.Empty;
		public string NewLanguageIso6391 = string.Empty;

		public TranslationChangedEventArgs(string OldLanguageIso6391, string NewLanguageIso6391)
		{
			this.OldLanguageIso6391 = OldLanguageIso6391;
			this.NewLanguageIso6391 = NewLanguageIso6391;
		}
	}

	public static class PluginTranslate
	{
		public static long TranslationVersion = 0;
		public static event EventHandler<TranslationChangedEventArgs> TranslationChanged = null;
		private static string LanguageIso6391 = string.Empty;
		#region Definitions of translated texts go here
		public const string PluginName = "Password Change Assistant";
		/// <summary>
		/// Old password:
		/// </summary>
		public static readonly string OldPW = @"Old password:";
		/// <summary>
		/// New password:
		/// </summary>
		public static readonly string NewPW = @"New password:";
		/// <summary>
		/// Old password: Copy
		/// </summary>
		public static readonly string OldPWCopy = @"Old password: Copy";
		/// <summary>
		/// Old password: Auto-Type
		/// </summary>
		public static readonly string OldPWType = @"Old password: Auto-Type";
		/// <summary>
		/// New password: Copy
		/// </summary>
		public static readonly string NewPWCopy = @"New password: Copy";
		/// <summary>
		/// New password: Auto-Type
		/// </summary>
		public static readonly string NewPWType = @"New password: Auto-Type";
		/// <summary>
		/// {0}: {1} ({2})
		/// </summary>
		public static readonly string EntryInfo = @"{0}: {1} ({2})";
		/// <summary>
		/// {0}: {1}
		/// </summary>
		public static readonly string EntryInfoNoUsername = @"{0}: {1}";
		/// <summary>
		/// Change password
		/// </summary>
		public static readonly string ButtonChangePW = @"Change password";
		/// <summary>
		/// Password Change Sequence
		/// </summary>
		public static readonly string PCASequence = @"Password Change Sequence";
		/// <summary>
		/// Old Password - New Password - New Password
		/// </summary>
		public static readonly string DefaultSequence01 = @"Old Password - New Password - New Password";
		/// <summary>
		/// Old Password - New Page - New Password - New Password
		/// </summary>
		public static readonly string DefaultSequence02 = @"Old Password - New Page - New Password - New Password";
		/// <summary>
		/// New Password - New Password
		/// </summary>
		public static readonly string DefaultSequence03 = @"New Password - New Password";
		/// <summary>
		/// Entry-specific sequence
		/// </summary>
		public static readonly string EntrySpecificSequence = @"Entry-specific sequence";
		/// <summary>
		/// Change password
		/// </summary>
		public static readonly string ChangePassword = @"Change password";
		/// <summary>
		/// No database opened / database locked
		/// </summary>
		public static readonly string NoDB = @"No database opened / database locked";
		/// <summary>
		/// Copy profiles instead of moving profiles
		/// </summary>
		public static readonly string ModeCopy = @"Copy profiles instead of moving profiles";
		/// <summary>
		/// {0} is already defined
		/// Overwrite?
		/// </summary>
		public static readonly string OptionsOverwrite = @"{0} is already defined
Overwrite?";
		/// <summary>
		/// Active database: {0}
		/// </summary>
		public static readonly string OptionsActiveDB = @"Active database: {0}";
		/// <summary>
		/// Password Profile Sync
		/// </summary>
		public static readonly string Options = @"Password Profile Sync";
		/// <summary>
		/// KeePass config
		/// </summary>
		public static readonly string OptionsGlobal = @"KeePass config";
		/// <summary>
		/// Direct link to 'Change password' site
		/// </summary>
		public static readonly string URL2Hint = @"Direct link to 'Change password' site";
		/// <summary>
		/// Save db-specific
		/// </summary>
		public static readonly string SaveProfileInDB = @"Save db-specific";
		/// <summary>
		/// Password Profile Sync
		/// </summary>
		public static readonly string OptionsPWSyncFormCaption = @"Password Profile Sync";
		/// <summary>
		/// Password Change Assistant form is shown
		/// </summary>
		public static readonly string PWFormShownCaption = @"Password Change Assistant form is shown";
		/// <summary>
		/// Open password change site in browser
		/// </summary>
		public static readonly string OpenUrlForPwChange = @"Open password change site in browser";
		/// <summary>
		/// Hold down [Shift] to invert this behaviour.
		/// </summary>
		public static readonly string OpenUrlForPwChangeShift = @"Hold down [Shift] to invert this behaviour.";
		/// <summary>
		/// Password Change Assistant can automatically open the website to change this entry's password.
		/// 
		/// Do you want this to be active?
		/// You can change this in the plugin's options anytime.
		/// </summary>
		public static readonly string OpenUrlForPwChangeExplanation = @"Password Change Assistant can automatically open the website to change this entry's password.

Do you want this to be active?
You can change this in the plugin's options anytime.";
		/// <summary>
		/// Please use the options inside '{0}' to define password details
		/// </summary>
		public static readonly string ProfileUsageHint = @"Please use the options inside '{0}' to define password details";
		/// <summary>
		/// At least 1 per set *
		/// </summary>
		public static readonly string ProfileAtLeast1PerSet = @"At least one per set *";
		/// <summary>
		/// Auto-Type delay (seconds)
		/// </summary>
		public static readonly string AutotypeDelay = @"Auto-Type delay (seconds)";
		#endregion

		#region NO changes in this area
		private static StringDictionary m_translation = new StringDictionary();

		public static void Init(Plugin plugin, string LanguageCodeIso6391)
		{
			List<string> lDebugStrings = new List<string>();
			m_translation.Clear();
			bool bError = true;
			LanguageCodeIso6391 = InitTranslation(plugin, lDebugStrings, LanguageCodeIso6391, out bError);
			if (bError && (LanguageCodeIso6391.Length > 2))
			{
				LanguageCodeIso6391 = LanguageCodeIso6391.Substring(0, 2);
				lDebugStrings.Add("Trying fallback: " + LanguageCodeIso6391);
				LanguageCodeIso6391 = InitTranslation(plugin, lDebugStrings, LanguageCodeIso6391, out bError);
			}
			if (bError)
			{
				PluginDebug.AddError("Reading translation failed", 0, lDebugStrings.ToArray());
				LanguageCodeIso6391 = "en";
			}
			else
			{
				List<FieldInfo> lTranslatable = new List<FieldInfo>(
					typeof(PluginTranslate).GetFields(BindingFlags.Static | BindingFlags.Public)
					).FindAll(x => x.IsInitOnly);
				lDebugStrings.Add("Parsing complete");
				lDebugStrings.Add("Translated texts read: " + m_translation.Count.ToString());
				lDebugStrings.Add("Translatable texts: " + lTranslatable.Count.ToString());
				foreach (FieldInfo f in lTranslatable)
				{
					if (m_translation.ContainsKey(f.Name))
					{
						lDebugStrings.Add("Key found: " + f.Name);
						f.SetValue(null, m_translation[f.Name]);
					}
					else
						lDebugStrings.Add("Key not found: " + f.Name);
				}
				PluginDebug.AddInfo("Reading translations finished", 0, lDebugStrings.ToArray());
			}
			if (TranslationChanged != null)
			{
				TranslationChanged(null, new TranslationChangedEventArgs(LanguageIso6391, LanguageCodeIso6391));
			}
			LanguageIso6391 = LanguageCodeIso6391;
			lDebugStrings.Clear();
		}

		private static string InitTranslation(Plugin plugin, List<string> lDebugStrings, string LanguageCodeIso6391, out bool bError)
		{
			if (string.IsNullOrEmpty(LanguageCodeIso6391))
			{
				lDebugStrings.Add("No language identifier supplied, using 'en' as fallback");
				LanguageCodeIso6391 = "en";
			}
			string filename = GetFilename(plugin.GetType().Namespace, LanguageCodeIso6391);
			lDebugStrings.Add("Translation file: " + filename);

			if (!File.Exists(filename)) //If e. g. 'plugin.zh-tw.language.xml' does not exist, try 'plugin.zh.language.xml'
			{
				lDebugStrings.Add("File does not exist");
				bError = true;
				return LanguageCodeIso6391;
			}
			else
			{
				string translation = string.Empty;
				try { translation = File.ReadAllText(filename); }
				catch (Exception ex)
				{
					lDebugStrings.Add("Error reading file: " + ex.Message);
					LanguageCodeIso6391 = "en";
					bError = true;
					return LanguageCodeIso6391;
				}
				XmlSerializer xs = new XmlSerializer(m_translation.GetType());
				lDebugStrings.Add("File read, parsing content");
				try
				{
					m_translation = (StringDictionary)xs.Deserialize(new StringReader(translation));
				}
				catch (Exception ex)
				{
					string sException = ex.Message;
					if (ex.InnerException != null) sException += "\n" + ex.InnerException.Message;
					lDebugStrings.Add("Error parsing file: " + sException);
					LanguageCodeIso6391 = "en";
					MessageBox.Show("Error parsing translation file\n\n" + sException, PluginName, MessageBoxButtons.OK, MessageBoxIcon.Error);
					bError = true;
					return LanguageCodeIso6391;
				}
				bError = false;
				return LanguageCodeIso6391;
			}
		}

		private static string GetFilename(string plugin, string lang)
		{
			string filename = UrlUtil.GetFileDirectory(WinUtil.GetExecutable(), true, true);
			filename += KeePass.App.AppDefs.PluginsDir + UrlUtil.LocalDirSepChar + "Translations" + UrlUtil.LocalDirSepChar;
			filename += plugin + "." + lang + ".language.xml";
			return filename;
		}
		#endregion
	}

	#region NO changes in this area
	[XmlRoot("Translation")]
	public class StringDictionary : Dictionary<string, string>, IXmlSerializable
	{
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();
			if (wasEmpty) return;
			bool bFirst = true;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (bFirst)
				{
					bFirst = false;
					try
					{
						reader.ReadStartElement("TranslationVersion");
						PluginTranslate.TranslationVersion = reader.ReadContentAsLong();
						reader.ReadEndElement();
					}
					catch { }
				}
				reader.ReadStartElement("item");
				reader.ReadStartElement("key");
				string key = reader.ReadContentAsString();
				reader.ReadEndElement();
				reader.ReadStartElement("value");
				string value = reader.ReadContentAsString();
				reader.ReadEndElement();
				this.Add(key, value);
				reader.ReadEndElement();
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("TranslationVersion");
			writer.WriteString(PluginTranslate.TranslationVersion.ToString());
			writer.WriteEndElement();
			foreach (string key in this.Keys)
			{
				writer.WriteStartElement("item");
				writer.WriteStartElement("key");
				writer.WriteString(key);
				writer.WriteEndElement();
				writer.WriteStartElement("value");
				writer.WriteString(this[key]);
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
		}
	}
	#endregion
}