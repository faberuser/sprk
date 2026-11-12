using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount.NCampaign;
using NGame2.NBattleContext;
using NGame2.NTutorial;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUtil;
using NGame2.NUtil.NConditionChecker;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NAccount;

public class UserCampaignManager : NShared.UserCampaignManager
{
	public class ChapterDungeonInfo : SingletonAuto<ChapterDungeonInfo>
	{
		private LocalAccountInt _storedChapterIndex;

		private LocalAccountInt _storedDungeonIndex;

		public int ChapterIndex { get; private set; }

		public int DungeonIndex { get; private set; }

		public ChapterDungeonInfo()
		{
			Reset();
		}

		public void Reset()
		{
			_storedChapterIndex = new LocalAccountInt(0, typeof(ChapterDungeonInfo).FullName + "_ChapterIndex");
			_storedDungeonIndex = new LocalAccountInt(0, typeof(ChapterDungeonInfo).FullName + "_DungeonIndex");
			ChapterIndex = _storedChapterIndex.Value;
			DungeonIndex = _storedDungeonIndex.Value;
			if (GameRoot.Instance != null && GameRoot.Instance.CampaignNodeDataContainer != null)
			{
				CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(ChapterIndex, DungeonIndex);
				if (data != null && __Extension_Method_ICollection__.COUNT(data.ResetNodeIndex) >= 2)
				{
					ChapterIndex = data.ResetNodeIndex[0];
					DungeonIndex = data.ResetNodeIndex[1];
					LocalAccountInt storedChapterIndex = _storedChapterIndex;
					int chapterIndex = ChapterIndex;
					storedChapterIndex.Value = chapterIndex;
					LocalAccountInt storedDungeonIndex = _storedDungeonIndex;
					int dungeonIndex = DungeonIndex;
					storedDungeonIndex.Value = dungeonIndex;
				}
			}
		}

		public void SetChapter(int chapter, bool isForce = false)
		{
			if (chapter == 0)
			{
				VespaDebug.LogError("Updating chapter to 0.");
				return;
			}
			ChapterIndex = chapter;
			if (!SingletonUpdater<TutorialManager>.instance.IsProgressing || isForce)
			{
				_storedChapterIndex.Value = chapter;
			}
		}

		public void SetDungeon(int dungeon, bool isForce = false)
		{
			DungeonIndex = dungeon;
			if (!SingletonUpdater<TutorialManager>.instance.IsProgressing || isForce)
			{
				_storedDungeonIndex.Value = dungeon;
			}
		}

		private LocalAccountEnum<DungeonDifficultyType> GetDifficultyStorage(int chapterIndex)
		{
			return new LocalAccountEnum<DungeonDifficultyType>(typeof(ChapterDungeonInfo).FullName + "_Difficulty_" + chapterIndex);
		}

		public void SetChapterDifficulty(int chapterIndex, DungeonDifficultyType difficultyType)
		{
			GetDifficultyStorage(chapterIndex).Value = difficultyType;
		}

		public DungeonDifficultyType GetChapterDifficulty(int chapterIndex)
		{
			return GetDifficultyStorage(chapterIndex).Value;
		}

		private LocalAccountEnum<DungeonDifficultyType> GetMaxDifficultyStorage(int chapterIndex, int dungeonIndex)
		{
			return new LocalAccountEnum<DungeonDifficultyType>(typeof(ChapterDungeonInfo).FullName + "_MaxDifficulty_" + chapterIndex + "_" + dungeonIndex);
		}

		public void SetMaxDifficulty(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficultyType)
		{
			GetMaxDifficultyStorage(chapterIndex, dungeonIndex).Value = difficultyType;
		}

		public DungeonDifficultyType GetMaxDifficulty(int chapterIndex, int dungeonIndex)
		{
			return GetMaxDifficultyStorage(chapterIndex, dungeonIndex).Value;
		}
	}

	private readonly UserObserverManager<NShared.ChapterDungeonInfo> _observerManager;

