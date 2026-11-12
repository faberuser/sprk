using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NShared.NWorldMap.NEventSubData;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NBattleContext;

public class EnterDungeonData
{
	private int _chapter;

	private int _originalDungeon;

	private int _linkedDungeon;

	private CampaignChapterData _chapterDataCache;

	private CampaignNodeData _originalNodeDataCache;

	private CampaignNodeData _linkedNodeDataCache;

	private CampaignDungeonData _originalDungeonDataCache;

	private CampaignDungeonData _linkedDungeonDataCache;

	public int Chapter
	{
		get
		{
			return _chapter;
		}
		private set
		{
			_chapter = value;
			ClearCache();
		}
	}

	public int OriginalDungeon
	{
		get
		{
			return _originalDungeon;
		}
		private set
		{
			_originalDungeon = value;
			ClearCache();
		}
	}

	public int LinkedDungeon
	{
		get
		{
			return _linkedDungeon;
		}
		private set
		{
			_linkedDungeon = value;
			ClearCache();
		}
	}

	public bool IsScenario { get; private set; }

	public NShared.DungeonDifficultyType Difficulty { get; set; }

	public bool ShowDuplicated { get; set; }

	public ResultCreatureInfo[] InitialHeroInfos { get; set; }

	public string BattleStartConfirmMessage { get; set; }

