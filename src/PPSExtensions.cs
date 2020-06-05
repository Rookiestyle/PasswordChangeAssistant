using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using KeePassLib;
using KeePassLib.Cryptography.PasswordGenerator;

namespace PasswordChangeAssistant
{
	public static class PPSExtensions
	{
		public static bool IsDBOnlyProfile(this PwProfile profile)
		{
			return profile.Name.EndsWith(Config.ProfileDBOnly);
		}

		//Return DB specific profiles from global configuration
		public static List<PwProfile> GetDBProfiles(this List<PwProfile> profiles)
		{
			return profiles.FindAll(x => x.IsDBOnlyProfile());
		}

		//Return DB specific profiles stored in database
		public static List<PwProfile> GetDBProfiles(this PwDatabase db)
		{
			List<string> profileNames = db.GetDBProfileNames();
			List<PwProfile> profiles = new List<PwProfile>();
			foreach (string profileName in profileNames)
			{
				try
				{
					PwProfile profile = db.GetProfile(profileName);
					if (profile != null) profiles.Add(profile);
				}
				catch (Exception) { }
			}
			return profiles;
		}

		//Return names of password profiles stored in database
		public static List<String> GetDBProfileNames(this PwDatabase db)
		{
			List<string> profiles = new List<string>();
			foreach (KeyValuePair<string, string> kvp in db.CustomData)
			{
				if (!kvp.Key.StartsWith(Config.ProfileConfig)) continue;
				profiles.Add(kvp.Key.Substring(Config.ProfileConfig.Length));
			}
			return profiles;
		}

		//Return specific password profile stored in database
		public static PwProfile GetProfile(this PwDatabase db, string profileName)
		{
			return Deserialize(db.CustomData.Get(Config.ProfileConfig + profileName));
		}

		//Return specific password profile from global configuration
		public static PwProfile GetProfile(this List<PwProfile> profiles, string profileName)
		{
			foreach (PwProfile profile in profiles)
			{
				if (profile.Name == profileName) return profile;
			}
			return null;
		}

		//Add single password profile to global configuration
		public static void AddDBProfile(this List<PwProfile> profiles, PwProfile profile)
		{
			PwProfile myProfile = profile.CloneDeep();
			myProfile.Name += Config.ProfileDBOnly;
			profiles.Add(myProfile);
		}

		//Remove database specific profiles from global configuration
		public static void RemoveDBProfiles(this List<PwProfile> profiles)
		{
			profiles.RemoveAll(x => x.Name.EndsWith(Config.ProfileDBOnly));
		}

		//Remove database specific profiles from database
		public static void RemoveDBProfiles(this PwDatabase db)
		{
			var newCustomData = db.CustomData.CloneDeep();
			foreach (KeyValuePair<string, string> kvp in newCustomData)
				if (kvp.Key.StartsWith(Config.ProfileConfig))
					db.CustomData.Remove(kvp.Key);
		}

		//Remove database specific profile from database
		public static void RemoveDBProfile(this PwDatabase db, string profileName)
		{
			db.CustomData.Remove(Config.ProfileConfig + profileName);
		}

		//Convert passwort profile to atring
		public static string Serialize(this PwProfile profile)
		{
			using (StringWriter writer = new StringWriter())
			{
				XmlSerializer profSerialize = new XmlSerializer(typeof(PwProfile));
				profSerialize.Serialize(writer, profile);
				return writer.ToString();
			}
		}

		//Convert string to password profile
		public static PwProfile Deserialize(string profileData)
		{
			try
			{
				XmlSerializer profDeserialize = new XmlSerializer(typeof(PwProfile));
				PwProfile profile = (PwProfile)profDeserialize.Deserialize(new StringReader(profileData));
				return profile;
			}
			catch (Exception) { }
			return null;
		}

		//Copy password profile to database
		public static bool CopyTo(this PwProfile profile, PwDatabase target)
		{
			PwProfile myProfile = profile.CloneDeep();
			if (myProfile.Name.EndsWith(Config.ProfileDBOnly))
				myProfile.Name = myProfile.Name.Substring(0, myProfile.Name.Length - Config.ProfileDBOnly.Length);
			PwProfile targetProfile = target.GetProfile(myProfile.Name);
			if ((targetProfile != null) && myProfile.IsEqual(targetProfile)) return false;
			string profileData = myProfile.Serialize();
			target.CustomData.Set(Config.ProfileConfig + myProfile.Name, profileData);
			return true;
		}

		//Check whether two PwProfile instances are equal. PwProfile.Equals doesn't work (CharSet)
		public static bool IsEqual(this PwProfile profile, PwProfile compareProfile)
		{
			if (compareProfile == null) return false;
			if (profile.Name != compareProfile.Name) return false;
			if (profile.GeneratorType != compareProfile.GeneratorType) return false;
			if (profile.CollectUserEntropy != compareProfile.CollectUserEntropy) return false;
			if (profile.Length != compareProfile.Length) return false;
			if (profile.CharSet.ToString() != compareProfile.CharSet.ToString()) return false;
			if (profile.CharSetRanges != compareProfile.CharSetRanges) return false;
			if (profile.CharSetAdditional != compareProfile.CharSetAdditional) return false;
			if (profile.Pattern != compareProfile.Pattern) return false;
			if (profile.PatternPermutePassword != compareProfile.PatternPermutePassword) return false;
			if (profile.ExcludeLookAlike != compareProfile.ExcludeLookAlike) return false;
			if (profile.NoRepeatingCharacters != compareProfile.NoRepeatingCharacters) return false;
			if (profile.ExcludeCharacters != compareProfile.ExcludeCharacters) return false;
			if (profile.CustomAlgorithmUuid != compareProfile.CustomAlgorithmUuid) return false;
			if (profile.CustomAlgorithmOptions != compareProfile.CustomAlgorithmOptions) return false;
			return true;
		}
	}
}
