using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NGuild;
using NGame2.NUI;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

public class ContentUISeasonFirstViewFx : MonoBehaviour
{
	private const string SEASON_FIRST_VIEW_ON_MULTI_BUTTON = "SeasonFirstViewOnMultiButton_";

	private const int VALUE_INVALID = -1;

	public BattleType BattleType;

	public string UIType = "";

	public int TutorialIndex;

	public GameObject SeasonFxRoot;

	public bool ActiveOnEnable = true;

	private GuildArenaSessionState _prevGuildArenaSessionState = GuildArenaSessionState.NotHeld;

	public void Open()
	{
		if (String_Extension_Method.IsNullOrEmpty(UIType))
		{
			UIType = base.gameObject.name;
		}
		BattleType battleType = BattleType;
		if (battleType == BattleType.Raid || battleType != BattleType.GuildArena)
		{
			UIHelper.EnableGameObject(SeasonFxRoot, IsSeasonFirst());
		}
		else
		{
			UIHelper.EnableGameObject(SeasonFxRoot, IsGuildArenaSessionFirst());
		}
	}

	public void Close()
	{
		UIHelper.EnableGameObject(SeasonFxRoot, enable: false);
	}

	public void SaveToLocal()
	{
		if (!CheckTutotrial())
		{
			return;
		}
		LocalAccountInt localAccountInt = new LocalAccountInt(-1, "SeasonFirstViewOnMultiButton_" + UIType);
		BattleType battleType = BattleType;
		if (battleType == BattleType.Raid || battleType != BattleType.GuildArena)
		{
			int curChallengeRaidIndex = GetCurChallengeRaidIndex();
			localAccountInt.Value = curChallengeRaidIndex;
			return;
		}
		GuildArenaSessionInfo guildArenaSessionInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildArenaSessionInfo;
		if (guildArenaSessionInfo != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.IsBattleTime())
		{
			int sessionIndex = guildArenaSessionInfo.SessionIndex;
			localAccountInt.Value = sessionIndex;
		}
	}

	private void OnEnable()
	{
		if (ActiveOnEnable)
		{
			Open();
		}
	}

	private void OnDisable()
	{
		UIHelper.EnableGameObject(SeasonFxRoot, enable: false);
	}

	private bool IsSeasonFirst()
	{
		if (!CheckTutotrial())
		{
			return false;
		}
		int curChallengeRaidIndex = GetCurChallengeRaidIndex();
		return IsChanged(curChallengeRaidIndex);
	}

	private bool IsGuildArenaSessionFirst()
	{
		GuildArenaSessionInfo guildArenaSessionInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildArenaSessionInfo;
		if (guildArenaSessionInfo == null)
		{
			return false;
		}
		if (!GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.IsBattleTime())
		{
			return false;
		}
		int sessionIndex = guildArenaSessionInfo.SessionIndex;
		return IsChanged(sessionIndex);
	}

	private bool CheckTutotrial()
	{
		if (TutorialIndex <= 0)
		{
			return true;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.TutorialManager?.IsComplete(TutorialIndex) ?? false;
	}

	private int GetCurChallengeRaidIndex()
	{
		List<DropBonusEvent> effectiveEvents = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvents(EventBonusType.ChallengeRaidSeason);
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(effectiveEvents, 0))
		{
			return -1;
		}
		DropBonusEvent dropBonusEvent = effectiveEvents[0];
		if (dropBonusEvent == null)
		{
			return -1;
		}
		return int.Parse(dropBonusEvent.Condition);
	}

	private int GetCurGuildArenaSession()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildArenaSessionInfo?.SessionIndex ?? (-1);
	}

	private bool IsChanged(int index)
	{
		if (new LocalAccountInt(-1, "SeasonFirstViewOnMultiButton_" + UIType).Value == index)
		{
			return false;
		}
		return true;
	}
}
