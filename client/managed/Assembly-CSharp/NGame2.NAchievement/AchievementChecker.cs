using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NAchievement;

public class AchievementChecker : SingletonAuto<AchievementChecker>, AchievementComponent.IDataProvider, HeroPortraitComponent.IDataProvider
{
	public static bool Cleared(int achievementIndex)
	{
		NGame2.NAccount.UserAchievementManager achievementManager = GlobalAutoBehaviour<AccountManager>.instance.AchievementManager;
		AchievementDataContainer achievementDataContainer = GameRoot.Instance.AchievementDataContainer;
		if (achievementDataContainer == null)
		{
			return false;
		}
		AchievementData achievementData = null;
		AchievementInfo achievementInfo = achievementManager.Get(achievementIndex);
		if (achievementInfo != null)
		{
			achievementData = achievementDataContainer.GetData(achievementInfo.AchievementIndex, achievementInfo.LastStep + 1);
			if (achievementData == null)
			{
				return false;
			}
		}
		else
		{
			achievementData = achievementDataContainer.GetData(achievementIndex, 1);
		}
		return Cleared(achievementInfo, achievementData);
	}

	public static bool Cleared(AchievementInfo achievementInfo, AchievementData achievementData)
	{
		if (achievementData == null)
		{
			return false;
		}
		if (achievementData.ReqType == AchievementReqType.LoginOrConnectedTime)
		{
			DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
			if (GameRoot.Instance.AchievementDataContainer.GetLoginOrConnectedAchievementIndex(currentServerLocalTime.Hour) != achievementData.Index)
			{
				return false;
			}
		}
		ulong num = (ulong)(achievementInfo?.Achievement ?? 0);
		ulong achievementRequirement = GetAchievementRequirement(achievementData);
		if (!achievementData.ReqValueDecrease)
		{
			return num >= achievementRequirement;
		}
		if (num == 0L)
		{
			return false;
		}
		return num <= achievementRequirement;
	}

	public static bool Cleared(AchievementInfo achievementInfo)
	{
		if (achievementInfo == null)
		{
			return false;
		}
		if (GameRoot.Instance.AchievementDataContainer == null)
		{
			return false;
		}
		AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(achievementInfo.AchievementIndex, achievementInfo.LastStep + 1);
		if (data == null)
		{
			return false;
		}
		return Cleared(achievementInfo, data);
	}

	public static bool Completed(int achievementIndex)
	{
		return Completed(GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(achievementIndex));
	}

	public static bool Completed(AchievementInfo achievementInfo)
	{
		if (achievementInfo == null)
		{
			return false;
		}
		if (GameRoot.Instance.AchievementDataContainer == null)
		{
			return false;
		}
		if (GameRoot.Instance.AchievementDataContainer.GetData(achievementInfo.AchievementIndex, achievementInfo.LastStep + 1) == null)
		{
			return true;
		}
		return false;
	}

	private static ulong GetAchievementRequirement(AchievementData achievementData)
	{
		if (achievementData.ReqSub2Type != AchievementReqSubType.None)
		{
			return (ulong)achievementData.ReqSub2Value;
		}
		if (achievementData.ReqSub1Type != AchievementReqSubType.None)
		{
			return (ulong)achievementData.ReqSub1Value;
		}
		return Vespa_String_Extension_Method.TOULONG(achievementData.ReqValue, 0uL);
	}

