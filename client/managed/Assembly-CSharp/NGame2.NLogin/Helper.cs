using System;
using System.Collections.Generic;
using System.Linq;
using NGameAsset;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NLogin;

public static class Helper
{
	private static readonly LocalStorageString _lastDeviceUid = new LocalStorageString("Account_LastDeviceUid");

	public static string GetDeviceUid()
	{
		string text = null;
		string value = _lastDeviceUid.Value;
		if (!string.IsNullOrEmpty(value))
		{
			text = value;
		}
		else
		{
			text = Guid.NewGuid().ToString("N");
			_lastDeviceUid.Value = text;
		}
		return text;
	}

	public static bool IsNewDevice()
	{
		return String_Extension_Method.IsNullOrEmpty(_lastDeviceUid.Value);
	}

	public static AppServerInfo GetServerSafely(VersionedHost versionedHost, string serverName)
	{
		if (!String_Extension_Method.IsNullOrEmpty(serverName))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(versionedHost.Server); i++)
			{
				AppServerInfo appServerInfo = versionedHost.Server[i];
				if (appServerInfo != null && string.Equals(appServerInfo.Name, serverName))
				{
					return appServerInfo;
				}
			}
		}
		return GetServerOfMyCountry(versionedHost);
	}

	public static AppServerInfo GetServerOfMyCountry(VersionedHost versionedHost)
	{
		string countryCode = SingletonUpdater<LoginManager>.instance.CountryCode;
		AppServerInfo appServerInfo = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(versionedHost.Server); i++)
		{
			AppServerInfo appServerInfo2 = versionedHost.Server[i];
			if (appServerInfo2 != null && (__Extension_Method_ICollection__.COUNT(appServerInfo2.Country) == 0 || appServerInfo2.Country.Contains(countryCode)))
			{
				appServerInfo = appServerInfo2;
			}
		}
		if (appServerInfo == null)
		{
			appServerInfo = ((__Extension_Method_ICollection__.COUNT(versionedHost.Server) > 0) ? versionedHost.Server[0] : null);
		}
		return appServerInfo;
	}

	public static bool FindHostInfo(string text, ClientConfig clientConfig, out VersionedHost foundHostInfo, out AppVersionInfo foundAppVersionInfo)
	{
		foundHostInfo = null;
		foundAppVersionInfo = null;
		Action<VersionedHost> i_action = delegate(VersionedHost hostInfo)
		{
			string value = SingletonUpdater<LoginManager>.instance.ForcedServerIp.Value;
			string value2 = SingletonUpdater<LoginManager>.instance.ForceLoginServer.Value;
			if (!String_Extension_Method.IsNullOrEmpty(value))
			{
				List<AppServerInfo> list = new List<AppServerInfo>();
				AppServerInfo item = new AppServerInfo
				{
					Name = value,
					Host = value,
					LoginServer = value2
				};
				list.Add(item);
				hostInfo.Server = list.ToArray();
			}
		};
		try
		{
			long normalizedVersion = ClientConfig.GetNormalizedVersion(clientConfig.Version);
			VersionedHost[] array = BaseJsonMarshaler.ParseArray(text, JsonMarshaler.Parse<VersionedHost>);
			string forcedServerName = clientConfig.ForcedServerName;
			if (!string.IsNullOrEmpty(forcedServerName))
			{
				for (int num = 0; num < __Extension_Method_ICollection__.COUNT(array); num++)
				{
					VersionedHost versionedHost = array[num];
					if (versionedHost != null && __Extension_Method_ICollection__.COUNT(versionedHost.Server) != 0 && versionedHost.Server.Any((AppServerInfo e) => string.Equals(e.Name, forcedServerName, StringComparison.InvariantCultureIgnoreCase)))
					{
						foundHostInfo = versionedHost;
						foundAppVersionInfo = versionedHost.AppVersion.FirstOrDefault((AppVersionInfo e) => string.Equals(e.Platform, DefinePlatform.PlatformName, StringComparison.InvariantCultureIgnoreCase));
						if (foundAppVersionInfo != null)
						{
							Vespa_System_Action_Extension_Method.fire(i_action, foundHostInfo);
							return true;
						}
						VespaDebug.LogError("Forced host exist but no app : server=" + forcedServerName + " platform=" + DefinePlatform.PlatformName);
					}
				}
			}
			bool flag = false;
			for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(array); num2++)
			{
				VersionedHost versionedHost2 = array[num2];
				if (versionedHost2 == null || (flag && (string.IsNullOrEmpty(versionedHost2.Type) || versionedHost2.Type != "Hotfix")))
				{
					continue;
				}
				for (int num3 = 0; num3 < __Extension_Method_ICollection__.COUNT(versionedHost2.AppVersion); num3++)
				{
					AppVersionInfo appVersionInfo = versionedHost2.AppVersion[num3];
					if (appVersionInfo != null && string.Equals(appVersionInfo.Platform, DefinePlatform.PlatformName, StringComparison.InvariantCultureIgnoreCase) && CheckCdnTextureFormat(appVersionInfo.IsETC1TextureCdn))
					{
						long num4 = (string.IsNullOrEmpty(appVersionInfo.HotfixVersion) ? 0 : ClientConfig.GetNormalizedVersion(appVersionInfo.HotfixVersion));
						if (num4 != 0L && normalizedVersion == num4)
						{
							flag = true;
							break;
						}
						long num5 = (string.IsNullOrEmpty(appVersionInfo.MinVersion) ? 0 : ClientConfig.GetNormalizedVersion(appVersionInfo.MinVersion));
						if (normalizedVersion >= num5)
						{
							foundHostInfo = versionedHost2;
							foundAppVersionInfo = appVersionInfo;
							Vespa_System_Action_Extension_Method.fire(i_action, foundHostInfo);
							return true;
						}
					}
				}
			}
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
		}
		return false;
	}

	private static bool CheckCdnTextureFormat(string isETC1TextureCdn)
	{
		if (String_Extension_Method.IsNullOrEmpty(isETC1TextureCdn))
		{
			return true;
		}
		bool num = Convert.ToBoolean(isETC1TextureCdn);
		bool flag = !SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_4x4);
		return num == flag;
	}
}