	private LocalAccountInt _LatelyBattleChapterIndex = new LocalAccountInt(0, typeof(LobbyManagement).FullName + "_LatelyBattleChapterIndex_20200213");

	private LocalAccountInt _LatelyBattleDungeonIndex = new LocalAccountInt(0, typeof(LobbyManagement).FullName + "_LatelyBattleDungeonIndex_20200213");

	private LocalAccountInt _LatelyHighLevelChapterIndex = new LocalAccountInt(0, typeof(LobbyManagement).FullName + "_LatelyHighLevelChapterIndex2_20200213");

	private LocalAccountInt _LatelyHighLevelDungeonIndex = new LocalAccountInt(0, typeof(LobbyManagement).FullName + "_LatelyHighLevelDungeonIndex2_20200213");

	private LocalAccountInt _LatelyConquestChapterIndex = new LocalAccountInt(0, typeof(LobbyManagement).FullName + "_LatelyConquestChapterIndex_20200213");

	private LocalAccountInt _LatelyConquestDungeonIndex = new LocalAccountInt(0, typeof(LobbyManagement).FullName + "_LatelyConquestDungeonIndex_20200213");

	private LocalAccountIntArray _LatelyPlayDungeonIndex_Chapter = new LocalAccountIntArray(0, typeof(LobbyManagement).FullName + "_LatelyPlayDungeonIndex_Chapter");

	private LocalAccountIntArray _LatelyHighLevelDungeonIndex_Chapter = new LocalAccountIntArray(1, typeof(LobbyManagement).FullName + "_LatelyHighLevelDungeonIndex_Chapter");

	public static int ChapterIndex
	{
		get
		{
			return SingletonAuto<ChapterDungeonInfo>.instance.ChapterIndex;
		}
		set
		{
			SingletonAuto<ChapterDungeonInfo>.instance.SetChapter(value);
		}
	}

	public static int DungeonIndex
	{
		get
		{
			return SingletonAuto<ChapterDungeonInfo>.instance.DungeonIndex;
		}
		set
		{
			SingletonAuto<ChapterDungeonInfo>.instance.SetDungeon(value);
		}
	}

	public static int LinkedDungeonIndex => GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLinkedDungeonIndex();

	public static int InvalidDungeonIndex => -1;

	public IUserObserverManager ObserverManager => _observerManager;

	public int LatelyBattleChapterIndex
	{
		get
		{
			return _LatelyBattleChapterIndex.Value;
		}
		set
		{
			_LatelyBattleChapterIndex.Value = value;
		}
	}

	public int LatelyBattleDungeonIndex
	{
		get
		{
			return _LatelyBattleDungeonIndex.Value;
		}
		set
		{
			_LatelyBattleDungeonIndex.Value = value;
		}
	}

	public int LatelyHighLevelChapterIndex
	{
		get
		{
			return _LatelyHighLevelChapterIndex.Value;
		}
		set
		{
			_LatelyHighLevelChapterIndex.Value = value;
		}
	}

	public int LatelyHighLevelDungeonIndex
	{
		get
		{
			return _LatelyHighLevelDungeonIndex.Value;
		}
		set
		{
			_LatelyHighLevelDungeonIndex.Value = value;
		}
	}

	public int LatelyConquestChapterIndex
	{
		get
		{
			return _LatelyConquestChapterIndex.Value;
		}
		set
		{
			_LatelyConquestChapterIndex.Value = value;
		}
	}

	public int LatelyConquestDungeonIndex
	{
		get
		{
			return _LatelyConquestDungeonIndex.Value;
		}
		set
		{
			_LatelyConquestDungeonIndex.Value = value;
		}
	}

	public static void SetChapterDungeonIndex(int chapterIndex, int dungeonIndex, bool isForce = false)
	{
		SingletonAuto<ChapterDungeonInfo>.instance.SetChapter(chapterIndex, isForce);
		SingletonAuto<ChapterDungeonInfo>.instance.SetDungeon(dungeonIndex, isForce);
	}

