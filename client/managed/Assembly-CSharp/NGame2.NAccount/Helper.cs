using System;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NContents;
using NGame2.NDropBonusEvent;
using NGame2.NEventTrigger;
using NGame2.NSubStory;
using NGame2.NUI.NWindow.NodeFunctionType;
using NGame2.NUtil;
using NGameAsset;
using NShared;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.NWorldMap.NEventSubData;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NAccount;

public static class Helper
{
	public enum ECostumeState
	{
		None,
		Equipped,
		Owned,
		Buyable,
		NeedHero,
		NeedAwaken,
		CannotBuy
	}

	public struct LevelInfo
	{
		public int Level;

		public long CurExp;

		public long LocalExp;

		public float ExpPercent;

		public bool IsMaxLevel;

		public static LevelInfo CreateTeamLevelInfo(int level, long curExp)
		{
			return new LevelInfo(level, curExp, GameRoot.Instance.TeamLevelDataContainer);
		}

		public static LevelInfo CreateHeroLevelInfo(int level, long curExp)
		{
			return new LevelInfo(level, curExp, GameRoot.Instance.CreatureLevelDataContainer);
		}

		private LevelInfo(int level, long curExp, IExpGetter expGetter)
		{
			Level = level;
			CurExp = curExp;
			TeamLevelData data = GameRoot.Instance.TeamLevelDataContainer.GetData(level);
			if (data == null || data.LocalExp <= 0)
			{
				LocalExp = 0L;
				ExpPercent = 1f;
				IsMaxLevel = true;
			}
			else
			{
				LocalExp = data.LocalExp;
				ExpPercent = (float)CurExp / (float)LocalExp;
				IsMaxLevel = false;
			}
		}
	}

	public struct SkillExtendInfo
	{
		public enum EExtendStatus
		{
			Extendable,
			AlreadyExtended,
			PrevExtendNeeded,
			NotEnoughItem,
			NotLearnedCoreSkill
		}

		public SkillData TableData;

		public EExtendStatus ExtendStatus;

		public int ExtendItemCount;

		public bool CanLearn => ExtendStatus == EExtendStatus.Extendable;

		public bool CannotLearnNoExtended
		{
			get
			{
				if (!CanLearn)
				{
					return ExtendStatus != EExtendStatus.AlreadyExtended;
				}
				return false;
			}
		}
	}

	public struct SkillBasicInfo
	{
		public int TableIndex;

		public int Level;

		public int TranscendLevel;

		public bool IsLocked;

		public SkillData TableData;

		public string StackTypeName;

		public string ItemCode;
	}

	public struct SkillDetailInfo
	{
		public SkillBasicInfo BasicInfo;

		public SkillBookInfo BookInfo;

		public int UpgradeGold;

		public bool CanLevelUp;

		public int ReqHeroLevel;

		public SkillExtendInfo[] ExtendInfos;
	}

	public struct SkillBookInfo
	{
		public ItemData BookItemData;

		public string CreatureTagType;

		public int CurItemCount;
	}

	public struct UpgradeHeroStarInfo
	{
		public string ItemCode;

		public int ItemCount;

		public int ReqItemCountForNext;

		public int ReqGold;

		public bool IsEnoughGold;

		public bool IsEnoughItem;

		public bool CanUpgrade;
	}

	public struct ReqItemInfo
	{
		public string ItemCode;

		public int MyItemCount;

		public int ReqItemCount;
	}

	public class ReqResourceInfo
	{
		public long ReqGold;

		public List<ReqItemInfo> ReqItemList = new List<ReqItemInfo>();

		public bool IsEnough
		{
			get
			{
				if (IsEnoughGold)
				{
					return IsEnoughItems;
				}
				return false;
			}
		}

		public bool IsEnoughGold => GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold >= ReqGold;

		public bool IsEnoughItems
		{
			get
			{
				for (int i = 0; i < ReqItemList.Count; i++)
				{
					ReqItemInfo reqItemInfo = ReqItemList[i];
					if (reqItemInfo.MyItemCount < reqItemInfo.ReqItemCount)
					{
						return false;
					}
				}
				return true;
			}
		}
	}

	public class FunctionStateData
	{
		public enum EFunctionState
		{
			Enabled,
			AllNodeFunctionLocked,
			CurrentNodeFunctionLocked,
			Disabled
		}

		public string Button;

		public NodeFunctionType Type;

		public EFunctionState State;

		public string DisableMsg;
	}

	private static int _MaxHeroExp;

	private static Dictionary<string, bool> _restrictionCache = new Dictionary<string, bool>();

