using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using KeePass;
using KeePass.UI;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Security;
using PluginTools;
using PluginTranslation;

namespace PasswordChangeAssistant
{
  public class PCAInitData
  {
    private PwEntry m_pe = null;
    public PwEntry Entry { get { return m_pe; } }
    public string Title;
    public string User;
    public ProtectedString OldPassword;
    public string MainURL;
    public string PCAURL;
    public DateTime Expiry;
    public bool Expires;
    public bool SetExpiry;
    public string PCASequence;

    private static MethodInfo m_miSprCompileFn = null;
    private static MethodInfo m_miGetEntryListSprContext = null;

    static PCAInitData()
    {
      Type t = typeof(Program).Assembly.GetType("KeePass.UI.AsyncPwListUpdate");
      m_miSprCompileFn = t.GetMethod("SprCompileFn", BindingFlags.Static | BindingFlags.NonPublic);
      m_miGetEntryListSprContext = typeof(KeePass.Forms.MainForm).GetMethod("GetEntryListSprContext", BindingFlags.Static | BindingFlags.NonPublic);
    }

    public string GetDereferenced(string sUrl)
    {
      if (m_miSprCompileFn == null && m_miGetEntryListSprContext == null) return sUrl;
      if (!sUrl.Contains("{")) return sUrl;
      if (m_miGetEntryListSprContext != null)
      {
        var ctx = (SprContext)m_miGetEntryListSprContext.Invoke(Program.MainForm, new object[] { Entry, Program.MainForm.DocumentManager.SafeFindContainerOf(Entry) });
        sUrl = SprEngine.Compile(sUrl, ctx);
      }
      else
      {
        PwListItem pli = new PwListItem(Entry);
        bool bEntryListShowDerefDataAndRefs = Program.Config.MainWindow.EntryListShowDerefDataAndRefs;
        Program.Config.MainWindow.EntryListShowDerefDataAndRefs = false;
        sUrl = (string)m_miSprCompileFn.Invoke(null, new object[] { sUrl, pli });
        Program.Config.MainWindow.EntryListShowDerefDataAndRefs = bEntryListShowDerefDataAndRefs;
      }
      return sUrl;
    }

    public PCAInitData(PwEntry pe)
    {
      PCAInitData_Intern(pe);
    }

    public PCAInitData(KeePass.Forms.PwEntryForm pef)
    {
      if (pef == null) return;
      PCAInitData_Intern(pef.EntryRef);
      if (pef.CustomDataExists(Config.PCASequence))
        PCASequence = pef.CustomDataGetSafe(Config.PCASequence);
      PCAURL = pef.CustomDataGetSafe(Config.PCAURLField);
    }

    private void PCAInitData_Intern(PwEntry pe)
    {
      if (pe == null) return;
      m_pe = pe;
      Title = pe.Strings.ReadSafe(PwDefs.TitleField);
      User = pe.Strings.ReadSafe(PwDefs.UserNameField);
      Expires = pe.Expires;
      Expiry = pe.ExpiryTime;
      OldPassword = pe.Strings.GetSafe(PwDefs.PasswordField);
      PCASequence = PasswordChangeAssistantExt.GetPCASequence(pe, Config.DefaultPCASequences[PluginTranslate.DefaultSequence01]);
      SetExpiry = false;
      MainURL = pe.Strings.ReadSafe(PwDefs.UrlField);
      PCAURL = pe.CustomDataGetSafe(Config.PCAURLField);
    }

    public void OpenURL()
    {
      string URL = PCAURL;
      if (string.IsNullOrEmpty(URL)) URL = MainURL;
      if (string.IsNullOrEmpty(URL)) return;

      URL = GetDereferenced(URL);

      KeePass.Util.WinUtil.OpenUrl(URL, m_pe, true);
    }
  }

  internal class DataMigration
  {
    [Flags]
    private enum Migrations
    { //Update CheckAndMigrate(PwDatabase db) if changes are done here
      None = 0,
      Entry2CustomData = 1,
    }

