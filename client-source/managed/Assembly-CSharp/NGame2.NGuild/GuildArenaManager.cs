using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.GetGuildArenaSessionInfo;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NGuild;

public class GuildArenaManager
{
	private int[] _battledHeroIndices;

	private int _seasonIndex { get; set; }

	private GuildArenaSessionInfo _sessionInfo { get; set; }

	private bool _guildArenaApplied { get; set; }

	private int _autoApplyGuildArena { get; set; }

	private GuildArenaRankerInfo[] _matchedGuildRankerInfos { get; set; }

	private GuildMemberServerInfo _MVPMember { get; set; }

	public bool IsMatched
	{
		get
		{
			return _isMatched;
		}
		set
		{
			_isMatched = value;
		}
	}

	private bool _isMatched { get; set; }

	private int _winResult { get; set; }

	private int _addSeasonScore { get; set; }

	public GuildArenaSeasonType SeasonType
	{
		get
		{
			return _seasonType;
		}
		set
		{
			_seasonType = value;
		}
	}

	private GuildArenaSeasonType _seasonType { get; set; }

	private GuildArenaDeckInfo[] _savedDeckInfos { get; set; }

	public bool AlreadyRequest { get; set; }

	public void RequestGuildArenaSessionInfo(Action onComplete, bool enableErrorPopup = true)
	{
		NShared.GetGuildArenaSessionInfo.Request request = new NShared.GetGuildArenaSessionInfo.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildArenaSessionInfo.Response response)
		{
			if (response == null)
			{
				if (enableErrorPopup)
				{
					MessagePopup.OpenError(BaseResultType.Fail);
				}
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				if (enableErrorPopup)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
			}
			else if (response.Result != ResultType.Success)
			{
				if (enableErrorPopup)
				{
					MessagePopup.OpenError(response.Result);
				}
			}
			else
			{
				ApplyGuildArenaSessionInfo(response);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
				if (onComplete != null)
				{
					Vespa_System_Action_Extension_Method.fire(onComplete);
				}
			}
		});
	}

	public void ApplyGuildArenaSessionInfo(NShared.GetGuildArenaSessionInfo.Response response)
	{
		if (response == null)
		{
			Debug.LogError("GuildArenaSessionInfo is null");
		}
		_seasonIndex = response.SeasonIndex;
		_sessionInfo = response.SessionInfo;
		_guildArenaApplied = response.GuildArenaApplied;
		_autoApplyGuildArena = response.AutoApply;
		_matchedGuildRankerInfos = response.Rankers;
		_MVPMember = response.MVPMember;
		_winResult = response.WinResult;
		_addSeasonScore = response.AddSeasonScore;
		SeasonType = response.SeasonType;
		IsMatched = response.IsMatched;
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildArenaSessionInfo = response.SessionInfo;
	}

	public void ApplyBattledHeroIndices(int[] battledHeroIndices)
	{
		_battledHeroIndices = battledHeroIndices;
	}

	public int[] GetBattledHeroIndices()
	{
		return _battledHeroIndices;
	}

	public int GetGuildArenaSeasonIndex()
	{
		return _seasonIndex;
	}

	public int GetAddSeasonScore()
	{
		return _addSeasonScore;
	}

	public int GetWinResult()
	{
		return _winResult;
	}

	public GuildMemberServerInfo GetMVPMember()
	{
		return _MVPMember;
	}

	public GuildArenaSessionInfo GetGuildArenaSessionInfo()
	{
		return _sessionInfo;
	}

	public GuildArenaRankerInfo[] GetMatchedGuildRankerInfos()
	{
		return _matchedGuildRankerInfos;
	}

	public GuildArenaRankerInfo GetMyGuildInfo()
	{
		if (_matchedGuildRankerInfos != null && _matchedGuildRankerInfos.Length != 0)
		{
			return _matchedGuildRankerInfos[0];
		}
		return null;
	}

	public GuildArenaRankerInfo GetEnemyGuildInfo()
	{
		if (_matchedGuildRankerInfos != null && _matchedGuildRankerInfos.Length > 1)
		{
			return _matchedGuildRankerInfos[1];
		}
		return null;
	}

	public bool IsGuildArenaApplied()
	{
		return _guildArenaApplied;
	}

	public bool IsGuildArenaAutoApplied()
	{
		if (_autoApplyGuildArena != 1)
		{
			return false;
		}
		return true;
	}

	public void SetGuildArenaApplied()
	{
		_guildArenaApplied = true;
	}

	public void SetGuildArenaAutoApplied(int value)
	{
		_autoApplyGuildArena = value;
	}

	public GuildArenaDeckInfo[] GetSavedDeckInfos()
	{
		return _savedDeckInfos;
	}

	public void SetSavedDeckInfos(GuildArenaDeckInfo[] deckInfos)
	{
		Array.Sort(deckInfos, (GuildArenaDeckInfo deck1, GuildArenaDeckInfo deck2) => deck1.DeckIndex.CompareTo(deck2.DeckIndex));
		_savedDeckInfos = deckInfos;
	}

	public bool IsBattleTime()
	{
		GuildArenaSessionInfo guildArenaSessionInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildArenaSessionInfo;
		if (guildArenaSessionInfo == null)
		{
			return false;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(guildArenaSessionInfo.BattleBeginTime, out var o_dateTime) && Vespa_String_Extension_Method2.TryParseNetworkTime2(guildArenaSessionInfo.BattleEndTime, out var o_dateTime2) && currentServerUTCTime >= o_dateTime && currentServerUTCTime < o_dateTime2)
		{
			return true;
		}
		return false;
	}

	public bool IsApplyTime()
	{
		GuildArenaSessionInfo guildArenaSessionInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildArenaSessionInfo;
		if (guildArenaSessionInfo == null)
		{
			return false;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(guildArenaSessionInfo.BeginTime, out var o_dateTime) && Vespa_String_Extension_Method2.TryParseNetworkTime2(guildArenaSessionInfo.ApplyEndTime, out var o_dateTime2) && currentServerUTCTime >= o_dateTime && currentServerUTCTime < o_dateTime2)
		{
			return true;
		}
		return false;
	}

	public bool IsApplyEndedTime()
	{
		GuildArenaSessionInfo guildArenaSessionInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildArenaSessionInfo;
		if (guildArenaSessionInfo == null)
		{
			return false;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(guildArenaSessionInfo.ApplyEndTime, out var o_dateTime) && Vespa_String_Extension_Method2.TryParseNetworkTime2(guildArenaSessionInfo.BattleBeginTime, out var o_dateTime2) && currentServerUTCTime >= o_dateTime && currentServerUTCTime < o_dateTime2)
		{
			return true;
		}
		return false;
	}

	public bool CheckEndTime()
	{
		GuildArenaSessionInfo guildArenaSessionInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildArenaSessionInfo;
		if (guildArenaSessionInfo == null)
		{
			return false;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(guildArenaSessionInfo.EndTime, out var o_dateTime) && currentServerUTCTime > o_dateTime)
		{
			return true;
		}
		return false;
	}

	public string GetGuildArenaSeasonDisplayName()
	{
		return GetGuildArenaSeasonDisplayName(GetGuildArenaSeasonIndex());
	}

	public string GetGuildArenaSeasonDisplayName(int seasonIndex)
	{
		string displayLocalKeyBySeasonIndex = GameRoot.Instance.GuildArenaSeasonDataContainer.GetDisplayLocalKeyBySeasonIndex(seasonIndex);
		if (displayLocalKeyBySeasonIndex != null)
		{
			return LocalizationManager.GetStringDirect(displayLocalKeyBySeasonIndex);
		}
		return GameRoot.Instance.GuildArenaSeasonDataContainer.GetDisplayNameBySeasonIndex(seasonIndex);
	}
}