	public static FunctionStateData GetFunctionState(int chapterIndex, int dungeonIndex, bool containsDisableText)
	{
		return GetFunctionState(GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex), containsDisableText);
	}

	public static FunctionStateData GetFunctionState(CampaignNodeData nodeData, bool containsDisableMsg)
	{
		FunctionStateData functionStateData = new FunctionStateData();
		functionStateData.Type = NodeFunctionType.None;
		if (nodeData == null)
		{
			return functionStateData;
		}
		int chapterIndex = nodeData.ChapterIndex;
		int dungeonIndex = nodeData.DungeonIndex;
		if (HasEventTrigger(chapterIndex, dungeonIndex))
		{
			functionStateData.Type = NodeFunctionType.EventTrigger;
			functionStateData.State = FunctionStateData.EFunctionState.Enabled;
			return functionStateData;
		}
		if (HasExcavationEvent(chapterIndex, dungeonIndex))
		{
			functionStateData.Type = NodeFunctionType.Excavation;
			functionStateData.State = IsAllNodeFunctionOpened(containsDisableMsg, out functionStateData.DisableMsg);
			return functionStateData;
		}
		functionStateData.Type = SwitchingFunctionType(nodeData);
		functionStateData.Button = nodeData.FunctionButton;
		functionStateData.State = IsNodeFunctionOpened(functionStateData.Type, chapterIndex, dungeonIndex, containsDisableMsg, out functionStateData.DisableMsg);
		if (functionStateData.Type == NodeFunctionType.None && HasNpc(chapterIndex, dungeonIndex))
		{
			functionStateData.Type = NodeFunctionType.NPC;
			functionStateData.State = IsAllNodeFunctionOpened(containsDisableMsg, out functionStateData.DisableMsg);
		}
		return functionStateData;
	}

	public static NodeFunctionType SwitchingFunctionType(CampaignNodeData nodeData)
	{
		if (nodeData.Function != NodeFunctionType.Switching)
		{
			return nodeData.Function;
		}
		string switchCondtionValue = nodeData.GetSwitchCondtionValue(0);
		return SingletonAuto<SwitchingContainer>.instance.GetCondition(switchCondtionValue)?.GetFunctionType(nodeData.SwitchCondition, nodeData.Function) ?? nodeData.Function;
	}

	public static bool HasExcavationEvent(int chapterIndex, int dungeonIndex)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.GetEventData<Excavation>(chapterIndex, dungeonIndex) != null;
	}

	public static bool HasEventTrigger(int chapterIndex, int dungeonIndex)
	{
		NodeFunctionMenu nodeFunctionMenu = new NodeFunctionMenu();
		nodeFunctionMenu.ChapterIndex = chapterIndex;
		nodeFunctionMenu.DungeonIndex = dungeonIndex;
		return SingletonAuto<EventTriggerManager>.instance.CanRaiseEvent(nodeFunctionMenu, ignoreCurrent: true);
	}

	public static bool CheckDropBonusEventNode(int chapterIndex, int dungeonIndex)
	{
		bool result = false;
		List<DropBonusEvent> allEffectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent(EventBonusType.EventNode);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allEffectiveEvent); i++)
		{
			string condition = allEffectiveEvent[i].Condition;
			if (String_Extension_Method.IsNullOrEmpty(condition))
			{
				continue;
			}
			string[] array = condition.Split(new char[1] { ',' });
			if (__Extension_Method_ICollection__.COUNT(array) >= 2)
			{
				string a = array[0].Trim();
				string a2 = array[1].Trim();
				if (string.Equals(a, chapterIndex.ToString(), StringComparison.InvariantCultureIgnoreCase) && string.Equals(a2, dungeonIndex.ToString(), StringComparison.InvariantCultureIgnoreCase))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public static List<DropBonusEvent> GetDropBonusEventNodeInfo(int chapterIndex, int dungeonIndex)
	{
		List<DropBonusEvent> list = new List<DropBonusEvent>();
		List<DropBonusEvent> allEffectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent(EventBonusType.EventNode);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allEffectiveEvent); i++)
		{
			DropBonusEvent dropBonusEvent = allEffectiveEvent[i];
			string condition = dropBonusEvent.Condition;
			if (String_Extension_Method.IsNullOrEmpty(condition))
			{
				continue;
			}
			string[] array = condition.Split(new char[1] { ',' });
			if (__Extension_Method_ICollection__.COUNT(array) >= 2)
			{
				string a = array[0].Trim();
				string a2 = array[1].Trim();
				if (string.Equals(a, chapterIndex.ToString(), StringComparison.InvariantCultureIgnoreCase) && string.Equals(a2, dungeonIndex.ToString(), StringComparison.InvariantCultureIgnoreCase))
				{
					list.Add(dropBonusEvent);
				}
			}
		}
		return list;
	}

	public static ECostumeState GetCostumeState(CostumeData equippedCostumeData, CostumeData costumeData, bool checkNeedHero = true)
	{
		if (costumeData == null)
		{
			return ECostumeState.None;
		}
		if (equippedCostumeData != null && equippedCostumeData.CostumeIndex == costumeData.CostumeIndex)
		{
			return ECostumeState.Equipped;
		}
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(costumeData.HeroIndex);
		if (checkNeedHero && cachedHeroInfo == null)
		{
			return ECostumeState.NeedHero;
		}
		if (costumeData.EquipCondition == EEquipCondition.NeedAwaken && costumeData.HeroStar > 0 && cachedHeroInfo.Star < costumeData.HeroStar)
		{
			return ECostumeState.NeedAwaken;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(costumeData.CostumeIndex) != null)
		{
			return ECostumeState.Owned;
		}
		if (costumeData.IsFreeCostume())
		{
			return ECostumeState.Owned;
		}
		return ECostumeState.Buyable;
	}

	public static bool HasNpc(int chapterIndex, int dungeonIndex)
	{
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null || String_Extension_Method.IsNullOrEmpty(data.EntryNPCCode))
		{
			return false;
		}
		return GameRoot.Instance.NPCDataContainer.GetData(data.EntryNPCCode)?.EntryMenuExist() ?? false;
	}

	public static int GetMinDifficulty(int chapterIndex, int dungeonIndex)
	{
		CampaignWaveDataContainer.WaveCollectionData waveCollection = GameRoot.Instance.CampaignWaveDataContainer.GetWaveCollection(chapterIndex, dungeonIndex);
		if (waveCollection == null || __Extension_Method_ICollection__.IsNullOrEmpty(waveCollection.DifficultyDic))
		{
			return 0;
		}
		return (int)waveCollection.MinDifficulty;
	}

	public static int GetMaxDifficulty(int chapterIndex, int dungeonIndex)
	{
		CampaignWaveDataContainer.WaveCollectionData waveCollection = GameRoot.Instance.CampaignWaveDataContainer.GetWaveCollection(chapterIndex, dungeonIndex);
		if (waveCollection == null || __Extension_Method_ICollection__.IsNullOrEmpty(waveCollection.DifficultyDic))
		{
			return 0;
		}
		return (int)waveCollection.MaxDifficulty;
	}

	public static int GetMaxAvailableDifficulty(int chapterIndex, int dungeonIndex)
	{
		CampaignWaveDataContainer.WaveCollectionData waveCollection = GameRoot.Instance.CampaignWaveDataContainer.GetWaveCollection(chapterIndex, dungeonIndex);
		if (waveCollection == null || __Extension_Method_ICollection__.IsNullOrEmpty(waveCollection.DifficultyDic))
		{
			VespaDebug.LogError("WaveData of any difficulty not found : chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			return 1;
		}
		if (!AccountManager.IsLogined())
		{
			return (int)waveCollection.MaxDifficulty;
		}
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		NShared.DungeonDifficultyType minDifficulty = waveCollection.MinDifficulty;
		short num = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Get(chapterIndex, dungeonIndex)?.MaxStar ?? 0;
		int num2 = System.Math.Max(num / 10, (int)minDifficulty);
		if (num % 10 < 3)
		{
			return num2;
		}
		for (int i = 0; i < 100; i++)
		{
			if (num2 >= (int)waveCollection.MaxDifficulty)
			{
				return num2;
			}
			if (num2 + 1 != 3 || data == null || data.HellMode_ReqChapterIndex == 0 || data.HellMode_ReqDungeonIndex == 0 || GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(data.HellMode_ReqChapterIndex, data.HellMode_ReqDungeonIndex, data.HellMode_ReqDungeonDifficulty))
			{
				num2++;
				if (waveCollection.DifficultyDic.ContainsKey((NShared.DungeonDifficultyType)num2))
				{
					break;
				}
			}
		}
		return Mathf.Min(num2, (int)waveCollection.MaxDifficulty);
	}

	public static int GetDifficulty(int chapterIndex, int dungeonIndex)
	{
		NShared.DungeonDifficultyType maxAvailableDifficulty = (NShared.DungeonDifficultyType)GetMaxAvailableDifficulty(chapterIndex, dungeonIndex);
		NShared.DungeonDifficultyType chapterDifficulty = SingletonAuto<UserCampaignManager.ChapterDungeonInfo>.instance.GetChapterDifficulty(chapterIndex);
		if (chapterDifficulty < maxAvailableDifficulty)
		{
			CampaignWaveDataContainer.WaveCollectionData waveCollection = GameRoot.Instance.CampaignWaveDataContainer.GetWaveCollection(chapterIndex, dungeonIndex);
			if (waveCollection != null && waveCollection.DifficultyDic.ContainsKey(chapterDifficulty))
			{
				return (int)chapterDifficulty;
			}
		}
		return (int)maxAvailableDifficulty;
	}

	public static bool CheckDifficulty(int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficulty)
	{
		CampaignWaveDataContainer.WaveCollectionData waveCollection = GameRoot.Instance.CampaignWaveDataContainer.GetWaveCollection(chapterIndex, dungeonIndex);
		if (waveCollection == null || __Extension_Method_ICollection__.IsNullOrEmpty(waveCollection.DifficultyDic))
		{
			return false;
		}
		return waveCollection.DifficultyDic.ContainsKey(difficulty);
	}

	public static string GetArg(string[] args, int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(args, index))
		{
			return string.Empty;
		}
		return args[index];
	}

	public static string GetArg(HashString[] args, int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(args, index))
		{
			return string.Empty;
		}
		return args[index];
	}

	public static FunctionStateData.EFunctionState IsAllNodeFunctionOpened(bool containsDisableMsg, out string oDisableMsg)
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("OpenNodeEventChapterIndex");
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("OpenNodeEventDungeonIndex");
		if (valueInt == 0 || valueInt2 == 0)
		{
			oDisableMsg = null;
			return FunctionStateData.EFunctionState.Enabled;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(valueInt, valueInt2))
		{
			oDisableMsg = LocalizationManager.GetString(NodeFunction.ALL_NODE_FUNCTION_LOCKED, valueInt, valueInt2);
			return FunctionStateData.EFunctionState.AllNodeFunctionLocked;
		}
		oDisableMsg = null;
		return FunctionStateData.EFunctionState.Enabled;
	}

	public static FunctionStateData.EFunctionState IsNodeFunctionOpened(NodeFunctionType functionType, int chapterIndex, int dungeonIndex, bool containsDisableMsg, out string oDisableMsg)
	{
		switch (functionType)
		{
		case NodeFunctionType.Battle:
		case NodeFunctionType.SubStoryBoss:
			oDisableMsg = null;
			return FunctionStateData.EFunctionState.Enabled;
		case NodeFunctionType.BattleOnce:
			oDisableMsg = null;
			if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex))
			{
				return FunctionStateData.EFunctionState.Enabled;
			}
			return FunctionStateData.EFunctionState.Disabled;
		default:
		{
			if (IsAllNodeFunctionOpened(containsDisableMsg, out oDisableMsg) != FunctionStateData.EFunctionState.Enabled)
			{
				return FunctionStateData.EFunctionState.AllNodeFunctionLocked;
			}
			CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
			if (data == null)
			{
				oDisableMsg = null;
				return FunctionStateData.EFunctionState.Enabled;
			}
			FunctionStateData.EFunctionState eFunctionState = FunctionStateData.EFunctionState.Enabled;
			switch (SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(data.ContentsType))
			{
			case ContentsStatus.Hide:
				eFunctionState = FunctionStateData.EFunctionState.Disabled;
				break;
			case ContentsStatus.Disable:
				eFunctionState = FunctionStateData.EFunctionState.Disabled;
				break;
			}
			if (eFunctionState != FunctionStateData.EFunctionState.Enabled)
			{
				oDisableMsg = LocalizationManager.GetString(NodeFunction.NOT_READY);
				return eFunctionState;
			}
			if (data.MenuOpenCondition == CampaignNodeData.EMenuOpenCondition.None)
			{
				oDisableMsg = null;
				return FunctionStateData.EFunctionState.Enabled;
			}
			if (data.MenuOpenCondition == CampaignNodeData.EMenuOpenCondition.ClearDungeon)
			{
				int num = Vespa_String_Extension_Method.TOINT(GetArg(data.MenuOpenConditionArgs, 0), 0);
				int num2 = Vespa_String_Extension_Method.TOINT(GetArg(data.MenuOpenConditionArgs, 1), 0);
				NShared.DungeonDifficultyType difficultyType = Vespa_String_Extension_Method.TOENUM(GetArg(data.MenuOpenConditionArgs, 2), NShared.DungeonDifficultyType.Easy);
				if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(num, num2, difficultyType))
				{
					oDisableMsg = null;
					return FunctionStateData.EFunctionState.Enabled;
				}
				oDisableMsg = LocalizationManager.GetString(NodeFunction.CURRENT_NODE_FUNCTION_LOCKED, num, num2);
				return FunctionStateData.EFunctionState.CurrentNodeFunctionLocked;
			}
			if (data.MenuOpenCondition == CampaignNodeData.EMenuOpenCondition.NotReady)
			{
				if (NGameAsset.Define.IsLiveBuild())
				{
					oDisableMsg = LocalizationManager.GetString(NodeFunction.NOT_READY);
					return FunctionStateData.EFunctionState.CurrentNodeFunctionLocked;
				}
				oDisableMsg = null;
				return FunctionStateData.EFunctionState.Enabled;
			}
			oDisableMsg = null;
			return FunctionStateData.EFunctionState.Enabled;
		}
		}
	}

	public static int GetHeroStar(int heroIndex, bool ignoreAccount = false)
	{
		if (!ignoreAccount && GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
			if (cachedHeroInfo != null)
			{
				return cachedHeroInfo.Star;
			}
		}
		return GameRoot.Instance.CreatureDataContainer.GetData(heroIndex)?.StartHeroStar ?? 0;
	}

	public static LevelInfo GetAccountTeamLevelInfo()
	{
		int teamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel;
		long curExp = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamExp;
		return GetTeamLevelInfo(teamLevel, curExp);
	}

	public static LevelInfo GetTeamLevelInfo(int curLevel, long curExp)
	{
		return LevelInfo.CreateTeamLevelInfo(curLevel, curExp);
	}

	public static LevelInfo GetHeroLevelInfo(HeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return LevelInfo.CreateHeroLevelInfo(0, 0L);
		}
		short level = heroInfo.Level;
		int exp = heroInfo.Exp;
		return GetHeroLevelInfo(level, exp);
	}

	public static LevelInfo GetHeroLevelInfo(int curLevel, int curExp)
	{
		return LevelInfo.CreateHeroLevelInfo(curLevel, curExp);
	}

	public static SkillBasicInfo GetSkillBasicInfo(HeroInfo heroInfo, int skillSlotIndex, int linkIndex = 0)
	{
		int tableIndex = 0;
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		if (data != null)
		{
			tableIndex = ((skillSlotIndex != 0) ? data.GetSkillIndexBySlot(skillSlotIndex, linkIndex) : data.BaseSkillIndex);
		}
		return CreateSkillBasicInfo(heroInfo, skillSlotIndex, tableIndex);
	}

	public static SkillBasicInfo[] GetSkillBasicInfos(HeroInfo heroInfo, int skillSlotIndex)
	{
		int num = 0;
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		List<SkillBasicInfo> list = new List<SkillBasicInfo>();
		if (data != null)
		{
			int num2 = __Extension_Method_ICollection__.COUNT(data.GetLinkedSkillIndexBySlot(skillSlotIndex)) + 1;
			for (int i = 0; i < num2; i++)
			{
				num = data.GetSkillIndexBySlot(skillSlotIndex, i);
				SkillBasicInfo item = CreateSkillBasicInfo(heroInfo, skillSlotIndex, num);
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	public static SkillBasicInfo CreateSkillBasicInfo(HeroInfo heroInfo, int skillSlotIndex, int tableIndex)
	{
		SkillBasicInfo result = default(SkillBasicInfo);
		if (heroInfo != null && skillSlotIndex > 0)
		{
			result.Level = heroInfo.GetSkillLevel(skillSlotIndex, isUserHero: true);
			result.IsLocked = result.Level == 0;
		}
		result.TableIndex = tableIndex;
		result.TableData = GameRoot.Instance.SkillDataContainer.GetData(result.TableIndex);
		return result;
	}

	public static SkillBookInfo GetSkillBookInfo(HeroInfo heroInfo, int skillSlotIndex)
	{
		SkillBookInfo result = default(SkillBookInfo);
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		if (data == null)
		{
			return result;
		}
		UpgradeBookItemData upgradeBookItemData = null;
		string text = data.TagType.ToString();
		if (!String_Extension_Method.IsNullOrEmpty(text))
		{
			result.CreatureTagType = text;
			upgradeBookItemData = GameRoot.Instance.UpgradeBookItemDataContainer.GetDataByTagTypeGrade(data.TagType, skillSlotIndex);
			if (upgradeBookItemData != null)
			{
				result.BookItemData = GameRoot.Instance.ItemDataContainer.GetDataByCode(upgradeBookItemData.ItemCode);
			}
			result.CurItemCount = ((result.BookItemData != null) ? GetItemCount(result.BookItemData.Index) : 0);
		}
		return result;
	}

	public static SkillDetailInfo GetSkillDetailInfo(HeroInfo heroInfo, int skillSlotIndex, int linkIndex = 0)
	{
		SkillDetailInfo result = new SkillDetailInfo
		{
			BasicInfo = GetSkillBasicInfo(heroInfo, skillSlotIndex, linkIndex)
		};
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		if (data.StartHeroStar == 0)
		{
			return result;
		}
		result.BookInfo = GetSkillBookInfo(heroInfo, skillSlotIndex);
		CreatureGradeData data2 = GameRoot.Instance.CreatureGradeDataContainer.GetData(skillSlotIndex - 1);
		SkillPriceKey key = new SkillPriceKey(skillSlotIndex, result.BasicInfo.Level);
		result.UpgradeGold = GameRoot.Instance.SkillPriceDataContainer.GetData(key)?.ReqGold ?? (-1);
		result.ReqHeroLevel = data2?.Level ?? 1;
		int skillExtend = heroInfo.GetSkillExtend(skillSlotIndex);
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxSkillExtend");
		result.ExtendInfos = new SkillExtendInfo[valueInt];
		for (int i = 0; i < valueInt; i++)
		{
			result.ExtendInfos[i].ExtendItemCount = GameRoot.Instance.ConstantDataContainer.GetValueInt("ReqItemCountSkillExtend" + (i + 1));
		}
		if (result.BasicInfo.IsLocked)
		{
			for (int j = 0; j < valueInt; j++)
			{
				result.ExtendInfos[j].ExtendStatus = SkillExtendInfo.EExtendStatus.NotLearnedCoreSkill;
			}
		}
		else
		{
			for (int k = 0; k < Mathf.Min(skillExtend, valueInt); k++)
			{
				result.ExtendInfos[k].ExtendStatus = SkillExtendInfo.EExtendStatus.AlreadyExtended;
			}
			int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("ReqHeroLevelSkillSlot" + skillSlotIndex);
			if (heroInfo.Level < valueInt2)
			{
				for (int l = skillExtend; l < valueInt; l++)
				{
					result.ExtendInfos[l].ExtendStatus = SkillExtendInfo.EExtendStatus.PrevExtendNeeded;
				}
			}
			else
			{
				if (skillExtend < valueInt)
				{
					result.ExtendInfos[skillExtend].ExtendStatus = ((SprkSkillBookTickets.Usable(result.BookInfo) < result.ExtendInfos[skillExtend].ExtendItemCount) ? SkillExtendInfo.EExtendStatus.NotEnoughItem : SkillExtendInfo.EExtendStatus.Extendable);
				}
				for (int m = skillExtend + 1; m < valueInt; m++)
				{
					result.ExtendInfos[m].ExtendStatus = SkillExtendInfo.EExtendStatus.PrevExtendNeeded;
				}
			}
		}
		int[] skillExtend2 = data.GetSkillExtend(skillSlotIndex);
		if (skillExtend2 != null)
		{
			for (int n = 0; n < Mathf.Min(valueInt, skillExtend2.Length); n++)
			{
				result.ExtendInfos[n].TableData = GameRoot.Instance.SkillDataContainer.GetData(skillExtend2[n]);
			}
		}
		return result;
	}

	public static bool CanSkillExtend(SkillDetailInfo skillInfo)
	{
		if (!RestoredPortal.HasEnhanceableSkill(skillInfo))
		{
			return false;
		}
		if (Array.FindIndex(skillInfo.ExtendInfos, (SkillExtendInfo info) => info.ExtendStatus == SkillExtendInfo.EExtendStatus.Extendable) != -1)
		{
			return true;
		}
		return false;
	}

	public static bool CanAnySkillExtend(HeroInfo heroInfo)
	{
		for (int i = 1; i <= 5; i++)
		{
			if (CanSkillExtend(GetSkillDetailInfo(heroInfo, i)))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetAccountItemCount(string itemCode)
	{
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
		if (dataByCode == null)
		{
			return 0;
		}
		return GetItemCount(dataByCode.Index);
	}

	public static int GetItemCount(int itemIndex)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(itemIndex)?.Count ?? 0;
	}

	public static void GetHeroExpForNextLevel(HeroInfo heroInfo, out long expForNextLevel)
	{
		expForNextLevel = 0L;
		if (heroInfo != null)
		{
			expForNextLevel = GameRoot.Instance.CreatureLevelDataContainer.GetData(heroInfo.Level)?.LocalExp ?? 0;
		}
	}

	public static int GetMaxHeroTotalExp()
	{
		if (_MaxHeroExp == 0)
		{
			int num = 1;
			while (true)
			{
				CreatureLevelData data = GameRoot.Instance.CreatureLevelDataContainer.GetData(num++);
				if (data == null)
				{
					break;
				}
				_MaxHeroExp = (int)data.TotalExp;
			}
		}
		return _MaxHeroExp;
	}

	public static void GetChangedHeroLevelExpInfo(int startHeroLevel, long changedTotalHeroExp, List<int> listNeededExps, out int finalHeroLevel)
	{
		finalHeroLevel = startHeroLevel;
		int num = startHeroLevel;
		while (true)
		{
			CreatureLevelData data = GameRoot.Instance.CreatureLevelDataContainer.GetData(num);
			CreatureLevelData data2 = GameRoot.Instance.CreatureLevelDataContainer.GetData(num + 1);
			if (data != null)
			{
				long totalExp = data.TotalExp;
				long num2 = data2?.TotalExp ?? 0;
				if (totalExp < changedTotalHeroExp)
				{
					finalHeroLevel = num;
					listNeededExps.Add((int)totalExp);
					listNeededExps.Add((int)num2);
					num++;
					continue;
				}
				break;
			}
			break;
		}
	}

	public static bool GameDataLoadedAndLogined()
	{
		if (!SingletonAuto<TableDataManager>.isValidInstance)
		{
			return false;
		}
		if (!SingletonAuto<TableDataManager>.instance.IsLoaded())
		{
			return false;
		}
		if (!AccountManager.IsLogin())
		{
			return false;
		}
		return true;
	}

	public static bool CanEquipRune(EquipItemData equipItemData, RuneItemData runeItemData)
	{
		if (equipItemData == null)
		{
			return false;
		}
		if (runeItemData == null)
		{
			return false;
		}
		if (!Array.Exists(runeItemData.PartTypes, (EquipPartType item) => item == equipItemData.PartType))
		{
			return false;
		}
		return true;
	}

	public static int GetChestExtendSize(int count, bool addBaseCount = true)
	{
		int num = 0;
		if (addBaseCount)
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("EquipCHESTMaxCount", 40);
			num += valueInt;
		}
		int chestExtend = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.ChestExtend;
		InventoryExtendData nextDataByExtendCount = GameRoot.Instance.InventoryExtendDataContainer.GetNextDataByExtendCount(chestExtend, InventoryType.Chest);
		if (nextDataByExtendCount == null)
		{
			return num;
		}
		int equipItemExtendSize = nextDataByExtendCount.EquipItemExtendSize;
		int num2 = count * equipItemExtendSize;
		return num + num2;
	}

	public static int GetInventoryExtendSize(int count, bool addBaseCount = true)
	{
		int num = 0;
		if (addBaseCount)
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("EquipItemMaxCount");
			num += valueInt;
		}
		int inventoryExtend = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.InventoryExtend;
		InventoryExtendData nextDataByExtendCount = GameRoot.Instance.InventoryExtendDataContainer.GetNextDataByExtendCount(inventoryExtend);
		if (nextDataByExtendCount == null)
		{
			return num;
		}
		int equipItemExtendSize = nextDataByExtendCount.EquipItemExtendSize;
		int num2 = count * equipItemExtendSize;
		return num + num2;
	}

	public static int GetPunishmentExtendSize(int count, bool addBaseCount = true)
	{
		int num = 0;
		if (addBaseCount)
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxPunishmentRuneItemCount", 100);
			num += valueInt;
		}
		int punishmentRuneStorageSlotExtend = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.PunishmentRuneStorageSlotExtend;
		InventoryExtendData nextDataByExtendCount = GameRoot.Instance.InventoryExtendDataContainer.GetNextDataByExtendCount(punishmentRuneStorageSlotExtend, InventoryType.PunishmentRune);
		if (nextDataByExtendCount == null)
		{
			return num;
		}
		int equipItemExtendSize = nextDataByExtendCount.EquipItemExtendSize;
		int num2 = count * equipItemExtendSize;
		return num + num2;
	}

	public static int GetCalcExtendPrice(InventoryType type, int count, int currExtendCount)
	{
		int num = 0;
		InventoryExtendData nextDataByExtendCount = GameRoot.Instance.InventoryExtendDataContainer.GetNextDataByExtendCount(currExtendCount, type);
		if (nextDataByExtendCount == null)
		{
			return num;
		}
		int equipItemExtendSize = nextDataByExtendCount.EquipItemExtendSize;
		int num2 = count * equipItemExtendSize;
		return num + num2;
	}

	public static bool CheckFixedSetHero(int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficulty, bool isScenario)
	{
		if (!GameRoot.isValidInstance)
		{
			return false;
		}
		DungeonHeroSetData data = GameRoot.Instance.DungeonHeroSetDataContainer.GetData(chapterIndex, dungeonIndex, difficulty);
		if (data == null)
		{
			return false;
		}
		if (data.IsScenario)
		{
			return isScenario;
		}
		return true;
	}

	public static HeroInfo GetDungeonHeroSetHeroInfo(int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficulty, int heroIndex, bool isScenario)
	{
		if (!CheckFixedSetHero(chapterIndex, dungeonIndex, difficulty, isScenario))
		{
			return null;
		}
		DungeonHeroSetData data = GameRoot.Instance.DungeonHeroSetDataContainer.GetData(chapterIndex, dungeonIndex, difficulty);
		int costumeIndex = GameRoot.Instance.DungeonHeroListSetDataContainer.GetData(data.HeroListSetDataIndex, heroIndex)?.CostumeIndex ?? 0;
		HeroInfo heroInfo = new HeroInfo();
		heroInfo.HeroIndex = heroIndex;
		heroInfo.CostumeIndex = costumeIndex;
		heroInfo.Level = (sbyte)data.CommonHeroLevel;
		heroInfo.Star = (sbyte)data.CommonHeroStar;
		heroInfo.Transcended = (sbyte)data.CommonHeroTranscended;
		CachedHeroInfo cachedHeroInfo = new CachedHeroInfo();
		cachedHeroInfo.HeroIndex = heroInfo.HeroIndex;
		cachedHeroInfo.CostumeIndex = heroInfo.CostumeIndex;
		cachedHeroInfo.Level = heroInfo.Level;
		cachedHeroInfo.Star = heroInfo.Star;
		cachedHeroInfo.Transcended = heroInfo.Transcended;
		cachedHeroInfo.SetSkillLevel(1, heroInfo.Level);
		cachedHeroInfo.SetSkillLevel(2, heroInfo.Level);
		cachedHeroInfo.SetSkillLevel(3, heroInfo.Level);
		cachedHeroInfo.SetSkillLevel(4, heroInfo.Level);
		cachedHeroInfo.SetSkillLevel(5, heroInfo.Level);
		if (data.CommonUniqueWeapon)
		{
			int num = 1000 + heroIndex;
			if (num != 0 && GameRoot.Instance.ItemDataContainer.GetData(num) != null)
			{
				EquipItemInfo equipItemInfo = new EquipItemInfo();
				equipItemInfo.ItemIndex = num;
				int partIndex = (equipItemInfo.SlotIndex = GameRoot.Instance.EquipSlotDataContainer.GetEquipSlotIndexFor(EquipPartType.Weapon));
				cachedHeroInfo.SetEquipItemInfo(partIndex, equipItemInfo);
			}
		}
		cachedHeroInfo.Validate();
		return cachedHeroInfo;
	}

	private static ForcedHeroInfo GetForcedHeroInfo(int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficulty, int heroIndex, bool isScenario)
	{
		if (!CheckFixedSetHero(chapterIndex, dungeonIndex, difficulty, isScenario))
		{
			return null;
		}
		HeroInfo dungeonHeroSetHeroInfo = GetDungeonHeroSetHeroInfo(chapterIndex, dungeonIndex, difficulty, heroIndex, isScenario);
		if (dungeonHeroSetHeroInfo == null)
		{
			return null;
		}
		DungeonHeroSetData data = GameRoot.Instance.DungeonHeroSetDataContainer.GetData(chapterIndex, dungeonIndex, difficulty);
		ForcedHeroInfo forcedHeroInfo = new ForcedHeroInfo();
		forcedHeroInfo.CachedHeroInfo = dungeonHeroSetHeroInfo as CachedHeroInfo;
		forcedHeroInfo.HeroTier = data.CommonHeroTier;
		forcedHeroInfo.HeroScaleR = 1000;
		if (data.CommonUniqueWeapon)
		{
			forcedHeroInfo.WeaponIndex = 1000 + heroIndex;
		}
		forcedHeroInfo.AddExtraSkills = data.CommonHeroAddExtraSkill;
		return forcedHeroInfo;
	}

	public static List<BattleCreatureCreateInfo> GetDungeonHeroSetBattleCreatureInfos(EnterDungeonData enterDungeon, int[] heroIndices)
	{
		List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroIndices); i++)
		{
			int num = heroIndices[i];
			if (num == 0)
			{
				continue;
			}
			ForcedHeroInfo forcedHeroInfo = GetForcedHeroInfo(enterDungeon.Chapter, enterDungeon.LinkedDungeon, enterDungeon.Difficulty, num, enterDungeon.IsScenario);
			if (forcedHeroInfo != null)
			{
				BattleCreatureCreateInfo battleCreatureCreateInfo = SubStoryUtillity.CreateBattleCreatureCreateInfo(forcedHeroInfo.CachedHeroInfo, forcedHeroInfo.HeroScaleR, forcedHeroInfo.HeroTier, forcedHeroInfo.WeaponIndex, forcedHeroInfo.AddExtraSkills, 1000, i == 0);
				if (battleCreatureCreateInfo != null)
				{
					list.Add(battleCreatureCreateInfo);
				}
			}
		}
		return list;
	}

	public static SkillBasicInfo GetSoulWeaponBasicSkillInfo(int heroIndex, SoulWeaponInfo soulWeaponInfo)
	{
		int num = 0;
		int slotNo = 10;
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data != null)
		{
			num = data.GetSkillIndexBySlot(slotNo);
		}
		SkillBasicInfo result = default(SkillBasicInfo);
		SoulWeaponSkillData dataByBasicSkillIndex = GameRoot.Instance.SoulWeaponSkillDataContainer.GetDataByBasicSkillIndex(num);
		if (dataByBasicSkillIndex != null)
		{
			result.StackTypeName = dataByBasicSkillIndex.StackTypeName;
			result.ItemCode = dataByBasicSkillIndex.ItemCode;
		}
		result.TableIndex = num;
		result.TableData = GameRoot.Instance.SkillDataContainer.GetData(result.TableIndex);
		result.Level = 1;
		return result;
	}

	public static bool CheckRestriction(string constantTableKey, bool cacheResult = false)
	{
		string[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>(constantTableKey);
		if (valueArray == null)
		{
			return true;
		}
		if (!cacheResult)
		{
			return SingletonAuto<ConditionChecker>.instance.Check(valueArray);
		}
		if (_restrictionCache.ContainsKey(constantTableKey))
		{
			return _restrictionCache[constantTableKey];
		}
		_restrictionCache[constantTableKey] = SingletonAuto<ConditionChecker>.instance.Check(valueArray);
		return _restrictionCache[constantTableKey];
	}
}
