using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NContents;

public class ContentsOpenChecker : SingletonAuto<ContentsOpenChecker>
{
	private class OpenContentsContainer
	{
		private List<CampaignNodeData> _checkNodeOpenList = new List<CampaignNodeData>();

		private readonly HashSet<ERightMenuButton> _lockedMenus = new HashSet<ERightMenuButton>();

		private List<CampaignChapterData> _checkHellModeOpenList = new List<CampaignChapterData>();

		public void Init()
		{
			foreach (CampaignNodeData item in GameRoot.Instance.CampaignNodeDataContainer.GetData_by_MenuOpenCondition(CampaignNodeData.EMenuOpenCondition.ClearDungeon))
			{
				int chapterIndex = Vespa_String_Extension_Method.TOINT(Helper.GetArg(item.MenuOpenConditionArgs, 0), 0);
				int dungeonIndex = Vespa_String_Extension_Method.TOINT(Helper.GetArg(item.MenuOpenConditionArgs, 1), 0);
				DungeonDifficultyType difficultyType = Vespa_String_Extension_Method.TOENUM(Helper.GetArg(item.MenuOpenConditionArgs, 2), DungeonDifficultyType.Easy);
				if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex, difficultyType))
				{
					_checkNodeOpenList.Add(item);
				}
			}
			foreach (CampaignChapterData allDatum in GameRoot.Instance.CampaignChapterDataContainer.GetAllData())
			{
				if (allDatum.HellMode_ReqChapterIndex > 0 && allDatum.HellMode_ReqDungeonIndex > 0 && !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(allDatum.HellMode_ReqChapterIndex, allDatum.HellMode_ReqDungeonIndex, allDatum.HellMode_ReqDungeonDifficulty))
				{
					_checkHellModeOpenList.Add(allDatum);
				}
			}
			InitLockedMenu();
		}

		public bool IsContentsOpenNode(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
		{
			foreach (CampaignNodeData checkNodeOpen in _checkNodeOpenList)
			{
				int num = Vespa_String_Extension_Method.TOINT(Helper.GetArg(checkNodeOpen.MenuOpenConditionArgs, 0), 0);
				int num2 = Vespa_String_Extension_Method.TOINT(Helper.GetArg(checkNodeOpen.MenuOpenConditionArgs, 1), 0);
				DungeonDifficultyType dungeonDifficultyType = Vespa_String_Extension_Method.TOENUM(Helper.GetArg(checkNodeOpen.MenuOpenConditionArgs, 2), DungeonDifficultyType.Easy);
				if (chapterIndex == num && dungeonIndex == num2 && difficulty >= dungeonDifficultyType)
				{
					return true;
				}
			}
			return false;
		}

		public CampaignNodeData CheckContentsOpen()
		{
			CampaignNodeData campaignNodeData = null;
			foreach (CampaignNodeData checkNodeOpen in _checkNodeOpenList)
			{
				int chapterIndex = Vespa_String_Extension_Method.TOINT(Helper.GetArg(checkNodeOpen.MenuOpenConditionArgs, 0), 0);
				int dungeonIndex = Vespa_String_Extension_Method.TOINT(Helper.GetArg(checkNodeOpen.MenuOpenConditionArgs, 1), 0);
				DungeonDifficultyType difficultyType = Vespa_String_Extension_Method.TOENUM(Helper.GetArg(checkNodeOpen.MenuOpenConditionArgs, 2), DungeonDifficultyType.Easy);
				if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex, difficultyType))
				{
					campaignNodeData = checkNodeOpen;
					break;
				}
			}
			if (campaignNodeData != null)
			{
				_checkNodeOpenList.Remove(campaignNodeData);
				return campaignNodeData;
			}
			return null;
		}

		public List<CampaignChapterData> CheckHellModeOpen()
		{
			List<CampaignChapterData> list = null;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_checkHellModeOpenList); i++)
			{
				CampaignChapterData campaignChapterData = _checkHellModeOpenList[i];
				if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(campaignChapterData.HellMode_ReqChapterIndex, campaignChapterData.HellMode_ReqDungeonIndex, campaignChapterData.HellMode_ReqDungeonDifficulty))
				{
					if (list == null)
					{
						list = new List<CampaignChapterData>();
					}
					list.Add(campaignChapterData);
					_checkHellModeOpenList.RemoveAt(i);
					i--;
				}
			}
			return list;
		}

		private void InitLockedMenu()
		{
			EnumLookup<ERightMenuButton>.ValueData[] valueArray = EnumLookup<ERightMenuButton>.ValueArray;
			for (int i = 0; i < valueArray.Length; i++)
			{
				ERightMenuButton enumValue = valueArray[i].EnumValue;
				if (!SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(enumValue))
				{
					_lockedMenus.Add(enumValue);
				}
			}
		}

		public List<ERightMenuButton> CheckMenusOpen()
		{
			List<ERightMenuButton> list = null;
			foreach (ERightMenuButton lockedMenu in _lockedMenus)
			{
				if (SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(lockedMenu))
				{
					list = list ?? new List<ERightMenuButton>();
					list.Add(lockedMenu);
				}
			}
			if (list != null)
			{
				foreach (ERightMenuButton item in list)
				{
					_lockedMenus.Remove(item);
				}
			}
			return list;
		}
	}

	private OpenContentsContainer nodeContainerInternal;

	private OpenContentsContainer nodeContainer
	{
		get
		{
			if (nodeContainerInternal == null)
			{
				nodeContainerInternal = new OpenContentsContainer();
				nodeContainerInternal.Init();
			}
			return nodeContainerInternal;
		}
	}

	public bool IsContentsOpenNode(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		return nodeContainer.IsContentsOpenNode(chapterIndex, dungeonIndex, difficulty);
	}

	public CampaignNodeData CheckContentsOpen()
	{
		return nodeContainer.CheckContentsOpen();
	}

	public List<CampaignChapterData> CheckHellModeOpen()
	{
		return nodeContainer.CheckHellModeOpen();
	}

	public List<ERightMenuButton> CheckMenusOpen()
	{
		return nodeContainer.CheckMenusOpen();
	}

	public bool CheckContentsOpen(ContentsType contentsType)
	{
		if (contentsType == ContentsType.None)
		{
			return true;
		}
		ContentsOpenConditionData dataByContentsType = GameRoot.Instance.ContentsOpenConditionDataContainer.GetDataByContentsType(contentsType);
		if (dataByContentsType == null)
		{
			return true;
		}
		if (!String_Extension_Method.IsNullOrEmpty(dataByContentsType.EntryMenuCondition))
		{
			return NGame2.NUI.NWindow.EntryMenu.CheckEntryMenuCondition(null, dataByContentsType.EntryMenuCondition);
		}
		if (dataByContentsType.OpenConditionSubQuest <= 0)
		{
			return true;
		}
		return CheckClearSubQuest(dataByContentsType.OpenConditionSubQuest);
	}

	public bool CheckNodeOpen(int chapterIndex, int dungeonIndex)
	{
		int subQuestIndexByNode = GameRoot.Instance.ContentsOpenConditionDataContainer.GetSubQuestIndexByNode(chapterIndex, dungeonIndex);
		if (subQuestIndexByNode == 0)
		{
			return true;
		}
		return CheckClearSubQuest(subQuestIndexByNode);
	}

	private bool CheckClearSubQuest(int index)
	{
		if (GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(index) == null)
		{
			return true;
		}
		if (!AccountManager.IsLogined())
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.QuestManager == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestInfoByQuestIndex(index) == null)
		{
			return false;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.QuestManager.IsGetRewardableSubQuest(index))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.QuestManager.IsGetCompleteSubQuest(index);
		}
		return true;
	}
}