	public bool IsScenarioReplay
	{
		get
		{
			if (!IsScenario)
			{
				return false;
			}
			if (!AccountManager.IsLogined())
			{
				return false;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(Chapter, OriginalDungeon);
		}
	}

	public CampaignChapterData ChapterData
	{
		get
		{
			if (_chapterDataCache == null)
			{
				if (!GameRoot.isValidInstance)
				{
					return null;
				}
				_chapterDataCache = GameRoot.Instance.CampaignChapterDataContainer.GetData(Chapter);
			}
			return _chapterDataCache;
		}
	}

	public CampaignNodeData OriginalNodeData
	{
		get
		{
			if (_originalNodeDataCache == null)
			{
				if (!GameRoot.isValidInstance)
				{
					return null;
				}
				_originalNodeDataCache = GameRoot.Instance.CampaignNodeDataContainer.GetData(Chapter, OriginalDungeon);
			}
			return _originalNodeDataCache;
		}
	}

	public CampaignNodeData LinkedNodeData
	{
		get
		{
			if (_linkedNodeDataCache == null)
			{
				if (!GameRoot.isValidInstance)
				{
					return null;
				}
				_linkedNodeDataCache = GameRoot.Instance.CampaignNodeDataContainer.GetData(Chapter, LinkedDungeon);
			}
			return _linkedNodeDataCache;
		}
	}

	public CampaignDungeonData OriginalDungeonData
	{
		get
		{
			if (_originalDungeonDataCache == null)
			{
				if (!GameRoot.isValidInstance)
				{
					return null;
				}
				_originalDungeonDataCache = GameRoot.Instance.CampaignDungeonDataContainer.GetData(Chapter, OriginalDungeon);
			}
			return _originalDungeonDataCache;
		}
	}

	public CampaignDungeonData LinkedDungeonData
	{
		get
		{
			if (_linkedDungeonDataCache == null)
			{
				if (!GameRoot.isValidInstance)
				{
					return null;
				}
				_linkedDungeonDataCache = GameRoot.Instance.CampaignDungeonDataContainer.GetData(Chapter, LinkedDungeon);
			}
			return _linkedDungeonDataCache;
		}
	}

	public QuestDungeon QuestDungeon
	{
		get
		{
			if (!AccountManager.IsLogined())
			{
				return null;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.GetEventData<QuestDungeon>(Chapter, LinkedDungeon);
		}
	}

	public int CreatureExpGain
	{
		get
		{
			if (LinkedDungeonData != null)
			{
				return LinkedDungeonData.GetCreatureExpOfDifficulty(Difficulty);
			}
			return 0;
		}
	}

	public int DungeonRewardIndex
	{
		get
		{
			if (LinkedDungeonData != null)
			{
				return LinkedDungeonData.GetRewardIndexOfDifficulty(Difficulty);
			}
			return 0;
		}
	}

	public EnterDungeonData()
	{
		Chapter = 0;
		OriginalDungeon = 0;
		LinkedDungeon = 0;
		Difficulty = NShared.DungeonDifficultyType.Easy;
		IsScenario = false;
	}

	public EnterDungeonData(int chapter, int originalDungeon, int linkedDungeon)
	{
		NShared.DungeonDifficultyType difficulty = (NShared.DungeonDifficultyType)Helper.GetDifficulty(chapter, originalDungeon);
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapter, originalDungeon);
		Init(chapter, originalDungeon, linkedDungeon, difficulty, GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanPlayScenario(chapter, originalDungeon) && !flag);
	}

	public EnterDungeonData(int chapter, int originalDungeon, int linkedDungeon, NShared.DungeonDifficultyType difficulty, bool isScenario = false)
	{
		Init(chapter, originalDungeon, linkedDungeon, difficulty, isScenario);
	}

	private void Init(int chapter, int originalDungeon, int linkedDungeon, NShared.DungeonDifficultyType difficulty, bool isScenario = false)
	{
		Chapter = chapter;
		OriginalDungeon = originalDungeon;
		LinkedDungeon = linkedDungeon;
		Difficulty = difficulty;
		IsScenario = isScenario;
		Verify();
	}

	public int GetFirstClearGemCount()
	{
		CampaignDungeonData originalDungeonData = OriginalDungeonData;
		if (originalDungeonData == null || GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.FirstClearRewarded(Chapter, OriginalDungeon, Difficulty))
		{
			return 0;
		}
		return Difficulty switch
		{
			NShared.DungeonDifficultyType.Easy => originalDungeonData.FirstClearGem_Easy, 
			NShared.DungeonDifficultyType.Normal => originalDungeonData.FirstClearGem_Normal, 
			NShared.DungeonDifficultyType.Hard => originalDungeonData.FirstClearGem_Hard, 
			NShared.DungeonDifficultyType.Hell => originalDungeonData.FirstClearGem_Hell, 
			_ => 0, 
		};
	}

	public int GetFirstRewardIndex()
	{
		CampaignDungeonData originalDungeonData = OriginalDungeonData;
		if (originalDungeonData == null || GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.FirstClearRewarded(Chapter, OriginalDungeon, Difficulty))
		{
			return 0;
		}
		if (originalDungeonData == null)
		{
			return 0;
		}
		return Difficulty switch
		{
			NShared.DungeonDifficultyType.Easy => originalDungeonData.FirstClearRewardIndex_Easy, 
			NShared.DungeonDifficultyType.Normal => originalDungeonData.FirstClearRewardIndex_Normal, 
			NShared.DungeonDifficultyType.Hard => originalDungeonData.FirstClearRewardIndex_Hard, 
			NShared.DungeonDifficultyType.Hell => originalDungeonData.FirstClearRewardIndex_Hell, 
			_ => 0, 
		};
	}

	private void Verify()
	{
		if (LinkedDungeon == 0)
		{
			LinkedDungeon = OriginalDungeon;
		}
		if (!GameRoot.isValidInstance)
		{
			return;
		}
		if (GameRoot.Instance.CampaignChapterDataContainer.GetData(Chapter) == null)
		{
			VespaDebug.LogError("Chapter data not found : " + Chapter);
			Reset();
			return;
		}
		if (GameRoot.Instance.CampaignDungeonDataContainer.GetData(Chapter, LinkedDungeon) == null)
		{
			if (GameRoot.Instance.CampaignDungeonDataContainer.GetData(Chapter, OriginalDungeon) == null)
			{
				Reset();
				return;
			}
			LinkedDungeon = OriginalDungeon;
		}
		if (IsScenario && (!GameRoot.isValidInstance || !GameRoot.Instance.CampaignDungeonDataContainer.IsScenarioDungeon(Chapter, LinkedDungeon)))
		{
			IsScenario = false;
		}
	}

	private void Reset()
	{
		Chapter = 0;
		OriginalDungeon = 0;
		LinkedDungeon = 0;
		Difficulty = NShared.DungeonDifficultyType.Easy;
		IsScenario = false;
		ShowDuplicated = false;
	}

	private void ClearCache()
	{
		_chapterDataCache = null;
		_originalNodeDataCache = null;
		_linkedNodeDataCache = null;
		_originalDungeonDataCache = null;
		_linkedDungeonDataCache = null;
	}

	public bool CheckBattleEnter()
	{
		int staminaCount = ((LinkedDungeonData != null) ? LinkedDungeonData.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), Difficulty) : 0);
		return CheckBattleEnter(staminaCount);
	}

	public bool CheckBattleEnter(int staminaCount)
	{
		int num = 0;
		long maxTryCount = num;
		int num2 = 0;
		long remainCount = num2;
		CampaignDungeonData linkedDungeonData = LinkedDungeonData;
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CheckEventDungeon(Chapter, LinkedDungeon, out maxTryCount, out remainCount))
		{
			long num3 = remainCount;
			int num4 = 0;
			if (num3 <= num4)
			{
				int result = -1;
				List<DropBonusEvent> allEffectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent(EventBonusType.EventDungeon);
				if (allEffectiveEvent != null)
				{
					for (int i = 0; i < allEffectiveEvent.Count; i++)
					{
						DropBonusEvent dropBonusEvent = allEffectiveEvent[i];
						if (!String_Extension_Method.IsNullOrEmpty(dropBonusEvent.Condition))
						{
							int.TryParse(dropBonusEvent.Condition, out result);
							break;
						}
					}
				}
				if (result == Chapter)
				{
					EventDungeonManagement.OpenConfirmResetEventDungeon(null, Chapter, LinkedDungeon);
				}
				else
				{
					LobbyCampaignDungeon.OpenConfirmResetEventDungeon(linkedDungeonData.ChapterIndex, linkedDungeonData.DungeonIndex, null);
				}
				return false;
			}
		}
		else
		{
			if (IsScenarioReplay)
			{
				return true;
			}
			if (linkedDungeonData != null)
			{
				return UIHelper.CheckStamina(linkedDungeonData.ReqStaminaType, staminaCount, linkedDungeonData.ChapterIndex, null);
			}
		}
		return true;
	}

	public bool CheckIsNotInitDailyCount()
	{
		if (LinkedDungeonData.IsNotInItDailyCount)
		{
			MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), LocalizationManager.GetStringDirect("MEMORY_DUNGEON_EXCESS_LIMIT"));
			return true;
		}
		return false;
	}
}
