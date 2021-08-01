using KeePass.Forms;
using KeePassLib;
using KeePassLib.Collections;
using System.Windows.Forms;

namespace PluginTools
{
	public static partial class Tools
	{
		public static bool CustomDataExists(this PwEntry pe, string sKey)
		{
			return pe != null && pe.CustomData.Exists(sKey);
		}

		public static void CustomDataSet(this PwEntry pe, string sKey, string sValue)
		{
			if (pe == null) return;
			if (string.IsNullOrEmpty(sValue))
				pe.CustomData.Remove(sKey);
			else
				pe.CustomData.Set(sKey, sValue);
		}

		public static string CustomDataGetSafe(this PwEntry pe, string sKey)
		{
			if (pe == null) return string.Empty;
			if (!pe.CustomDataExists(sKey)) return string.Empty;
			return pe.CustomData.Get(sKey);
		}

		public static bool CustomDataRemove(this PwEntry pe, string sKey)
		{
			if (pe == null) return false;
			return pe.CustomData.Remove(sKey);
		}

		public static bool CustomDataExists(this PwEntryForm pef, string sKey)
		{
			if (pef == null) return false;
			var dCustomData = (StringDictionaryEx)Tools.GetField("m_sdCustomData", pef);
			return dCustomData != null && dCustomData.Exists(sKey);
		}

		public static void CustomDataSet(this PwEntryForm pef, string sKey, string sValue)
		{
			if (string.IsNullOrEmpty(sValue))
			{
				CustomDataRemove(pef, sKey);
				return;
			}
			var dCustomData = (StringDictionaryEx)Tools.GetField("m_sdCustomData", pef);
			var lvCustomData = (ListView)Tools.GetControl("m_lvCustomData", pef);

			if (dCustomData == null || lvCustomData == null) return;

			//Update StringDictionary
			dCustomData.Set(sKey, sValue);

			//Update ListView
			for (int i = 0; i < lvCustomData.Items.Count; i++)
			{
				if (lvCustomData.Items[i].SubItems[0].Text != sKey) continue;

				//Entry already exists
				lvCustomData.Items[i].SubItems[1].Text = sValue;

				//We're done
				return;
			}

			//Add entry
			ListViewItem lvi = lvCustomData.Items.Add(sKey);
			lvi.SubItems.Add(sValue);
		}
	
		public static string CustomDataGetSafe(this PwEntryForm pef, string sKey)
		{
			if (pef == null) return string.Empty;
			var dCustomData = (StringDictionaryEx)Tools.GetField("m_sdCustomData", pef);

			if (dCustomData == null || !dCustomData.Exists(sKey)) return string.Empty;
			return dCustomData.Get(sKey);
		}

		public static bool CustomDataRemove(this PwEntryForm pef, string sKey)
		{
			var dCustomData = (StringDictionaryEx)Tools.GetField("m_sdCustomData", pef);
			var lvCustomData = (ListView)Tools.GetControl("m_lvCustomData", pef);

			if (dCustomData == null || lvCustomData == null) return false;

			//Update StringDictionary
			bool bRemoved = dCustomData.Remove(sKey);

			//Update ListView
			for (int i = 0; i < lvCustomData.Items.Count; i++)
			{
				if (lvCustomData.Items[i].SubItems[0].Text != sKey) continue;

				lvCustomData.Items.RemoveAt(i);
				break;
			}
			return bRemoved;
		}
	}
}