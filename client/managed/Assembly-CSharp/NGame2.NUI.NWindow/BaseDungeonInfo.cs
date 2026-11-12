using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NDispatchBattle;
using NGame2.NParty;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPortrait;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBooster;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class BaseDungeonInfo : LobbyFullWindow, IBoosterListObserver, IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>, IUserObserver
{
	[Serializable]
	public class DefaultInfoComponents
	{
		public UILabel Label_Name;

		public DungeonDifficultyComponent DungeonDifficultyComponent;

		public GameObject MonsterRoot;

		public UIScrollGrid Grid_Monsters;

		private List<Component> _monsterComponents = new List<Component>();

		public UIWidget Widget_FirstGem;

		public UILabel Label_firstGem;

		public GameObject FirstClearRewardRoot;

		public UIScrollGrid Grid_FirstClearReward;

		private List<Component> _firstClearRewardComponents = new List<Component>();

		public GameObject RewardExpRoot;

		public UILabel Label_RewardExp;

		public GameObject RewardGoldRoot;

		public UIWidget Widget_RewardGoldMinMax;

		public UILabel Label_GoldMin;

		public UILabel Label_GoldMax;

		public UILabel Label_NoGold;

		public UIWidget Widget_ScenarioReplay;

		public GameObject RemainTimeRoot;

		public UILabel Label_RemainTime;

		public GameObject EventInfoRoot;

		public UILabel Label_EventRemainCount;

		public UILabel Label_EventMaxCount;

		public UIButton Button_EventReset;

		public UIButton Button_PartyPlay;

		public UIButton ButtonOnlyParyPlay;

		public UIButton Button_BattleReady;

		public UIButton Button_MoveEnter;

		public UIButton ButtonDispatch;

		public UIButton ButtonDispatchFinished;

		public void AddMonsterComponent(Component component)
		{
			if (!(component == null))
			{
				_monsterComponents.Add(component);
			}
		}

		public void AddMonsterComponents(List<Component> components)
		{
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(components))
			{
				_monsterComponents.AddRange(components);
			}
		}

		public void ReleaseMonsterComponents()
		{
			FactoryBase.ReleaseAndClearList(_monsterComponents);
		}

		public void AddFirstRewardComponent(Component component)
		{
			if (!(component == null))
			{
				_firstClearRewardComponents.Add(component);
			}
		}

		public void ReleaseFirstRewardComponents()
		{
			FactoryBase.ReleaseAndClearList(_firstClearRewardComponents);
		}

		public void Reset()
		{
			UIHelper.EnableGameObject(MonsterRoot, enable: false);
			UIHelper.EnableGameObject(RewardGoldRoot, enable: false);
			UIHelper.EnableGameObject(RewardGoldRoot, enable: false);
			UIHelper.EnableButton(Button_PartyPlay, enable: true, grayscaleIfDisable: true);
			ReleaseMonsterComponents();
			ReleaseFirstRewardComponents();
		}
	}

	private EnterDungeonData _enterDungeonCache;

	public float CheckUpdateTimeTick = 0.5f;

	private float _lastcheckTime;

	protected RewardData _customRewardData;

	protected bool _showHeroReward;

	protected bool _presetted;

	public DefaultInfoComponents InfoComponents;

	public EnterDungeonData EnterDungeon
	{
		get
		{
			if (_enterDungeonCache == null)
			{
				_enterDungeonCache = new EnterDungeonData();
			}
			return _enterDungeonCache;
		}
	}

	public void Open<T>(EnterDungeonData enterDungeonData, T listener, bool notCurrentNode = false)
	{
		SetupEnterDungeonData(enterDungeonData);
		SetupDiffcultyComponent();
		SetupBattleReadyButton(notCurrentNode);
		Open(delegate
		{
			OnOpen(enterDungeonData, listener, notCurrentNode);
			Refresh();
		}, null);
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.ObserverManager.AddObserver(this);
		}
	}

	public virtual void OnOpen<T>(EnterDungeonData enterBattle, T listener, bool notCurrentNode = false)
	{
	}

	public virtual void AutoNextBattle()
	{
	}

	public virtual void Refresh()
	{
		SetupDungeonName(GetDungeonName());
		SetupRewardExp();
		SetupRewardGold();
		SetupFirstClearGem();
		SetupFirstClearReward();
		SetupEventComponents();
		SetupPartyPlayButton();
		SetupMonsterInfos();
		UpdateButtonDispatch();
		GlobalAutoBehaviour<BoosterManager>.instance.AddObserver(this);
	}

	public override void OnClosing()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.ObserverManager.RemoveObserver(this);
		}
		base.OnClosing();
	}

	public override void Reset()
	{
		base.Reset();
		GlobalAutoBehaviour<BoosterManager>.instance.RemoveObserver(this);
		if (_presetted)
		{
			_presetted = false;
			return;
		}
		_customRewardData = null;
		_showHeroReward = false;
	}

	public virtual DungeonDifficultyComponent GetDungeonDiffcultyComp()
	{
		return InfoComponents.DungeonDifficultyComponent;
	}

	public virtual RewardData GetRewardData()
	{
		if (_customRewardData != null)
		{
			return _customRewardData;
		}
		int dungeonRewardIndex = EnterDungeon.DungeonRewardIndex;
		return GameRoot.Instance.RewardDataContainer.GetData(dungeonRewardIndex);
	}

	public virtual void PresetShowHeroReward(bool show)
	{
		_showHeroReward = show;
		_presetted = true;
	}

	public virtual void PresetCustomRewardData(RewardData rewardData)
	{
		_customRewardData = rewardData;
		_presetted = true;
	}

	public virtual int GetRewardExp()
	{
		return EnterDungeon.CreatureExpGain;
	}

	public virtual List<MonsterHeroInfo> GetDungeonMonsterInfos()
	{
		return GetAllMonsterInfos();
	}

	public virtual string GetDungeonName()
	{
		CampaignNodeData linkedNodeData = EnterDungeon.LinkedNodeData;
		if (linkedNodeData == null)
		{
			CampaignDungeonData linkedDungeonData = EnterDungeon.LinkedDungeonData;
			if (linkedDungeonData == null)
			{
				return null;
			}
			EventDungeonData data = GameRoot.Instance.EventDungeonDataContainer.GetData(linkedDungeonData.ChapterIndex, linkedDungeonData.DungeonIndex);
			if (data != null)
			{
				return LocalizationManager.GetStringDirect(data.DungeonName);
			}
		}
		else if (!String_Extension_Method.IsNullOrEmpty(linkedNodeData.PortalNameCode))
		{
			return LocalizationManager.GetStringDirect(linkedNodeData.PortalNameCode);
		}
		return linkedNodeData?.Name;
	}

	public bool IsOnlyMultiDungeon()
	{
		Helper.FunctionStateData functionState = Helper.GetFunctionState(EnterDungeon.Chapter, EnterDungeon.LinkedDungeon, containsDisableText: true);
		if (functionState == null)
		{
			return false;
		}
		return functionState.Type == NShared.NodeFunctionType.MultiBattle;
	}

	public bool CanEnterPartyPlay()
	{
		if (GlobalAutoBehaviour<PartyAutoSearchManager>.instance.DoAutoSearch)
		{
			string stringDirect = LocalizationManager.GetStringDirect("PARTY_SEARCH");
			string stringDirect2 = LocalizationManager.GetStringDirect("PARTY_SEARCH_CANCEL_WARNING");
			MessagePopup3Button.Open(stringDirect, stringDirect2, delegate
			{
				GlobalAutoBehaviour<PartyAutoSearchManager>.instance.CloseSearch();
			}, null, NShared.NLocalization.Common.COMMON_CONFIRM, NShared.NLocalization.Common.COMMON_CANCEL);
			return false;
		}
		return true;
	}

	public List<MonsterHeroInfo> GetBossMonsterInfos()
	{
		List<MonsterHeroInfo> allMonsterInfos = GetAllMonsterInfos();
		for (int num = __Extension_Method_ICollection__.COUNT(allMonsterInfos) - 1; num >= 0; num--)
		{
			if (!allMonsterInfos[num].IsBoss)
			{
				allMonsterInfos.RemoveAt(num);
			}
		}
		return allMonsterInfos;
	}

	public MonsterHeroInfo GetBossMonsterInfo()
	{
		MonsterHeroInfo result = null;
		List<MonsterHeroInfo> allMonsterInfos = GetAllMonsterInfos();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(allMonsterInfos))
		{
			result = allMonsterInfos[0];
		}
		return result;
	}

	public List<MonsterHeroInfo> GetNormalMonsterInfos()
	{
		List<MonsterHeroInfo> allMonsterInfos = GetAllMonsterInfos();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(allMonsterInfos))
		{
			allMonsterInfos.RemoveAt(0);
		}
		return allMonsterInfos;
	}

	public List<MonsterHeroInfo> GetAllMonsterInfos()
	{
		return UIHelper.GetMonsterInfoFromDungeon(EnterDungeon);
	}

	public void SetupDiffcultyComponent()
	{
		DungeonDifficultyComponent dungeonDiffcultyComp = GetDungeonDiffcultyComp();
		if (!(dungeonDiffcultyComp == null))
		{
			if (EnterDungeon.LinkedDungeonData.NoDifficulty)
			{
				UIHelper.EnableGameObject(dungeonDiffcultyComp, enable: false);
				return;
			}
			UIHelper.EnableGameObject(dungeonDiffcultyComp, enable: true);
			dungeonDiffcultyComp.Init(EnterDungeon.Chapter, EnterDungeon.OriginalDungeon);
		}
	}

	public void SetupEnterDungeonData(EnterDungeonData enterDungeonData)
	{
		_enterDungeonCache = enterDungeonData;
	}

	public void SetupDungeonName(string dungeonName)
	{
		UIHelper.SetLabelText(InfoComponents.Label_Name, dungeonName);
	}

	public void SetupFirstClearGem()
	{
		if (!(InfoComponents.Widget_FirstGem == null))
		{
			int firstClearGemCount = EnterDungeon.GetFirstClearGemCount();
			UIHelper.EnableGameObject(InfoComponents.Widget_FirstGem, firstClearGemCount > 0);
			if (firstClearGemCount > 0)
			{
				UIHelper.SetLabelText(InfoComponents.Label_firstGem, "X" + firstClearGemCount);
			}
		}
	}

	public void SetupFirstClearReward()
	{
		if (InfoComponents.FirstClearRewardRoot == null)
		{
			return;
		}
		InfoComponents.ReleaseFirstRewardComponents();
		int firstRewardIndex = EnterDungeon.GetFirstRewardIndex();
		UIHelper.EnableGameObject(InfoComponents.FirstClearRewardRoot, firstRewardIndex > 0);
		if (firstRewardIndex <= 0)
		{
			return;
		}
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(firstRewardIndex);
		if (data == null)
		{
			return;
		}
		int GroupCount;
		foreach (RewardElementData item in data.GetRewardItemListWithGroupCount(out GroupCount))
		{
			if (data == null)
			{
				continue;
			}
			CurrencyType currencyType = item.GetCurrencyType();
			if (currencyType != CurrencyType.None)
			{
				CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(InfoComponents.Grid_FirstClearReward, CurrencyIconComponent.PrefabAuto3);
				int itemMin = item.ItemMin;
				CurrencyIconComponent.Listener listener = null;
				int num = itemMin;
				currencyIconComponent.Open(currencyType, num, listener);
				InfoComponents.AddFirstRewardComponent(currencyIconComponent);
				continue;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item.ItemCode);
			if (dataByCode != null)
			{
				int itemMin2 = item.ItemMin;
				int itemMax = item.ItemMax;
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(InfoComponents.Grid_FirstClearReward, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(dataByCode);
				if (dataByCode.Type != ItemType.Equip)
				{
					itemIconComponent.LargeSetItemMinMax(itemMin2, itemMax);
				}
				InfoComponents.AddFirstRewardComponent(itemIconComponent);
			}
		}
		if (InfoComponents.Grid_FirstClearReward != null)
		{
			UIHelper.RepositionGrid(InfoComponents.Grid_FirstClearReward);
		}
	}

	public void SetupEventComponents()
	{
		int num = 0;
		long maxTryCount = num;
		int num2 = 0;
		long remainCount = num2;
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CheckEventDungeon(EnterDungeon.Chapter, EnterDungeon.LinkedDungeon, out maxTryCount, out remainCount);
		UIHelper.EnableGameObject(InfoComponents.EventInfoRoot, flag);
		if (flag)
		{
			UIHelper.SetLabelText(InfoComponents.Label_EventMaxCount, maxTryCount);
			UIHelper.SetLabelText(InfoComponents.Label_EventRemainCount, remainCount);
			UIHelper.EnableGameObject(InfoComponents.Button_EventReset, remainCount == 0L);
			UIHelper.EnableGameObject(InfoComponents.Button_BattleReady, remainCount != 0L);
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (CheckUpdateTick())
		{
			int dungeonRemainTime = GetDungeonRemainTime();
			long num = dungeonRemainTime;
			GameObject remainTimeRoot = InfoComponents.RemainTimeRoot;
			int num2 = -1;
			UIHelper.EnableGameObject(remainTimeRoot, num != num2);
			string empty = string.Empty;
			UIHelper.SetLabelText(text: (num != 0L) ? UIHelper.GetRemainTimeLabelText(num) : LocalizationManager.GetStringDirect("CONTENTS_REMAIN_TIME_MINUTE_SECOND", "--", "--"), label: InfoComponents.Label_RemainTime);
		}
	}

	protected virtual int GetDungeonRemainTime()
	{
		return -1;
	}

	public bool CheckUpdateTick()
	{
		if (Time.time - _lastcheckTime > CheckUpdateTimeTick)
		{
			_lastcheckTime = Time.time;
			return true;
		}
		return false;
	}

	public void SetupRewardGold()
	{
		bool flag = !EnterDungeon.IsScenarioReplay;
		UIHelper.EnableGameObject(InfoComponents.RewardGoldRoot, flag);
		UIHelper.EnableGameObject(InfoComponents.Widget_ScenarioReplay, !flag);
		if (flag)
		{
			RewardData rewardData = GetRewardData();
			bool flag2 = rewardData != null && rewardData.GoldMax > 0;
			UIHelper.EnableGameObject(InfoComponents.Widget_RewardGoldMinMax, flag2);
			UIHelper.EnableGameObject(InfoComponents.Label_NoGold, !flag2);
			if (flag2)
			{
				UIHelper.SetLabelText(InfoComponents.Label_GoldMin, rewardData.GoldMin);
				UIHelper.SetLabelText(InfoComponents.Label_GoldMax, rewardData.GoldMax);
			}
		}
	}

	public void SetupRewardExp()
	{
		UIHelper.EnableGameObject(InfoComponents.RewardExpRoot, !EnterDungeon.IsScenarioReplay);
		UIHelper.SetLabelText(InfoComponents.Label_RewardExp, GetRewardExp());
	}

	public virtual void SetupMonsterInfos()
	{
		InfoComponents.ReleaseMonsterComponents();
		UIHelper.EnableGameObject(InfoComponents.MonsterRoot, enable: true);
		AddGridMonsters(GetDungeonMonsterInfos(), InfoComponents.Grid_Monsters, InfoComponents.AddMonsterComponent);
	}

	public MonsterComponent AddGridBossMonster(MonsterHeroInfo monsterInfo, IGrid grid)
	{
		if (monsterInfo == null)
		{
			return null;
		}
		if (grid == null)
		{
			return null;
		}
		MonsterComponent monsterComponent = UIHelper.AddGridItem(grid, MonsterComponent.BossPrefabAuto);
		if (monsterComponent != null)
		{
			monsterComponent.Open(monsterInfo, default(MonsterComponent.StockData));
		}
		return monsterComponent;
	}

	public void AddGridMonster(MonsterHeroInfo monsterInfo, IGrid grid, Action<MonsterComponent> addAction)
	{
		if (monsterInfo != null && grid != null)
		{
			MonsterComponent monsterComponent = UIHelper.AddGridItem(grid, MonsterComponent.PrefabAuto);
			if (monsterComponent != null)
			{
				monsterComponent.Open(monsterInfo, default(MonsterComponent.StockData));
				Vespa_System_Action_Extension_Method.fire(addAction, monsterComponent);
			}
		}
	}

	public void AddGridMonsters(List<MonsterHeroInfo> monsterInfo, IGrid grid, Action<MonsterComponent> addAction)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(monsterInfo))
		{
			for (int i = 0; i < monsterInfo.Count; i++)
			{
				AddGridMonster(monsterInfo[i], grid, addAction);
			}
			if (grid != null)
			{
				UIHelper.RepositionGrid(grid);
			}
		}
	}

	public void SetupBattleReadyButton(bool notCurrentNode)
	{
		if (IsOnlyMultiDungeon())
		{
			UIHelper.EnableGameObject(InfoComponents.Button_BattleReady, enable: false);
			UIHelper.EnableGameObject(InfoComponents.Button_MoveEnter, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(InfoComponents.Button_BattleReady, !notCurrentNode);
			UIHelper.EnableGameObject(InfoComponents.Button_MoveEnter, notCurrentNode);
		}
	}

	private bool CheckBattleType(BattleType battleType)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(EnterDungeon.Chapter, EnterDungeon.LinkedDungeon);
		if (data == null)
		{
			return false;
		}
		return data.BattleType == battleType;
	}

	public void SetupPartyPlayButton()
	{
		bool flag = IsOnlyMultiDungeon();
		UIHelper.EnableGameObject(InfoComponents.ButtonOnlyParyPlay, flag);
		bool num = MultiPartyManagement.CheckPartyDungeonContentsStatus();
		bool flag2 = MultiPartyManagement.IsEnablePartyPlay(EnterDungeon);
		bool flag3 = num && flag2;
		int maxAvailableDifficulty = Helper.GetMaxAvailableDifficulty(EnterDungeon.Chapter, EnterDungeon.LinkedDungeon);
		bool flag4 = (int)EnterDungeon.Difficulty <= maxAvailableDifficulty;
		if (flag)
		{
			UIHelper.EnableGameObject(InfoComponents.Button_PartyPlay, enable: false);
			UIHelper.EnableButton(InfoComponents.ButtonOnlyParyPlay, flag3 && flag4, grayscaleIfDisable: true);
		}
		else
		{
			bool flag5 = CheckBattleType(BattleType.EventDungeon);
			UIHelper.EnableGameObject(InfoComponents.Button_PartyPlay, !flag5);
			UIHelper.EnableButton(InfoComponents.Button_PartyPlay, flag3 && flag4, grayscaleIfDisable: true);
		}
	}

	public static List<Component> AddRewardItemToGrid(RewardData rewardData, IGrid grid, bool includeCurrency = false, List<RewardElementData> addedList = null, BattleType battleType = BattleType.None, bool showHeroReward = false)
	{
		List<Component> list = new List<Component>();
		if (rewardData == null)
		{
			return list;
		}
		int GroupCount;
		List<RewardElementData> rewardItemListWithGroupCount = rewardData.GetRewardItemListWithGroupCount(out GroupCount);
		rewardItemListWithGroupCount.Sort(delegate(RewardElementData lreward, RewardElementData rreward)
		{
			ItemData itemData = ((lreward != null) ? GameRoot.Instance.ItemDataContainer.GetDataByCode(lreward.ItemCode) : null);
			ItemData itemData2 = ((rreward != null) ? GameRoot.Instance.ItemDataContainer.GetDataByCode(rreward.ItemCode) : null);
			if (itemData == null && itemData2 == null)
			{
				return 0;
			}
			if (itemData == null)
			{
				return 1;
			}
			if (itemData2 == null)
			{
				return -1;
			}
			int num3 = ((itemData.Grade > 10) ? itemData.Grade : (itemData.Grade * 10 - 1));
			int value = ((itemData2.Grade > 10) ? itemData2.Grade : (itemData2.Grade * 10 - 1));
			int num4 = num3.CompareTo(value);
			if (num4 != 0)
			{
				return -num4;
			}
			int type = (int)itemData.Type;
			int type2 = (int)itemData2.Type;
			num4 = type.CompareTo(type2);
			return (num4 != 0) ? num4 : itemData.Index.CompareTo(itemData2.Index);
		});
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(addedList) && addedList.Count > 0)
		{
			foreach (RewardElementData added in addedList)
			{
				string ItemCode = added.ItemCode;
				rewardItemListWithGroupCount.RemoveAll((RewardElementData x) => x.ItemCode.Equals(ItemCode));
			}
		}
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(rewardItemListWithGroupCount); num++)
		{
			RewardElementData rewardElementData = rewardItemListWithGroupCount[num];
			HashString itemCode = rewardElementData.ItemCode;
			if (includeCurrency && EnumLookup<CurrencyType>.TryParse(itemCode, out var o_value))
			{
				CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(grid, CurrencyIconComponent.PrefabAuto);
				if (!(currencyIconComponent == null))
				{
					CurrencyType currencyType = o_value;
					int itemMax = rewardElementData.ItemMax;
					CurrencyIconComponent.Listener listener = null;
					int num2 = itemMax;
					currencyIconComponent.Open(currencyType, num2, listener);
					if (rewardElementData.ItemMin != rewardElementData.ItemMax)
					{
						currencyIconComponent.SetMinMaxCount(rewardElementData.ItemMin, rewardElementData.ItemMax);
					}
					list.Add(currencyIconComponent);
				}
				continue;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
			if (dataByCode != null && (showHeroReward || dataByCode.Type != ItemType.Hero))
			{
				int index = dataByCode.Index;
				int itemMin = rewardElementData.ItemMin;
				long minCount = itemMin;
				int itemMax2 = rewardElementData.ItemMax;
				long maxCount = itemMax2;
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(grid, ItemIconComponent.PrefabAuto);
				if (!(itemIconComponent == null))
				{
					itemIconComponent.Open(dataByCode);
					UIHelper.ShowBonusItemEventByMinMax(itemIconComponent, index, minCount, maxCount, battleType);
					list.Add(itemIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(grid);
		return list;
	}

	public List<Component> AddEventDungeonPointReward(IGrid grid, int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficult)
	{
		List<Component> list = new List<Component>();
		Dictionary<CurrencyType, int> eventDungeonRewardPointDatas = GameRoot.Instance.EventDungeonDataContainer.GetEventDungeonRewardPointDatas(difficult, chapterIndex, dungeonIndex);
		if (__Extension_Method_ICollection__.COUNT(eventDungeonRewardPointDatas) == 0)
		{
			return list;
		}
		foreach (KeyValuePair<CurrencyType, int> item in eventDungeonRewardPointDatas)
		{
			if (item.Value != 0)
			{
				CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(grid, CurrencyIconComponent.PrefabAuto);
				if (!(currencyIconComponent == null))
				{
					CurrencyType key = item.Key;
					int value = item.Value;
					CurrencyIconComponent.Listener listener = null;
					int num = value;
					currencyIconComponent.Open(key, num, listener);
					list.Add(currencyIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(grid);
		return list;
	}

	public void UpdateButtonDispatch()
	{
		UIHelper.EnableButton(InfoComponents.ButtonDispatch, enable: false, grayscaleIfDisable: true);
		UIHelper.EnableGameObject(InfoComponents.ButtonDispatch, enable: true);
		UIHelper.EnableGameObject(InfoComponents.ButtonDispatchFinished, enable: false);
		if (InfoComponents == null || _enterDungeonCache == null || GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager == null)
		{
			return;
		}
		CampaignDungeonData originalDungeonData = _enterDungeonCache.OriginalDungeonData;
		if (originalDungeonData == null)
		{
			return;
		}
		if (!IsDispatchable())
		{
			UIHelper.EnableButton(InfoComponents.ButtonDispatch, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(InfoComponents.ButtonDispatch, enable: true);
			UIHelper.EnableGameObject(InfoComponents.ButtonDispatchFinished, enable: false);
			return;
		}
		UIHelper.EnableButton(InfoComponents.ButtonDispatch, enable: true, grayscaleIfDisable: true);
		int i = 1;
		for (int num = 4; i < num; i++)
		{
			WCL_DispatchBattleInfo wCL_DispatchBattleInfo = GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.Get(originalDungeonData.ChapterIndex, originalDungeonData.DungeonIndex, i);
			if (wCL_DispatchBattleInfo != null && wCL_DispatchBattleInfo.State == DispatchState.ReadyToComplete)
			{
				UIHelper.EnableGameObject(InfoComponents.ButtonDispatch, enable: false);
				UIHelper.EnableGameObject(InfoComponents.ButtonDispatchFinished, enable: true);
				return;
			}
		}
		UIHelper.EnableGameObject(InfoComponents.ButtonDispatch, enable: true);
		UIHelper.EnableGameObject(InfoComponents.ButtonDispatchFinished, enable: false);
	}

	private bool IsDispatchable()
	{
		if (!DispatchHelper.IsDispatchable(_enterDungeonCache.OriginalDungeonData))
		{
			return false;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_enterDungeonCache.Chapter, _enterDungeonCache.LinkedDungeon, _enterDungeonCache.Difficulty))
		{
			return false;
		}
		return true;
	}

	public void OnClickDifficultySlot(GameObject senderObject)
	{
		DungeonDifficultyComponent dungeonDiffcultyComp = GetDungeonDiffcultyComp();
		if ((bool)dungeonDiffcultyComp && dungeonDiffcultyComp.OnClickDifficultyButton(senderObject))
		{
			ModifiedDungeonData(dungeonDiffcultyComp.DifficultyType, dungeonDiffcultyComp.PlayScenario);
			Refresh();
		}
	}

	public void OnClickDifficultyEasy()
	{
		DungeonDifficultyComponent dungeonDiffcultyComp = GetDungeonDiffcultyComp();
		if ((bool)dungeonDiffcultyComp && dungeonDiffcultyComp.SelectDifficuly(NShared.DungeonDifficultyType.Easy))
		{
			ModifiedDungeonData(dungeonDiffcultyComp.DifficultyType, dungeonDiffcultyComp.PlayScenario);
			Refresh();
		}
	}

	public void OnClickDifficultyNormal()
	{
		DungeonDifficultyComponent dungeonDiffcultyComp = GetDungeonDiffcultyComp();
		if ((bool)dungeonDiffcultyComp && dungeonDiffcultyComp.SelectDifficuly(NShared.DungeonDifficultyType.Normal))
		{
			ModifiedDungeonData(dungeonDiffcultyComp.DifficultyType, dungeonDiffcultyComp.PlayScenario);
			Refresh();
		}
	}

	public void OnClickDifficultyHard()
	{
		DungeonDifficultyComponent dungeonDiffcultyComp = GetDungeonDiffcultyComp();
		if ((bool)dungeonDiffcultyComp && dungeonDiffcultyComp.SelectDifficuly(NShared.DungeonDifficultyType.Hard))
		{
			ModifiedDungeonData(dungeonDiffcultyComp.DifficultyType, dungeonDiffcultyComp.PlayScenario);
			Refresh();
		}
	}

	public void OnClickDifficultyHell()
	{
		DungeonDifficultyComponent dungeonDiffcultyComp = GetDungeonDiffcultyComp();
		if ((bool)dungeonDiffcultyComp && dungeonDiffcultyComp.SelectDifficuly(NShared.DungeonDifficultyType.Hell))
		{
			ModifiedDungeonData(dungeonDiffcultyComp.DifficultyType, dungeonDiffcultyComp.PlayScenario);
			Refresh();
		}
	}

	public void OnClickDifficultyScenario()
	{
		DungeonDifficultyComponent dungeonDiffcultyComp = GetDungeonDiffcultyComp();
		if ((bool)dungeonDiffcultyComp && dungeonDiffcultyComp.SelectDifficulyScenario())
		{
			ModifiedDungeonData(dungeonDiffcultyComp.DifficultyType, dungeonDiffcultyComp.PlayScenario);
			Refresh();
		}
	}

	private void ModifiedDungeonData(NShared.DungeonDifficultyType difficulty, bool isScenario)
	{
		_enterDungeonCache = new EnterDungeonData(_enterDungeonCache.Chapter, _enterDungeonCache.OriginalDungeon, _enterDungeonCache.LinkedDungeon, difficulty, isScenario);
	}

	void IBoosterListObserver.OnChanged(BoosterInfo expiredBooster)
	{
		if (expiredBooster != null && expiredBooster.BonusType == EventBonusType.SaveStamina)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("EQUIP_END_BOOSTER", expiredBooster.Name));
			Refresh();
		}
	}

	void IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>.OnChanged(UserDispatchBattleInfoManager.Observer.BroadCastInfo changedInfo)
	{
		UserDispatchBattleInfoManager.Observer.BroadCastReason reason = changedInfo.Reason;
		if (reason == UserDispatchBattleInfoManager.Observer.BroadCastReason.OnUpdate || reason == UserDispatchBattleInfoManager.Observer.BroadCastReason.OnRemove)
		{
			UpdateButtonDispatch();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
