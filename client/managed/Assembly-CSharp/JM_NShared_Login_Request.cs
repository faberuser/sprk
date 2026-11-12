using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.Login;

public static class JM_NShared_Login_Request
{
	public static NShared.Login.Request Parse(string s)
	{
		return Parse(s, new NShared.Login.Request());
	}

	public static NShared.Login.Request Parse(string s, NShared.Login.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.Login.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.Login.Request());
	}

	public static NShared.Login.Request Parse(IDictionary d, NShared.Login.Request o)
	{
		o.LoginMethod = BaseJsonMarshaler.ParseEnum<LoginMethodType>(d, "LoginMethod");
		o.LoginId = BaseJsonMarshaler.ParseValueString(d, "LoginId");
		o.LegacyLoginId = BaseJsonMarshaler.ParseValueString(d, "LegacyLoginId");
		o.SelectedAccountId = BaseJsonMarshaler.ParseValueLong(d, "SelectedAccountId");
		o.EncryptSalt = BaseJsonMarshaler.ParseValueLong(d, "EncryptSalt");
		o.DeviceId = BaseJsonMarshaler.ParseValueString(d, "DeviceId");
		o.Version = BaseJsonMarshaler.ParseValueString(d, "Version");
		o.BattleVersion = BaseJsonMarshaler.ParseValueString(d, "BattleVersion");
		o.UTCOffsetHour = BaseJsonMarshaler.ParseValueInt(d, "UTCOffsetHour");
		o.ForcedHost = BaseJsonMarshaler.ParseValueBool(d, "ForcedHost");
		o.DeviceOs = BaseJsonMarshaler.ParseValueString(d, "DeviceOs");
		o.DevicePlatform = BaseJsonMarshaler.ParseValueString(d, "DevicePlatform");
		o.DeviceModel = BaseJsonMarshaler.ParseValueString(d, "DeviceModel");
		o.DeviceMemorySize = BaseJsonMarshaler.ParseValueInt(d, "DeviceMemorySize");
		o.DeviceToken = BaseJsonMarshaler.ParseValueString(d, "DeviceToken");
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.PatchNoteIndex = BaseJsonMarshaler.ParseValueInt(d, "PatchNoteIndex");
		o.EventPush = BaseJsonMarshaler.ParseValueBool(d, "EventPush");
		o.GuildRaidPush = BaseJsonMarshaler.ParseValueBool(d, "GuildRaidPush");
		o.GuildSuppressPush = BaseJsonMarshaler.ParseValueBool(d, "GuildSuppressPush");
		o.NightPush = BaseJsonMarshaler.ParseValueBool(d, "NightPush");
		o.GraphicsAPI = BaseJsonMarshaler.ParseValueString(d, "GraphicsAPI");
		o.SupportTextureFormat = BaseJsonMarshaler.ParseValueString(d, "SupportTextureFormat");
		o.Adid = BaseJsonMarshaler.ParseValueString(d, "Adid");
		o.GoogleAdid = BaseJsonMarshaler.ParseValueString(d, "GoogleAdid");
		o.Idfa = BaseJsonMarshaler.ParseValueString(d, "Idfa");
		o.Idfv = BaseJsonMarshaler.ParseValueString(d, "Idfv");
		o.SigningKey = BaseJsonMarshaler.ParseValueString(d, "SigningKey");
		o.DeviceBuildFingerprint = BaseJsonMarshaler.ParseValueString(d, "DeviceBuildFingerprint");
		o.DeviceBuildModel = BaseJsonMarshaler.ParseValueString(d, "DeviceBuildModel");
		o.DeviceBuildBrand = BaseJsonMarshaler.ParseValueString(d, "DeviceBuildBrand");
		o.DeviceBuildDevice = BaseJsonMarshaler.ParseValueString(d, "DeviceBuildDevice");
		o.DeviceBuildProduct = BaseJsonMarshaler.ParseValueString(d, "DeviceBuildProduct");
		o.DeviceBuildType = BaseJsonMarshaler.ParseValueString(d, "DeviceBuildType");
		o.DeviceBuildSerial = BaseJsonMarshaler.ParseValueString(d, "DeviceBuildSerial");
		o.DeviceBuildBoard = BaseJsonMarshaler.ParseValueString(d, "DeviceBuildBoard");
		o.CpuArch = BaseJsonMarshaler.ParseValueString(d, "CpuArch");
		o.IsDeviceRooted = BaseJsonMarshaler.ParseValueBool(d, "IsDeviceRooted");
		o.IsModInstalled = BaseJsonMarshaler.ParseValueBool(d, "IsModInstalled");
		o.LoginVerificationInfo = BaseJsonMarshaler.ParseValueString(d, "LoginVerificationInfo");
		o.AppPlayerBuild = BaseJsonMarshaler.ParseValueInt(d, "AppPlayerBuild");
		o.CancelSignOut = BaseJsonMarshaler.ParseValueBool(d, "CancelSignOut");
		o.ForceSignOut = BaseJsonMarshaler.ParseValueBool(d, "ForceSignOut");
		o.PackageName = BaseJsonMarshaler.ParseValueString(d, "PackageName");
		o.IsEmulator = BaseJsonMarshaler.ParseValueBool(d, "IsEmulator");
		o.EnableCharacterVoice = BaseJsonMarshaler.ParseValueBool(d, "EnableCharacterVoice");
		o.CharacterVoiceLanguage = BaseJsonMarshaler.ParseValueInt(d, "CharacterVoiceLanguage");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.Login.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.Login.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("LoginMethod", o.LoginMethod.ToString());
		if (o.LoginId != null)
		{
			dictionary.Add("LoginId", o.LoginId);
		}
		if (o.LegacyLoginId != null)
		{
			dictionary.Add("LegacyLoginId", o.LegacyLoginId);
		}
		dictionary.Add("SelectedAccountId", o.SelectedAccountId.ToString());
		dictionary.Add("EncryptSalt", o.EncryptSalt.ToString());
		if (o.DeviceId != null)
		{
			dictionary.Add("DeviceId", o.DeviceId);
		}
		if (o.Version != null)
		{
			dictionary.Add("Version", o.Version);
		}
		if (o.BattleVersion != null)
		{
			dictionary.Add("BattleVersion", o.BattleVersion);
		}
		dictionary.Add("UTCOffsetHour", o.UTCOffsetHour.ToString());
		dictionary.Add("ForcedHost", o.ForcedHost.ToString());
		if (o.DeviceOs != null)
		{
			dictionary.Add("DeviceOs", o.DeviceOs);
		}
		if (o.DevicePlatform != null)
		{
			dictionary.Add("DevicePlatform", o.DevicePlatform);
		}
		if (o.DeviceModel != null)
		{
			dictionary.Add("DeviceModel", o.DeviceModel);
		}
		dictionary.Add("DeviceMemorySize", o.DeviceMemorySize.ToString());
		if (o.DeviceToken != null)
		{
			dictionary.Add("DeviceToken", o.DeviceToken);
		}
		if (o.Language != null)
		{
			dictionary.Add("Language", o.Language);
		}
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		dictionary.Add("PatchNoteIndex", o.PatchNoteIndex.ToString());
		dictionary.Add("EventPush", o.EventPush.ToString());
		dictionary.Add("GuildRaidPush", o.GuildRaidPush.ToString());
		dictionary.Add("GuildSuppressPush", o.GuildSuppressPush.ToString());
		dictionary.Add("NightPush", o.NightPush.ToString());
		if (o.GraphicsAPI != null)
		{
			dictionary.Add("GraphicsAPI", o.GraphicsAPI);
		}
		if (o.SupportTextureFormat != null)
		{
			dictionary.Add("SupportTextureFormat", o.SupportTextureFormat);
		}
		if (o.Adid != null)
		{
			dictionary.Add("Adid", o.Adid);
		}
		if (o.GoogleAdid != null)
		{
			dictionary.Add("GoogleAdid", o.GoogleAdid);
		}
		if (o.Idfa != null)
		{
			dictionary.Add("Idfa", o.Idfa);
		}
		if (o.Idfv != null)
		{
			dictionary.Add("Idfv", o.Idfv);
		}
		if (o.SigningKey != null)
		{
			dictionary.Add("SigningKey", o.SigningKey);
		}
		if (o.DeviceBuildFingerprint != null)
		{
			dictionary.Add("DeviceBuildFingerprint", o.DeviceBuildFingerprint);
		}
		if (o.DeviceBuildModel != null)
		{
			dictionary.Add("DeviceBuildModel", o.DeviceBuildModel);
		}
		if (o.DeviceBuildBrand != null)
		{
			dictionary.Add("DeviceBuildBrand", o.DeviceBuildBrand);
		}
		if (o.DeviceBuildDevice != null)
		{
			dictionary.Add("DeviceBuildDevice", o.DeviceBuildDevice);
		}
		if (o.DeviceBuildProduct != null)
		{
			dictionary.Add("DeviceBuildProduct", o.DeviceBuildProduct);
		}
		if (o.DeviceBuildType != null)
		{
			dictionary.Add("DeviceBuildType", o.DeviceBuildType);
		}
		if (o.DeviceBuildSerial != null)
		{
			dictionary.Add("DeviceBuildSerial", o.DeviceBuildSerial);
		}
		if (o.DeviceBuildBoard != null)
		{
			dictionary.Add("DeviceBuildBoard", o.DeviceBuildBoard);
		}
		if (o.CpuArch != null)
		{
			dictionary.Add("CpuArch", o.CpuArch);
		}
		dictionary.Add("IsDeviceRooted", o.IsDeviceRooted.ToString());
		dictionary.Add("IsModInstalled", o.IsModInstalled.ToString());
		if (o.LoginVerificationInfo != null)
		{
			dictionary.Add("LoginVerificationInfo", o.LoginVerificationInfo);
		}
		dictionary.Add("AppPlayerBuild", o.AppPlayerBuild.ToString());
		dictionary.Add("CancelSignOut", o.CancelSignOut.ToString());
		dictionary.Add("ForceSignOut", o.ForceSignOut.ToString());
		if (o.PackageName != null)
		{
			dictionary.Add("PackageName", o.PackageName);
		}
		dictionary.Add("IsEmulator", o.IsEmulator.ToString());
		dictionary.Add("EnableCharacterVoice", o.EnableCharacterVoice.ToString());
		dictionary.Add("CharacterVoiceLanguage", o.CharacterVoiceLanguage.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