	public static void SetMaxDungeonDifficultyType(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficultyType)
	{
		SingletonAuto<ChapterDungeonInfo>.instance.SetMaxDifficulty(chapterIndex, dungeonIndex, difficultyType);
	}

	public static DungeonDifficultyType GetMaxDungeonDifficultyType(int chapterIndex, int dungeonIndex)
	{
		return SingletonAuto<ChapterDungeonInfo>.instance.GetMaxDifficulty(chapterIndex, dungeonIndex);
	}

	public UserCampaignManager()
	{
		_observerManager = new UserObserverManager<NShared.ChapterDungeonInfo>();
	}

	private void AnnounceToObservers(NShared.ChapterDungeonInfo info)
	{
		_observerManager.AnnounceToObservers(info);
	}

	public void Apply(NShared.ChapterDungeonInfo resultInfo)
	{
		if (resultInfo != null)
		{
			Add(resultInfo);
			AnnounceToObservers(resultInfo);
		}
	}

	public void Apply(NShared.ChapterDungeonInfo[] resultInfos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(resultInfos); i++)
		{
			Apply(resultInfos[i]);
		}
	}

	public bool IsCompleted(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficultyType = DungeonDifficultyType.Easy)
	{
		if (difficultyType == DungeonDifficultyType.Easy)
		{
			difficultyType = (DungeonDifficultyType)Helper.GetMinDifficulty(chapterIndex, dungeonIndex);
		}
		return SingletonAuto<DungeonCompleteCheckerContainer>.instance.Check(chapterIndex, dungeonIndex, difficultyType);
	}

	public bool IsVisibleAvatar(int chapterIndex, int dungeonIndex, CampaignNodeData nodeData)
	{
		if (SingletonAuto<ConditionChecker>.instance.CheckMatchCondition<NodeVisibleChecker>(nodeData))
		{
			return SingletonAuto<ConditionChecker>.instance.CheckCondition<NodeVisibleChecker>(nodeData);
		}
		return true;
	}

	public bool IsShakmehNode(int chapterIndex, int dungeonIndex, CampaignNodeData nodeData)
	{
		if (nodeData.Function != NodeFunctionType.ShakmehDungeon)
		{
			return false;
		}
		if (SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance == null)
		{
			return false;
		}
		if (GameRoot.Instance.ShakmehBossDataContainer.GetData(chapterIndex, dungeonIndex) == null)
		{
			return false;
		}
		return true;
	}

	public bool IsOrdealArenaNode(int chapterIndex, int dungeonIndex, CampaignNodeData nodeData)
	{
		if (nodeData.Function == NodeFunctionType.OrdealArenaBattle || nodeData.Function == NodeFunctionType.OrdealArenaEnd || nodeData.Function == NodeFunctionType.OrdealArenaEvent || nodeData.Function == NodeFunctionType.OrdealArenaStart)
		{
			return true;
		}
		return false;
	}

	public bool IsCompleteScenario(int chapterIndex, int dungeonIndex)
	{
		NShared.ChapterDungeonInfo chapterDungeonInfo = Get(chapterIndex, dungeonIndex);
		if (chapterDungeonInfo == null)
		{
			return false;
		}
		return chapterDungeonInfo.ScenarioComplete != 0;
	}

	public bool CanPlayScenario(int chapterIndex, int dungeonIndex)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return false;
		}
		if (!data.Scenario)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(data.ReqScenarioDungeon) < 2)
		{
			return true;
		}
		int chapterIndex2 = data.ReqScenarioDungeon[0];
		int dungeonIndex2 = data.ReqScenarioDungeon[1];
		NShared.ChapterDungeonInfo chapterDungeonInfo = Get(chapterIndex2, dungeonIndex2);
		if (chapterDungeonInfo == null || chapterDungeonInfo.ScenarioComplete == 0)
		{
			return false;
		}
		return true;
	}

	public int GetCompleteStar(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficultyType = DungeonDifficultyType.Easy)
	{
		NShared.ChapterDungeonInfo chapterDungeonInfo = Get(chapterIndex, dungeonIndex);
		if (chapterDungeonInfo == null)
		{
			return 0;
		}
		if (string.IsNullOrEmpty(chapterDungeonInfo.CompletedTime))
		{
			return 0;
		}
		if (chapterDungeonInfo.MaxStar / 10 < (int)difficultyType)
		{
			return 0;
		}
		return Mathf.Min(3, chapterDungeonInfo.MaxStar - (int)difficultyType * 10);
	}

	public void GetCompleteStar(int chapterIndex, int dungeonIndex, out DungeonDifficultyType oDifficultyType, out int oStar)
	{
		NShared.ChapterDungeonInfo chapterDungeonInfo = Get(chapterIndex, dungeonIndex);
		if (chapterDungeonInfo == null)
		{
			oDifficultyType = DungeonDifficultyType.Easy;
			oStar = 0;
			return;
		}
		if (string.IsNullOrEmpty(chapterDungeonInfo.CompletedTime))
		{
			oDifficultyType = DungeonDifficultyType.Easy;
			oStar = 0;
			return;
		}
		int num = (int)(oDifficultyType = (DungeonDifficultyType)System.Math.Min(3, chapterDungeonInfo.MaxStar / 10));
		int num2 = chapterDungeonInfo.MaxStar / 10;
		if (num2 < num)
		{
			oStar = 0;
		}
		else
		{
			oStar = Mathf.Min(3, chapterDungeonInfo.MaxStar - num2 * 10);
		}
	}

	public bool IsAlwaysOpened(int chapterIndex, int dungeonIndex)
	{
		return GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex)?.Opened ?? false;
	}

	public int FindPrevBattleNode(int chapterIndex, int dungeonIndex)
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		HashSet<int> hashSet = new HashSet<int>();
		list.AddRange(GameRoot.Instance.CampaignNodeDataContainer.GetPrevDungeonIndices(chapterIndex, dungeonIndex));
		while (list.Count > 0)
		{
			foreach (int item in list)
			{
				hashSet.Add(item);
				if (GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, item).Function == NodeFunctionType.Battle)
				{
					return item;
				}
				foreach (int prevDungeonIndex in GameRoot.Instance.CampaignNodeDataContainer.GetPrevDungeonIndices(chapterIndex, item))
				{
					if (!hashSet.Contains(prevDungeonIndex))
					{
						list2.Add(prevDungeonIndex);
					}
				}
			}
			list.Clear();
			list.AddRange(list2);
			list2.Clear();
		}
		return InvalidDungeonIndex;
	}

	public bool CanVisit(int chapterIndex, int dungeonIndex)
	{
		HashSet<int> checkedDungeonIndexList = new HashSet<int>();
		return CanVisit(chapterIndex, dungeonIndex, checkedDungeonIndexList);
	}

	public bool CanVisit(int chapterIndex, int dungeonIndex, HashSet<int> checkedDungeonIndexList)
	{
		if (checkedDungeonIndexList.Contains(dungeonIndex))
		{
			return false;
		}
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return false;
		}
		if (SingletonAuto<ConditionChecker>.instance.CheckMatchCondition<NodeVisibleChecker>(data))
		{
			if (data.ReqDungeonIndex != 0 && !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(data.ChapterIndex, data.ReqDungeonIndex))
			{
				return false;
			}
			return SingletonAuto<ConditionChecker>.instance.CheckCondition<NodeVisibleChecker>(data);
		}
		if (IsCompleted(chapterIndex, dungeonIndex))
		{
			return true;
		}
		if (data.Opened)
		{
			return true;
		}
		if (!SingletonAuto<ConditionChecker>.instance.CheckNodeOpen(data))
		{
			return false;
		}
		if (!CanGoToChapter(chapterIndex))
		{
			return false;
		}
		HashSet<int> prevDungeonIndices = GameRoot.Instance.CampaignNodeDataContainer.GetPrevDungeonIndices(chapterIndex, dungeonIndex);
		if (prevDungeonIndices == null || prevDungeonIndices.Count == 0)
		{
			if (data.ReqDungeonIndex == 0 && data.ReqTutorialIndex == 0 && __Extension_Method_ICollection__.IsNullOrEmpty(data.ReqCondition))
			{
				VespaDebug.LogError("prevDungeonIndexList is null: " + dungeonIndex);
				return false;
			}
			return true;
		}
		if (prevDungeonIndices.Any((int prevDungeonIndex) => IsCompleted(chapterIndex, prevDungeonIndex)))
		{
			return true;
		}
		if (prevDungeonIndices.Any((int prevDungeonIndex) => IsAlwaysOpened(chapterIndex, prevDungeonIndex)))
		{
			return true;
		}
		checkedDungeonIndexList.Add(dungeonIndex);
		foreach (int item in prevDungeonIndices)
		{
			CampaignNodeData data2 = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, item);
			if (data2 != null && !data2.Function.IsBattleFunction() && !data2.Function.IsSubStoryFunction() && !data2.Function.IsOrdealArenaFuction() && CanVisit(chapterIndex, item, checkedDungeonIndexList))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanBypass(int chapterIndex, int dungeonIndex)
	{
		if (IsCompleted(chapterIndex, dungeonIndex))
		{
			return true;
		}
		if (!CanVisit(chapterIndex, dungeonIndex))
		{
			return false;
		}
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return false;
		}
		if (!data.Function.IsBattleFunction())
		{
			return true;
		}
		if (data.Opened)
		{
			return true;
		}
		return false;
	}

	public bool CanGoToChapter(int chapterIndex)
	{
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data == null)
		{
			return false;
		}
		if (data.ReqChapterIndex != 0)
		{
			if (data.ReqDungeonIndex != 0)
			{
				if (!IsCompleted(data.ReqChapterIndex, data.ReqDungeonIndex, data.ReqDungeonDifficulty))
				{
					return false;
				}
			}
			else if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.ChapterCompleted(data.ReqChapterIndex))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanOpenChapterPortal(int chapterIndex)
	{
		if (GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex) == null)
		{
			return false;
		}
		if (chapterIndex != 0)
		{
			if (!IsCompleted(chapterIndex, 1))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool CheckEventDungeon(int chapterIndex, int dungeonIndex, out long maxTryCount, out long remainCount)
	{
		int num = 0;
		maxTryCount = num;
		int num2 = 0;
		remainCount = num2;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return false;
		}
		int num3 = Get(chapterIndex, dungeonIndex)?.DailyCompletedCount ?? 0;
		if (data.Value != null && data.Value.Count() > 0)
		{
			int num4 = data.Value[0];
			maxTryCount = num4;
			long num5 = maxTryCount;
			int num6 = num3;
			long val = checked(num5 - num6);
			int num7 = 0;
			remainCount = System.Math.Max(val, num7);
			return true;
		}
		return false;
	}

	public bool CheckEnterCount(EnterDungeonData enterDungeonData)
	{
		BattleType battleType = enterDungeonData.LinkedDungeonData.BattleType;
		if (battleType != BattleType.Event && battleType != BattleType.EventDungeon && battleType != BattleType.Campaign)
		{
			return true;
		}
		if (enterDungeonData == null || enterDungeonData.ChapterData == null || enterDungeonData.LinkedDungeonData == null)
		{
			return false;
		}
		if (!enterDungeonData.LinkedDungeonData.IsExceededEntryChapter)
		{
			return true;
		}
		int dailyEnterMaxCount = enterDungeonData.ChapterData.DailyEnterMaxCount;
		if (dailyEnterMaxCount > 0 && dailyEnterMaxCount - GetDailyCompletedCountByChapter(enterDungeonData.ChapterData.Index) <= 0)
		{
			return false;
		}
		return true;
	}

	public int GetDailyCompletedCountByChapter(int chapterIndex, int[] dungeonIndices = null)
	{
		List<int> list = new List<int>();
		list.Clear();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(dungeonIndices))
		{
			foreach (CampaignDungeonData allChapterDatum in GameRoot.Instance.CampaignDungeonDataContainer.GetAllChapterData(chapterIndex))
			{
				if (allChapterDatum != null && allChapterDatum.IsExceededEntryChapter)
				{
					list.Add(allChapterDatum.DungeonIndex);
				}
			}
		}
		else
		{
			list.AddRange(dungeonIndices.ToList());
		}
		int num = 0;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return num;
		}
		NShared.ChapterDungeonInfo chapterDungeonInfo = null;
		for (int i = 0; i < list.Count(); i++)
		{
			chapterDungeonInfo = Get(chapterIndex, list[i]);
			if (chapterDungeonInfo != null)
			{
				num += chapterDungeonInfo.DailyCompletedCount;
			}
		}
		return num;
	}

	public bool CheckConquestDungeon(int chapterIndex, int dungeonIndex)
	{
		if (GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex) == null)
		{
			return false;
		}
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return false;
		}
		if (data.BattleType != BattleType.Conquest)
		{
			return false;
		}
		return true;
	}

	public int GetLinkedDungeonIndex()
	{
		int chapterIndex = ChapterIndex;
		int dungeonIndex = DungeonIndex;
		return GetLinkedDungeonIndex(chapterIndex, dungeonIndex);
	}

	public int GetLinkedDungeonIndex(int chapterIndex, int dungeonIndex)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return dungeonIndex;
		}
		if (IsCompleted(chapterIndex, dungeonIndex))
		{
			return dungeonIndex;
		}
		int firstDungeonIndex = data.FirstDungeonIndex;
		if (firstDungeonIndex == 0)
		{
			return dungeonIndex;
		}
		return firstDungeonIndex;
	}

	public bool ChapterCleared(int chapterIndex)
	{
		int chapterAllStar = GetChapterAllStar(chapterIndex);
		int totalStar = GameRoot.Instance.CampaignDungeonDataContainer.GetTotalStar(chapterIndex);
		return chapterAllStar >= totalStar;
	}

	public bool ClearedChapter(int chapterIndex)
	{
		IList<CampaignDungeonData> allChapterData = GameRoot.Instance.CampaignDungeonDataContainer.GetAllChapterData(chapterIndex);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allChapterData))
		{
			return false;
		}
		foreach (CampaignDungeonData item in allChapterData)
		{
			if (item != null && !IsCompleted(chapterIndex, item.DungeonIndex))
			{
				return false;
			}
		}
		return true;
	}

	private int GetStar(int chapterIndex, int dungeonIndex)
	{
		NShared.ChapterDungeonInfo chapterDungeonInfo = Get(chapterIndex, dungeonIndex);
		if (chapterDungeonInfo == null)
		{
			return 0;
		}
		return chapterDungeonInfo.MaxStar % 10;
	}

	public int GetChapterAllStar(int chapterIndex)
	{
		List<int> battleDungeonIndices = GameRoot.Instance.CampaignDungeonDataContainer.GetBattleDungeonIndices(chapterIndex);
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(battleDungeonIndices); i++)
		{
			int star = GetStar(chapterIndex, battleDungeonIndices[i]);
			num += star;
		}
		return num;
	}

	public int ClearDungeonStar(int chapterIndex, int totalStar)
	{
		List<int> dungeonIndexByChapterIndex = GameRoot.Instance.CampaignDungeonDataContainer.GetDungeonIndexByChapterIndex(chapterIndex);
		int num = 0;
		for (int i = 0; i < dungeonIndexByChapterIndex.Count; i++)
		{
			int star = GetStar(chapterIndex, dungeonIndexByChapterIndex[i]);
			num += star;
		}
		return Mathf.Min(num, totalStar);
	}

	public int GetChapterFinalClearDungeon(int chapterIndex, DungeonDifficultyType difficulty = DungeonDifficultyType.Easy)
	{
		List<int> battleDungeonIndices = GameRoot.Instance.CampaignDungeonDataContainer.GetBattleDungeonIndices(chapterIndex);
		int result = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(battleDungeonIndices); i++)
		{
			CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, battleDungeonIndices[i]);
			if (!data.IsBranch && (data.BattleType == BattleType.Campaign || data.BattleType == BattleType.Boss) && GetStar(chapterIndex, battleDungeonIndices[i]) > 0)
			{
				result = battleDungeonIndices[i];
			}
		}
		return result;
	}

	public int GetChapterLastPlayDungeon(int chapterIndex, DungeonDifficultyType difficulty = DungeonDifficultyType.Easy)
	{
		List<int> battleDungeonIndices = GameRoot.Instance.CampaignDungeonDataContainer.GetBattleDungeonIndices(chapterIndex);
		int result = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(battleDungeonIndices); i++)
		{
			CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, battleDungeonIndices[i]);
			if (!data.IsBranch && (data.BattleType == BattleType.Campaign || data.BattleType == BattleType.Boss) && GetStar(chapterIndex, battleDungeonIndices[i]) > 0)
			{
				result = battleDungeonIndices[i];
			}
		}
		return result;
	}

	public void GetChapterClearDungeonAchievement(int chapterIndex, out long clearedDungeon, out long totalDungeon)
	{
		List<int> battleDungeonIndices = GameRoot.Instance.CampaignDungeonDataContainer.GetBattleDungeonIndices(chapterIndex);
		totalDungeon = __Extension_Method_ICollection__.COUNT(battleDungeonIndices);
		clearedDungeon = 0L;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(battleDungeonIndices); i++)
		{
			if (GetStar(chapterIndex, battleDungeonIndices[i]) > 0)
			{
				clearedDungeon++;
			}
		}
	}

	public bool FirstClearRewarded(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty = DungeonDifficultyType.Easy)
	{
		NShared.ChapterDungeonInfo chapterDungeonInfo = Get(chapterIndex, dungeonIndex);
		if (chapterDungeonInfo == null)
		{
			return false;
		}
		return ((short)(1 << (int)difficulty) & chapterDungeonInfo.FirstRewardedDiff) > 0;
	}

	public void SaveLatelyBattleNodePos(int chapterIndex, int dungeonIndex)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return;
		}
		if (data.BattleType == BattleType.Conquest)
		{
			LatelyConquestChapterIndex = chapterIndex;
			LatelyConquestDungeonIndex = dungeonIndex;
		}
		if (data.BattleType == BattleType.Campaign || data.BattleType == BattleType.Boss || data.BattleType == BattleType.Hideout || data.BattleType == BattleType.Shakmeh)
		{
			if (data.BattleType == BattleType.Hideout)
			{
				LatelyHighLevelChapterIndex = chapterIndex;
				LatelyHighLevelDungeonIndex = dungeonIndex;
				SetLatelyHighLevelDungeonIndex_Chapter(chapterIndex, dungeonIndex);
			}
			LatelyBattleChapterIndex = chapterIndex;
			LatelyBattleDungeonIndex = dungeonIndex;
			SetLatelyPlayDungeonIndex_Chapter(chapterIndex, dungeonIndex);
		}
	}

	public int GetLatelyPlayDungeonIndex_Chapter(int ChapterIndex)
	{
		return _LatelyPlayDungeonIndex_Chapter[ChapterIndex];
	}

	public void SetLatelyPlayDungeonIndex_Chapter(int ChapterIndex, int DungeonIndex)
	{
		_LatelyPlayDungeonIndex_Chapter[ChapterIndex] = DungeonIndex;
	}

	public int GetLatelyHighLevelDungeonIndex_Chapter(int ChapterIndex)
	{
		return _LatelyHighLevelDungeonIndex_Chapter[ChapterIndex];
	}

	public void SetLatelyHighLevelDungeonIndex_Chapter(int ChapterIndex, int DungeonIndex)
	{
		_LatelyHighLevelDungeonIndex_Chapter[ChapterIndex] = DungeonIndex;
	}
}