	public int GetClearedAchievementCount(AchievementType achievementType)
	{
		int num = 0;
		HashSet<int> achievementIndexSet = GameRoot.Instance.AchievementDataContainer.GetAchievementIndexSet(achievementType);
		if (achievementIndexSet == null)
		{
			return num;
		}
		HashSet<int>.Enumerator enumerator = achievementIndexSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (Cleared(enumerator.Current))
			{
				num++;
			}
		}
		return num;
	}

	public int GetClearedAchievementCount(AchievementType achievementType, AchievementCategoryType achievementCategory)
	{
		if (achievementCategory == AchievementCategoryType.None)
		{
			return GetClearedAchievementCount(achievementType);
		}
		int num = 0;
		HashSet<int> achievementIndexSet = GameRoot.Instance.AchievementDataContainer.GetAchievementIndexSet(achievementType);
		if (achievementIndexSet == null)
		{
			return num;
		}
		HashSet<int>.Enumerator enumerator = achievementIndexSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (Cleared(enumerator.Current) && GameRoot.Instance.AchievementDataContainer.GetData(enumerator.Current, 1).Category == achievementCategory)
			{
				num++;
			}
		}
		return num;
	}

	public int GetClearedAchievements(AchievementType[] achievementTypes)
	{
		int num = 0;
		for (int i = 0; i < achievementTypes.Length; i++)
		{
			num += GetClearedAchievementCount(achievementTypes[i]);
		}
		return num;
	}

	public int GetAllClearedAchievements(params AchievementType[] exceptions)
	{
		Array values = Enum.GetValues(typeof(AchievementType));
		int num = 0;
		for (int i = 0; i < values.Length; i++)
		{
			AchievementType achievementType = (AchievementType)values.GetValue(i);
			if (Array.FindIndex(exceptions, (AchievementType type) => achievementType == type) == -1)
			{
				num += GetClearedAchievementCount(achievementType);
			}
		}
		return num;
	}

	public int GetAllNotClearedAchievements(params AchievementType[] exceptions)
	{
		Array values = Enum.GetValues(typeof(AchievementType));
		int num = 0;
		for (int i = 0; i < values.Length; i++)
		{
			AchievementType achievementType = (AchievementType)values.GetValue(i);
			if (Array.FindIndex(exceptions, (AchievementType type) => achievementType == type) == -1)
			{
				num += GetNotClearedAchievementCount(achievementType);
			}
		}
		return num;
	}

	public int GetNotClearedAchievementCount(AchievementType achievementType)
	{
		int num = 0;
		HashSet<int> achievementIndexSet = GameRoot.Instance.AchievementDataContainer.GetAchievementIndexSet(achievementType);
		if (achievementIndexSet == null)
		{
			return num;
		}
		HashSet<int>.Enumerator enumerator = achievementIndexSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!Cleared(enumerator.Current) && !Completed(enumerator.Current))
			{
				num++;
			}
		}
		return num;
	}

	public int GetRemainDailyQuestCount()
	{
		HashSet<int> achievementIndexSet = GameRoot.Instance.AchievementDataContainer.GetAchievementIndexSet(AchievementType.Daily);
		if (achievementIndexSet == null)
		{
			return 0;
		}
		AchievementData achievementData = null;
		HashSet<int>.Enumerator enumerator = achievementIndexSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			achievementData = GameRoot.Instance.AchievementDataContainer.GetData(enumerator.Current, 1);
			if (achievementData != null && achievementData.ReqType == AchievementReqType.CompleteDailyQuest)
			{
				break;
			}
		}
		if (achievementData == null)
		{
			return 0;
		}
		ulong num = (ulong)(GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(achievementData.Index)?.Achievement ?? 0);
		ulong num2 = Vespa_String_Extension_Method.TOULONG(achievementData.ReqValue, 0uL);
		return (int)((num2 >= num) ? (num2 - num) : 0);
	}

	public static int GetLastStepOnAchievementInfo(int achievementIndex)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(achievementIndex)?.LastStep ?? 0;
	}

	private int GetCurAchievementStep(int achievementIndex)
	{
		AchievementInfo achievementInfo = GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(achievementIndex);
		if (achievementInfo == null)
		{
			return 1;
		}
		return achievementInfo.LastStep + 1;
	}

	private int GetLastStepOnAchievementData(int achievementIndex)
	{
		AchievementData achievementData = null;
		int num = 0;
		do
		{
			achievementData = GameRoot.Instance.AchievementDataContainer.GetData(achievementIndex, num + 1);
			if (achievementData != null)
			{
				num++;
			}
		}
		while (achievementData != null);
		return num;
	}

	public int GetRewardableHeroAchievementCount(int HeroIndex)
	{
		int num = 0;
		HashSet<int> heroAchievementIndexSet = GameRoot.Instance.AchievementDataContainer.GetHeroAchievementIndexSet(HeroIndex);
		if (heroAchievementIndexSet == null)
		{
			return num;
		}
		HashSet<int>.Enumerator enumerator = heroAchievementIndexSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (Cleared(enumerator.Current))
			{
				num++;
			}
		}
		return num;
	}

	public AchievementData GetAchievementData(int achievementIndex)
	{
		int curAchievementStep = GetCurAchievementStep(achievementIndex);
		return GameRoot.Instance.AchievementDataContainer.GetData(achievementIndex, curAchievementStep);
	}

	public void GetAchievementAndRequirement(int achievementIndex, out long curProgress, out long requirement, out long PrevRequirement)
	{
		curProgress = 0L;
		requirement = 1L;
		PrevRequirement = 0L;
		AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(achievementIndex, 1);
		if (data == null)
		{
			VespaDebug.LogError("AchievementData not found : AchievementIndex = " + achievementIndex);
			return;
		}
		if (data.DisplayProgress == DisplayProgressType.None)
		{
			if (data.OnlyDisplayClearFlag)
			{
				bool flag = Completed(achievementIndex) || Cleared(achievementIndex);
				curProgress = (flag ? 1 : 0);
				requirement = 1L;
				return;
			}
			curProgress = GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(achievementIndex)?.Achievement ?? 0;
			int curAchievementStep = GetCurAchievementStep(achievementIndex);
			AchievementData data2 = GameRoot.Instance.AchievementDataContainer.GetData(achievementIndex, curAchievementStep);
			if (data2 != null)
			{
				requirement = (long)GetAchievementRequirement(data2);
				AchievementData data3 = GameRoot.Instance.AchievementDataContainer.GetData(achievementIndex, curAchievementStep - 1);
				if (data3 != null)
				{
					PrevRequirement = (long)GetAchievementRequirement(data3);
				}
			}
			return;
		}
		if (data.DisplayProgress == DisplayProgressType.ClearNode)
		{
			int chapterIndex = Vespa_String_Extension_Method.TOINT(data.ReqValue, 0);
			GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetChapterClearDungeonAchievement(chapterIndex, out curProgress, out requirement);
			return;
		}
		if (data.DisplayProgress == DisplayProgressType.SkillMaster)
		{
			int heroIndex = Vespa_String_Extension_Method.TOINT(data.ReqValue, 0);
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
			if (heroInfo != null)
			{
				curProgress = heroInfo.GetSkillExtend((int)data.ReqSub1Value);
				requirement = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxSkillExtend");
			}
			return;
		}
		if (data.DisplayProgress == DisplayProgressType.AllSkillMaster)
		{
			int heroIndex2 = Vespa_String_Extension_Method.TOINT(data.ReqValue, 0);
			CachedHeroInfo heroInfo2 = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex2);
			if (heroInfo2 == null)
			{
				return;
			}
			curProgress = 0L;
			requirement = 4L;
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxSkillExtend");
			for (int i = 1; i <= requirement; i++)
			{
				if (heroInfo2.GetSkillExtend(i) == valueInt)
				{
					curProgress++;
				}
			}
			return;
		}
		if (data.DisplayProgress == DisplayProgressType.EquipCount)
		{
			curProgress = 0L;
			requirement = Vespa_String_Extension_Method.TOINT(data.ReqValue, 0);
			long Grade = data.ReqSub1Value;
			{
				foreach (CachedHeroInfo heroInfo4 in GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: false))
				{
					List<EquipItemInfo> equipItemInfoList = heroInfo4.GetEquipItemInfoList();
					if (equipItemInfoList.Count >= 4 && equipItemInfoList.Find((EquipItemInfo e) => GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(e.ItemIndex).Grade < Grade) == null)
					{
						curProgress++;
					}
					if (curProgress == requirement)
					{
						break;
					}
				}
				return;
			}
		}
		if (data.DisplayProgress == DisplayProgressType.HeroLevel)
		{
			int heroIndex3 = Vespa_String_Extension_Method.TOINT(data.ReqValue, 0);
			CachedHeroInfo heroInfo3 = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex3);
			if (heroInfo3 != null)
			{
				curProgress = heroInfo3.Level;
				requirement = data.ReqSub1Value;
			}
		}
		else
		{
			if (data.DisplayProgress != DisplayProgressType.TotalEquipGradeCount)
			{
				return;
			}
			curProgress = 0L;
			requirement = data.ReqSub1Value;
			int Grade2 = Vespa_String_Extension_Method.TOINT(data.ReqValue, 0);
			foreach (CachedHeroInfo heroInfo5 in GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: false))
			{
				if (heroInfo5.GetEquipItemInfoList().Find((EquipItemInfo e) => GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(e.ItemIndex).Grade == Grade2) != null)
				{
					curProgress++;
				}
				if (curProgress == requirement)
				{
					break;
				}
			}
		}
	}

	public int GetAchievedPercent(int AchievementIndex)
	{
		AchievementInfo achievementInfo = GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(AchievementIndex);
		AchievementData achievementData = null;
		if (achievementInfo != null)
		{
			achievementData = GameRoot.Instance.AchievementDataContainer.GetData(achievementInfo.AchievementIndex, achievementInfo.LastStep + 1);
			if (achievementData == null)
			{
				return 0;
			}
		}
		else
		{
			achievementData = GameRoot.Instance.AchievementDataContainer.GetData(AchievementIndex, 1);
		}
		if (achievementData == null)
		{
			return 0;
		}
		if (achievementData.ReqType == AchievementReqType.LoginOrConnectedTime)
		{
			DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
			if (GameRoot.Instance.AchievementDataContainer.GetLoginOrConnectedAchievementIndex(currentServerLocalTime.Hour) != AchievementIndex)
			{
				return 0;
			}
		}
		GetAchievementAndRequirement(AchievementIndex, out var curProgress, out var requirement, out var _);
		if (requirement == 0L)
		{
			requirement = 1L;
		}
		return System.Math.Min((int)((float)curProgress / (float)requirement * 100f), 100);
	}

	public bool Available(int achievementIndex)
	{
		AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(achievementIndex, 1);
		if (data == null)
		{
			return false;
		}
		if (!NGame2.NUI.Common.CheckContentsOpenCondition(data.OpenCondition))
		{
			return false;
		}
		if (data.TutorialIndex != 0 && !GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(data.TutorialIndex))
		{
			return false;
		}
		if (data.LimitedEventMissionType != EventBonusType.None && !string.IsNullOrEmpty(data.EventName) && GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(data.LimitedEventMissionType, data.EventName) == null)
		{
			return false;
		}
		return true;
	}

	void AchievementComponent.IDataProvider.GetAchievementAndRequirement(int AchievementIndex, out long Achievement, out long Requirement, out long PrevRequirement)
	{
		GetAchievementAndRequirement(AchievementIndex, out Achievement, out Requirement, out PrevRequirement);
	}

	AchievementComponent.EAchievementStatus AchievementComponent.IDataProvider.GetAchievementStatus(int AchievementIndex)
	{
		int curAchievementStep = GetCurAchievementStep(AchievementIndex);
		int lastStepOnAchievementData = GetLastStepOnAchievementData(AchievementIndex);
		if (curAchievementStep > lastStepOnAchievementData)
		{
			return AchievementComponent.EAchievementStatus.Completed;
		}
		if (Cleared(AchievementIndex))
		{
			return AchievementComponent.EAchievementStatus.Cleared;
		}
		return AchievementComponent.EAchievementStatus.InProgress;
	}

	AchievementData AchievementComponent.IDataProvider.GetAchievementData(int AchievementIndex)
	{
		return GetAchievementData(AchievementIndex);
	}

	ulong AchievementComponent.IDataProvider.GetLastStepRequirement(int AchievementIndex)
	{
		int lastStepOnAchievementData = GetLastStepOnAchievementData(AchievementIndex);
		return GetAchievementRequirement(GameRoot.Instance.AchievementDataContainer.GetData(AchievementIndex, lastStepOnAchievementData));
	}

	AchievementData AchievementComponent.IDataProvider.GetLastAchievementData(int AchievementIndex)
	{
		int lastStepOnAchievementData = GetLastStepOnAchievementData(AchievementIndex);
		return GameRoot.Instance.AchievementDataContainer.GetData(AchievementIndex, lastStepOnAchievementData);
	}

	AchievementInfo AchievementComponent.IDataProvider.GetAchievementInfo(int AchievementIndex)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(AchievementIndex);
	}

	bool HeroPortraitComponent.IDataProvider.AchievementCompleted(int HeroIndex)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.HeroAchievementCompleted(HeroIndex);
	}

	int HeroPortraitComponent.IDataProvider.GetRewardableCount(int HeroIndex)
	{
		return GetRewardableHeroAchievementCount(HeroIndex);
	}
}