    public static bool CheckAndMigrate(PwDatabase db)
    {
      //Do NOT create a 'ALL' flag as this will be stored as 'ALL' and by that, no additional migrations would be done
      Migrations m = Migrations.None;
      foreach (var v in Enum.GetValues(typeof(Migrations))) m |= (Migrations)v;
      return CheckAndMigrate(db, m);
    }

    /// <summary>
    /// Perform all kind of migrations between different KeePassOTP versions
    /// </summary>
    /// <param name="db"></param>
    /// <returns>true if something was migrated, false if nothing was done</returns>
    private static bool CheckAndMigrate(PwDatabase db, Migrations omFlags)
    {
      string sMigration = "PCA.MigrationStatus";
      bool bMigrated = false;

      Migrations mStatusOld;
      try { mStatusOld = (Migrations)Enum.Parse(typeof(Migrations), db.CustomData.Get(sMigration), true); }
      catch { mStatusOld = Migrations.None; }
      Migrations mStatusNew = mStatusOld;

      if (MigrationRequired(Migrations.Entry2CustomData, omFlags, mStatusOld))
      {
        bMigrated |= MigrateEntry2CustomData(db) > 0;
        mStatusNew |= Migrations.Entry2CustomData;
      }

      if ((mStatusNew != mStatusOld) || bMigrated)
      {
        db.CustomData.Set(sMigration, mStatusNew.ToString());
        db.SettingsChanged = DateTime.UtcNow;
        db.Modified = true;
        KeePass.Program.MainForm.UpdateUI(false, null, false, null, false, null, KeePass.Program.MainForm.ActiveDatabase == db);
      }
      return bMigrated;
    }

    private static int MigrateEntry2CustomData(PwDatabase db)
    {
      int i = 0;
      var lEntries = db.RootGroup.GetEntries(true);
      foreach (PwEntry pe in lEntries)
      {
        i += MoveField(pe, "PCAURL", Config.PCAURLField);
        i += MoveField(pe, "PCASequence", Config.PCASequence);
        i += MoveField(pe, Config.ProfileLastUsedProfile, Config.ProfileLastUsedProfile);
      }
      return i;
    }

    private static int MoveField(PwEntry pe, string sFrom, string sTo)
    {
      string s = pe.Strings.ReadSafe(sFrom);
      if (string.IsNullOrEmpty(s)) return 0;
      pe.Strings.Remove(sFrom);
      pe.CustomDataSet(sTo, s);
      return 1;
    }

    private static bool MigrationRequired(Migrations mMigrate, Migrations mFlags, Migrations status)
    {
      if ((mMigrate & mFlags) != mMigrate) return false; //not requested
      if ((mMigrate & status) == mMigrate) return false; //already done
      return true;
    }
  }

  public static class Config
  {
    public const string PCAURLField = "PCA.URL";
    public const string PCAPluginField = "PCAPluginField";
    public const string PCAPluginFieldRef = "{S:" + PCAPluginField + "}";
    public const string PCASequence = "PCA.Sequence";
    public const string PlaceholderOldPW = "{PCA_OldPW}";
    public const string PlaceholderNewPW = "{PCA_NewPW}";
    public static Dictionary<string, string> DefaultPCASequences = new Dictionary<string, string>();
    public const string ProfileDBOnly = " (DB)";
    public const string ProfileCopied = "(*)";
    public const string ProfileLastUsedProfile = "PPS.Profile";
    public const string ProfileAutoGenerated = "PPS.Auto";
    public const string ProfileConfig = "PPS.";
    private const string _OpenUrlForPwChange = "PPS.OpenUrlForPwChange";
    private const string _HideBuiltInProfiles = "PCA.HideBuiltInProfiles";
    private const string _AutotypeDelay = "PCA.AutotypeDelay";

    public static Version KP_Version_2_50 = new Version(2, 50);

    public static bool OpenUrlForPwChange
    {
      get { return KeePass.Program.Config.CustomConfig.GetBool(_OpenUrlForPwChange, false); }
      set { KeePass.Program.Config.CustomConfig.SetBool(_OpenUrlForPwChange, value); }
    }

