using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NConditionChecker;
using NShared;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil;

public class ConditionChecker : SingletonAuto<ConditionChecker>
{
	public abstract class ConditionBase
	{
		public abstract bool Check(string[] values);
	}

	private LocalAccountInt _FailDungeonCnt = new LocalAccountInt(0, typeof(FailDungeon).FullName + "_FailDungeonCnt");

	private Dictionary<string, ConditionBase> _dic = new Dictionary<string, ConditionBase>();

	public int FailDungeonCount
	{
		get
		{
			return _FailDungeonCnt.Value;
		}
		set
		{
			_FailDungeonCnt.Value = value;
		}
	}

	public ConditionChecker()
	{
		AddCondition<AnyHeroLevel>();
		AddCondition<CompleteDungeon>();
		AddCondition<CompleteTutorial>();
		AddCondition<CompleteScenario>();
		AddCondition<LastDungeonResult>();
		AddCondition<PlayWave>();
		AddCondition<AnyHero>();
		AddCondition<AnyHeroStar>();
		AddCondition<AnyHeroTranscended>();
		AddCondition<BillingUser>();
		AddCondition<BuyItem>();
		AddCondition<FailDungeon>();
		AddCondition<LoginDailyCount>();
		AddCondition<TotalHeroCount>();
		AddCondition<CreateAccount>();
		AddCondition<ClearDungeon>();
		AddCondition<UseItem>();
		AddCondition<UseSelectionItem>();
		AddCondition<AutoNextBattle>();
		AddCondition<AutoRepeatBattle>();
		AddCondition<OpenNode>();
		AddCondition<Count>();
		AddCondition<ClearLabyrinth>();
		AddCondition<ClearHighLevelDungeon>();
		AddCondition<ClearChallengeTower>();
		AddCondition<ClearSuppress>();
		AddCondition<ClearChallengeRaid>();
		AddCondition<ClearWorldBoss>();
		AddCondition<ClearMaze>();
		AddCondition<ClearOrdealTower>();
		AddCondition<ClearTreasureHouse>();
		AddCondition<JoinMatch>();
		AddCondition<JoinMatchBanPick>();
		AddCondition<CheckMultiplay>();
		AddCondition<CheckClassBuff>();
		AddCondition<CheckTeamLevel>();
		AddCondition<NewbieBuyItemBonusCount>();
		AddCondition<GuildMemberCount>();
		AddCondition<NodeVisibleChecker>();
		AddCondition<JoinGuild>();
		AddCondition<OpenRaidHard>();
		AddCondition<CheckEventBuffCondition>();
		AddCondition<IsOpenGatewayToTrial>();
		AddCondition<CheckHeroCount>();
		AddCondition<IsEnoughShakmehPoint>();
		AddCondition<EventShop>();
		AddCondition<IsAbleEnterChallengeRaid>();
		AddCondition<AnyCompleteDungeon>();
		AddCondition<CompleteOrdealArenaFloor>();
		AddCondition<NewbieJumping>();
		AddCondition<CheckLastPlayBattleType>();
		AddCondition<PunishmentRaidClearState>();
		AddCondition<PunishmentCompareLevel>();
		AddCondition<PunishmentClearDungeonCount>();
		AddCondition<OwnedHero>();
		AddCondition<EventWorldTreeStepChecker>();
		AddCondition<IsBuyableProduct>();
		AddCondition<PunishmentRaidAlways>();
	}

	private void AddCondition<T>() where T : ConditionBase, new()
	{
		_dic.Add(typeof(T).Name, new T());
	}

