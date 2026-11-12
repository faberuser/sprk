using System.Collections.Generic;
using NGame2.NConfig;
using NShared;
using NVespa;

namespace NGame2.NLogin;

public static class LoginData
{
	public class LastServerNameData
	{
		private static Dictionary<string, LocalStorageString> _lastServerDic = new Dictionary<string, LocalStorageString>();

		private static string GetClientConfigName()
		{
			if (ClientConfig == null)
			{
				return string.Empty;
			}
			if (String_Extension_Method.IsNullOrEmpty(ClientConfig.Name))
			{
				return string.Empty;
			}
			return ClientConfig.Name;
		}

		public static LocalStorageString GetServerName()
		{
			string text = $"LoginData_{GetClientConfigName()}_LastServerName";
			LocalStorageString localStorageString = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_lastServerDic, text, null);
			if (localStorageString == null)
			{
				localStorageString = new LocalStorageString(string.Empty, text);
				_lastServerDic.Add(text, localStorageString);
			}
			return localStorageString;
		}
	}

	private static bool _isNewAccount = false;

	public static WebEventInfo _webEvent;

	public static MissionCategoryInfo[] _missionCategoryInfo;

	private static LocalStorageEnum<LoginMethodType> _lastLoginMethod = new LocalStorageEnum<LoginMethodType>("Account_LastLoginMethod");

	private static LocalStorageString _last_login_id = new LocalStorageString("Account_LastLoginId");

	private static LocalStorageString _last_login_legacy_id = new LocalStorageString("Account_LastLoginLegacyId");

	private static LocalStorageLong _last_login_account_id = new LocalStorageLong(0L, "Account_LastLoginAccountId");

	private static LocalStorageString _lastServerAddress = new LocalStorageString("Account_LastServerAddress");

	private static LocalStorageBool _lastUseHttps = new LocalStorageBool(i_default: false, "Account_LastUseHttps");

	private static LocalStorageString _lastCookieKeyword = new LocalStorageString("Account_LastCookieKeyword");

	private static LocalStorageInt _lastPatchNoteIndex = new LocalStorageInt(0, "Account_LastPatchNoteIndex");

	private static LocalStorageString _lastPatchNoteTitle = new LocalStorageString("Account_LastPatchNoteTitle");

	private static LocalStorageString _lastPatchNoteMessage = new LocalStorageString("Account_LastPatchNoteMessage");

	private static LocalStorageString _lastPatchNoteEndTime = new LocalStorageString("Account_LastPatchNoteEndTime");

	private static LocalStorageInt _lastEventCalendarVersion = new LocalStorageInt(0, "Account_EventCalendarVersion");

	private static LocalStorageString _lastAccessToken = new LocalStorageString("Account_LastAccessToken");

	private static LocalStorageString _lastRefreshToken = new LocalStorageString("Account_LastRefreshToken");

	public static bool IsNewAccount
	{
		get
		{
			return _isNewAccount;
		}
		set
		{
			_isNewAccount = value;
		}
	}

	public static ClientConfig ClientConfig => SingletonAuto<Config>.instance.ClientConfig;

	public static string LastServerName
	{
		get
		{
			return LastServerNameData.GetServerName().Value;
		}
		set
		{
			LastServerNameData.GetServerName().Value = value;
		}
	}

	public static LoginMethodType LastLoginMethod
	{
		get
		{
			return _lastLoginMethod.Value;
		}
		set
		{
			_lastLoginMethod.Value = value;
		}
	}

	public static LoginUserId LastLoginUserId
	{
		get
		{
			return new LoginUserId(_last_login_id.Value, _last_login_legacy_id.Value, _last_login_account_id.Value);
		}
		set
		{
			_last_login_id.Value = value.id;
			_last_login_legacy_id.Value = value.legacyId;
			_last_login_account_id.Value = value.accountId;
		}
	}

	public static string LastServerAddress
	{
		get
		{
			return _lastServerAddress.Value;
		}
		set
		{
			_lastServerAddress.Value = value;
		}
	}

	public static bool LastUseHttps
	{
		get
		{
			return _lastUseHttps.Value;
		}
		set
		{
			_lastUseHttps.Value = value;
		}
	}

	public static string LastCookieKeyword
	{
		get
		{
			return _lastCookieKeyword.Value;
		}
		set
		{
			_lastCookieKeyword.Value = value;
		}
	}

	public static int LastPatchNoteIndex
	{
		get
		{
			return _lastPatchNoteIndex.Value;
		}
		set
		{
			_lastPatchNoteIndex.Value = value;
		}
	}

	public static string LastPatchNoteTitle
	{
		get
		{
			return _lastPatchNoteTitle.Value;
		}
		set
		{
			_lastPatchNoteTitle.Value = value;
		}
	}

	public static string LastPatchNoteMessage
	{
		get
		{
			return _lastPatchNoteMessage.Value;
		}
		set
		{
			_lastPatchNoteMessage.Value = value;
		}
	}

	public static string LastPatchNoteEndTime
	{
		get
		{
			return _lastPatchNoteEndTime.Value;
		}
		set
		{
			_lastPatchNoteEndTime.Value = value;
		}
	}

	public static int EventCalendarVersion
	{
		get
		{
			return _lastEventCalendarVersion.Value;
		}
		set
		{
			_lastEventCalendarVersion.Value = value;
		}
	}

	public static string LastAccessToken
	{
		get
		{
			return _lastAccessToken.Value;
		}
		set
		{
			_lastAccessToken.Value = value;
		}
	}

	public static string LastRefreshToken
	{
		get
		{
			return _lastRefreshToken.Value;
		}
		set
		{
			_lastRefreshToken.Value = value;
			SprkAccounts.SyncRefreshToken();
		}
	}

	public static WebEventInfo WebEvent
	{
		get
		{
			return _webEvent;
		}
		set
		{
			_webEvent = value;
		}
	}

	public static MissionCategoryInfo[] MissionCategoryInfo
	{
		get
		{
			return _missionCategoryInfo;
		}
		set
		{
			_missionCategoryInfo = value;
		}
	}

	public static void ClearLastLoginData()
	{
		LastLoginMethod = LoginMethodType.None;
		LastLoginUserId = new LoginUserId(string.Empty, string.Empty, 0L);
		LastServerAddress = string.Empty;
		LastUseHttps = false;
		LastCookieKeyword = string.Empty;
	}

	public static void ClearLastPatchNote(bool resetToZero)
	{
		LastPatchNoteIndex = ((!resetToZero) ? (LastPatchNoteIndex - 1) : 0);
	}
}
