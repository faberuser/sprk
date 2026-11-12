using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AppVersionInfo
{
	public static AppVersionInfo Parse(string s)
	{
		return Parse(s, new AppVersionInfo());
	}

	public static AppVersionInfo Parse(string s, AppVersionInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AppVersionInfo Parse(IDictionary d)
	{
		return Parse(d, new AppVersionInfo());
	}

	public static AppVersionInfo Parse(IDictionary d, AppVersionInfo o)
	{
		o.Platform = BaseJsonMarshaler.ParseValueString(d, "Platform");
		o.MinVersion = BaseJsonMarshaler.ParseValueString(d, "MinVersion");
		o.HotfixVersion = BaseJsonMarshaler.ParseValueString(d, "HotfixVersion");
		o.PatchVersionUrl = BaseJsonMarshaler.ParseValueString(d, "PatchVersionUrl");
		o.CdnProxyUrl = BaseJsonMarshaler.ParseValueString(d, "CdnProxyUrl");
		o.CrashReport = BaseJsonMarshaler.ParseValueString(d, "CrashReport");
		o.OffBackgroundDownload = BaseJsonMarshaler.ParseValueString(d, "OffBackgroundDownload");
		o.DownloadFileWriteSleepTimeMS = BaseJsonMarshaler.ParseValueString(d, "DownloadFileWriteSleepTimeMS");
		o.IsETC1TextureCdn = BaseJsonMarshaler.ParseValueString(d, "IsETC1TextureCdn");
		o.LegacyLoadingBundlePack = BaseJsonMarshaler.ParseValueString(d, "LegacyLoadingBundlePack");
		o.LegacyLoadingBundlePack_x86 = BaseJsonMarshaler.ParseValueString(d, "LegacyLoadingBundlePack_x86");
		return o;
	}

	public static string EncodeJsonString(AppVersionInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AppVersionInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Platform != null)
		{
			dictionary.Add("Platform", o.Platform);
		}
		if (o.MinVersion != null)
		{
			dictionary.Add("MinVersion", o.MinVersion);
		}
		if (o.HotfixVersion != null)
		{
			dictionary.Add("HotfixVersion", o.HotfixVersion);
		}
		if (o.PatchVersionUrl != null)
		{
			dictionary.Add("PatchVersionUrl", o.PatchVersionUrl);
		}
		if (o.CdnProxyUrl != null)
		{
			dictionary.Add("CdnProxyUrl", o.CdnProxyUrl);
		}
		if (o.CrashReport != null)
		{
			dictionary.Add("CrashReport", o.CrashReport);
		}
		if (o.OffBackgroundDownload != null)
		{
			dictionary.Add("OffBackgroundDownload", o.OffBackgroundDownload);
		}
		if (o.DownloadFileWriteSleepTimeMS != null)
		{
			dictionary.Add("DownloadFileWriteSleepTimeMS", o.DownloadFileWriteSleepTimeMS);
		}
		if (o.IsETC1TextureCdn != null)
		{
			dictionary.Add("IsETC1TextureCdn", o.IsETC1TextureCdn);
		}
		if (o.LegacyLoadingBundlePack != null)
		{
			dictionary.Add("LegacyLoadingBundlePack", o.LegacyLoadingBundlePack);
		}
		if (o.LegacyLoadingBundlePack_x86 != null)
		{
			dictionary.Add("LegacyLoadingBundlePack_x86", o.LegacyLoadingBundlePack_x86);
		}
		return dictionary;
	}
}