	public bool Check(string[] values)
	{
		if (values != null && values.Length == 5 && values[0] == "CompleteTutorialOrSkippedDungeon")
		{
			return RestoredPortal.CheckNativeStoryCondition(values);
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(values))
		{
			return true;
		}
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, values[0], null)?.Check(values) ?? false;
	}

	public bool CheckNodeOpen(CampaignNodeData nodeData)
	{
		if (nodeData == null)
		{
			return false;
		}
		if (nodeData.Opened)
		{
			return true;
		}
		if (nodeData.ReqDungeonIndex != 0 && !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(nodeData.ChapterIndex, nodeData.ReqDungeonIndex))
		{
			return false;
		}
		if (nodeData.ReqTutorialIndex > 0 && !GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(nodeData.ReqTutorialIndex))
		{
			return false;
		}
		if (!Check(nodeData.ReqCondition))
		{
			return false;
		}
		return true;
	}

	public bool IsNodeAffectedByTutorial(CampaignNodeData nodeData, int tutorialIndex)
	{
		if (nodeData == null)
		{
			return false;
		}
		if (nodeData.ReqTutorialIndex == tutorialIndex)
		{
			return true;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(nodeData.ReqCondition) && Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, nodeData.ReqCondition[0], null) is CompleteTutorial completeTutorial && completeTutorial.ContainsTutorial(nodeData.ReqCondition, tutorialIndex))
		{
			return true;
		}
		return false;
	}

	public bool CheckMatchCondition<T>(CampaignNodeData nodeData) where T : ConditionBase
	{
		return CheckMatchCondition<T>(nodeData.ReqCondition);
	}

	public bool CheckMatchCondition<T>(string[] reqCondition) where T : ConditionBase
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(reqCondition))
		{
			return false;
		}
		if (Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, reqCondition[0], null) as T == null)
		{
			return false;
		}
		return true;
	}

	public bool CheckCondition<T>(CampaignNodeData nodeData) where T : ConditionBase
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(nodeData.ReqCondition))
		{
			return false;
		}
		if (!(Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, nodeData.ReqCondition[0], null) is T val))
		{
			return false;
		}
		return val.Check(nodeData.ReqCondition);
	}

	public bool CheckGetValues(string[] values, ref int count, ref int minutes)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 5)
		{
			return false;
		}
		if (!int.TryParse(values[1], out count) || String_Extension_Method.IsNullOrEmpty(values[2]) || !int.TryParse(values[3], out minutes) || String_Extension_Method.IsNullOrEmpty(values[4]))
		{
			return false;
		}
		return true;
	}

	public bool CheckGetCheckType(string checkTypeStr, ref CheckType checkType)
	{
		try
		{
			checkType = (CheckType)Enum.Parse(typeof(CheckType), checkTypeStr);
			return true;
		}
		catch
		{
			checkType = CheckType.None;
			return false;
		}
	}

	public bool CheckCount(StaminaType staminaType, int count, CheckType countCheckType)
	{
		if (count == 0)
		{
			return true;
		}
		int num = 0;
		long currentValue = num;
		int num2 = 0;
		long maxValue = num2;
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(staminaType, out currentValue, out maxValue))
		{
			return CheckCheckType(countCheckType, count, checked(maxValue - currentValue));
		}
		return false;
	}

	public bool CheckCheckType(CheckType checkType, int tabelValue, long userValue)
	{
		switch (checkType)
		{
		case CheckType.Up:
			if (tabelValue < userValue)
			{
				return true;
			}
			break;
		case CheckType.Down:
			if (tabelValue > userValue)
			{
				return true;
			}
			break;
		case CheckType.Equal:
			if (tabelValue == userValue)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public string GetTutorialCompletedTime(string condition)
	{
		int tutorialIndexByCondition = GameRoot.Instance.PlayGuideDataContainer.GetTutorialIndexByCondition(condition);
		TutorialInfo tutorialInfo = GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.Get(tutorialIndexByCondition);
		if (tutorialInfo != null)
		{
			return tutorialInfo.CompletedTime;
		}
		return "";
	}

	public int GetTotalMinutes(string time)
	{
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(time, out var o_dateTime))
		{
			return (int)(Common.GetUTCNow() - o_dateTime).TotalMinutes;
		}
		return 0;
	}
}