    public static bool HideBuiltInProfiles
    {
      get { return KeePass.Program.Config.CustomConfig.GetBool(_HideBuiltInProfiles, true); }
      set { KeePass.Program.Config.CustomConfig.SetBool(_HideBuiltInProfiles, value); }
    }

    public static int AutotypeDelay
    {
      //https://github.com/Rookiestyle/PasswordChangeAssistant/issues/16
      //Can't type into previous windows on some *nix
      //KeePass won't drop to background if a dialog is open
      get
      {
        var lDelay = KeePass.Program.Config.CustomConfig.GetLong(_AutotypeDelay, -1);
        if (lDelay >= 0) return (int)lDelay;
        if (!KeePassLib.Native.NativeLib.IsUnix()) return 0;
        PluginDebug.AddInfo("Detected Unix, setting default Auto-Type delay of 10 seconds");
        return 10;
      }
      set
      {
        if (value < 1) KeePass.Program.Config.CustomConfig.SetString(_AutotypeDelay, null);
        else KeePass.Program.Config.CustomConfig.SetLong(_AutotypeDelay, value);
      }
    }

    public static void OpenUrlForPwChange_Init()
    {
      if (KeePass.Program.Config.CustomConfig.GetBool(_OpenUrlForPwChange + "_Init", false)) return;
      KeePass.Program.Config.CustomConfig.SetBool(_OpenUrlForPwChange + "_Init", true);
      OpenUrlForPwChange = Tools.AskYesNo(PluginTranslate.OpenUrlForPwChangeExplanation, PluginTranslate.PluginName) == DialogResult.Yes;
    }

    internal static Image ScaleImage(Image img)
    {
      return ScaleImage(img, 16, 16);
    }

    internal static Image ScaleImage(Image img, int w, int h)
    {
      return KeePassLib.Utility.GfxUtil.ScaleImage(img, (int)(w * KeePass.UI.DpiUtil.FactorX), (int)(h * KeePass.UI.DpiUtil.FactorY));
    }
  }

  public class PEDCalcStub
  {
    private KeePass.Plugins.Plugin _pedcalc = null;

    private PropertyInfo _piActive = null;
    private MethodInfo _miGetPEDValue = null;

    public bool Loaded { get { return _pedcalc != null; } }
    public PEDCalcStub(KeePass.Plugins.Plugin p)
    {
      _pedcalc = p;
      Initialize();
    }

    private void Initialize()
    {
      if (_pedcalc == null) return;
      Type tC = _pedcalc.GetType().Assembly.GetType("PEDCalc.Configuration");
      if (tC != null)
      {
        _piActive = tC.GetProperty("Active", BindingFlags.Public | BindingFlags.Static);
      }
      Type tEE = _pedcalc.GetType().Assembly.GetType("PEDCalc.EntryExtensions");
      if (tEE != null)
      {
        _miGetPEDValue = tEE.GetMethod("GetPEDValue", BindingFlags.NonPublic | BindingFlags.Static);
      }
    }

    public bool Active { get { return IsActive(); } }

    public bool AdjustExpiryDateRequired(PwEntry pe)
    {
      if (_pedcalc == null) return false;
      if (_miGetPEDValue == null) return false;
      object pedNewExpireDate = _miGetPEDValue.Invoke(null, new object[] { pe, true });
      bool bOff = (bool)pedNewExpireDate.GetType().GetProperty("Off").GetValue(pedNewExpireDate, null);
      return !bOff;
    }

    public DateTime GetNewExpiryDateUtc(PwEntry pe, out object pedNewExpireDate)
    {
      pedNewExpireDate = new object();
      if (_pedcalc == null) return DateTime.MaxValue;
      if (_miGetPEDValue == null) return DateTime.MaxValue;
      pedNewExpireDate = _miGetPEDValue.Invoke(null, new object[] { pe, true });
      DateTime dtNewExpireDate = (DateTime)pedNewExpireDate.GetType().GetProperty("NewExpiryDateUtc").GetValue(pedNewExpireDate, null);
      return dtNewExpireDate;
    }

    private bool IsActive()
    {
      if (_pedcalc != null && _piActive != null) return (bool)_piActive.GetValue(null, null);
      return false;
    }
  }
}
