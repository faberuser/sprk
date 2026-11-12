using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NEventTrigger;
using NGame2.NHelper;
using NGame2.NLobby;
using NGame2.NLobby.NActivity;
using NGame2.NParty;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper;
using NGame2.NUI.NHelper.NEquip;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NEntryMenu.NAction;
using NGame2.NUI.NWindow.NInventory;
using NGame2.NUtil;
using NShared;
using NShared.ApplyHeroRunePage;
using NShared.ApplyHeroTranscendSkillPage;
using NShared.BuyHeroTranscendSkillPage;
using NShared.EquipRune;
using NShared.ExtendEquip;
using NShared.ExtendHeroSkill;
using NShared.ExtendRunePage;
using NShared.HeroLimitBreakExpUp;
using NShared.HeroLimitBreakLevelUp;
using NShared.LearnHeroSkill;
using NShared.LearnHeroTranscendSkillPage;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.ResetHeroTranscendSkillPage;
using NShared.SetEquip;
using NShared.SetEquipSwap;
using NShared.SetHideHeroUniqueWeapon;
using NShared.UnequipRune;
using NShared.UnlockEquip;
using NShared.UnsetEquip;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class MyHeroManagement : BaseManagement<MyHeroManagement>, HeroPortraitList.Listener, Hero3DViewPanel.Listener, HeroFullSceneViewBase.Listener, HeroDetailView.Listener, HeroStatPanel.Listener, HeroEquipListPanel.Listener, HeroEquipInfo.Listener, InvenEquipInfo.Listener, HeroRunehengeWindow.IListener, HeroSkillListPanel.Listener, HeroList.Listener, HeroSkillDesc.Listener, HeroDeck2.Listener, HeroTranscendSkillListPanel.Listener, HeroEquipTreasure.Listener, HeroEquipStoragePanel.IListener, HeroEquipStoragePanel.IDataProvider, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, IUserObserver<UserManager.UserStatInfo>, RecommendTranscendContainer.Listener, ArchiveHeroProfilePanel.Listener, HeroPresetStorageWindow.IListener, HeroPresetStorageWebManager.IListener
{
	private CachedHeroInfo _currentHero;

	private Hero3DViewPanel.EToggle _lastViewToggle = Hero3DViewPanel.EToggle.Info;

	private WebServiceRequest _currentServiceRequest;

	private EquipPartType _lastEquipPartType;

	private EquipPartType _selectedEquipPartType;

	public Hero3DViewPanel GetHero3DView(bool openedOnly = false)
	{
		HeroList heroList = GetInstance<HeroList>(openedOnly);
		if (heroList == null)
		{
			return null;
		}
		return heroList.Hero3DViewPanel;
	}

	private HeroStatPanel GetHeroStat()
	{
		HeroList heroList = GetInstance<HeroList>();
		if (heroList == null)
		{
			return null;
		}
		return heroList.HeroStatPanel;
	}

	public void RefreshHeroStat(int heroIndex)
	{
		HeroStatPanel heroStat = GetHeroStat();
		if (!(heroStat == null) && heroStat.isActiveAndEnabled && heroStat.CachedHeroInfo.HeroIndex == heroIndex)
		{
			heroStat.Open(heroStat.CachedHeroInfo, this);
		}
	}

	private HeroSkillListPanel GetHeroSkillList()
	{
		HeroList heroList = GetInstance<HeroList>();
		if (heroList == null)
		{
			return null;
		}
		return heroList.HeroSkillListPanel;
	}

	private HeroEquipListPanel GetHeroEquipList(bool openedOnly = false)
	{
		HeroList heroList = GetInstance<HeroList>(openedOnly);
		if (heroList == null)
		{
			return null;
		}
		return heroList.HeroEquipListPanel;
	}

	private HeroTranscendSkillListPanel GetHeroTranscendSkillList()
	{
		HeroList heroList = GetInstance<HeroList>();
		if (heroList == null)
		{
			return null;
		}
		return heroList.HeroTranscendSkillListPanel;
	}

	private HeroEquipStoragePanel GetHeroEquipStorageList(bool openedOnly = false)
	{
		HeroList heroList = GetInstance<HeroList>(openedOnly);
		if (heroList == null)
		{
			return null;
		}
		return heroList.HeroEquipStoragePanel;
	}

	private ArchiveHeroProfilePanel GetHeroProfile()
	{
		HeroList heroList = GetInstance<HeroList>();
		if (heroList == null)
		{
			return null;
		}
		return heroList.HeroProfilePanel;
	}

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<HeroDetailView>("Resources/Prefab2/Window/FullScene/HeroDetailView");
		AddInstance<HeroPortraitList>("Resources/Prefab2/Window/Hero/HeroPortraitList");
		AddInstance<InvenEquipInfoUIContainer>("Resources/Prefab2/Window/Inventory/invenEquipInfouiContainer");
		AddInstance<ForgeShortCutWindow>("Resources/Prefab2/Window/GameOption/ForgeShortCutWindow");
		AddInstance<HeroEquipUpgradeResultPopup>("Resources/Prefab2/Window/Hero/HeroEquipUpgradeResultPopup");
		AddInstance<HeroRunehengeWindow>("Resources/Prefab2/Window/Hero/HeroRunehenge");
		AddInstance<HeroList>("Resources/Prefab2/Window/Hero/HeroList");
		AddInstance<HeroSkillDesc>("Resources/Prefab2/Window/Hero/HeroSkillDesc");
		AddInstance<HeroDeck2>("Resources/Prefab2/Window/Hero/HeroDeck2");
		AddInstance<HeroEquipTreasure>("Resources/Prefab2/Window/Hero/HeroEquipTreasure");
		AddInstance<HeroEquipBreakResultPopup>("Resources/Prefab2/Window/Hero/HeroEquipBreakResultPopup");
		AddInstance<RecommendTranscendContainer>("Resources/Prefab2/Window/RecommendTranscend/RecommendTranscendContainer");
		AddInstance<RecommendEquipmentContainer>("Resources/Prefab2/Window/RecommendEquipment/RecommendEquipmentContainer");
		AddInstance<HeroPresetStorageWindow>("Resources/Prefab2/Window/Hero/HeroPresetStorageWindow");
		AddInstance<HeroLimitBreakGate>("Resources/Prefab2/Window/Hero/HeroLimitBreakGate");
		AddInstance<HeroLimitBreakExpItemUsePopup>("Resources/Prefab2/Window/Hero/HeroLimitBreakExpItemUsePopup");
		ReserveIconCache();
	}

	public static void ReserveIconCache()
	{
	}

	private void ReturnToDeck()
	{
		new GroupTask().ADDTASK(base.TaskFactory.Close<HeroDeck2>()).ADDTASK(base.TaskFactory.Open(delegate(HeroDeck2 window)
		{
			window.Init(this);
		})).WrapperTouchControl()
			.DisableTouch()
			.Commit();
	}

	private void Start()
	{
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		if (GetHeroEquipList(openedOnly: true) != null)
		{
			GetHeroEquipList().Update();
		}
		if (GetHero3DView(openedOnly: true) != null)
		{
			GetHero3DView().Update();
		}
	}

	public void Init()
	{
		SingletonAuto<HeroPresetStorageWebManager>.instance.AddListener(this);
		GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ObserverManager.RemoveObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.ObserverManager.AddObserver(this);
	}

	private void ResetScroll()
	{
		if (GetHeroStat() != null)
		{
			GetHeroStat().ResetScroll();
		}
		if (GetHeroEquipList() != null)
		{
			GetHeroEquipList().ResetScroll();
		}
	}

	public void SetHeroInfo(CachedHeroInfo heroInfo)
	{
		_currentHero = heroInfo;
	}

	public void OpenMyDeck()
	{
		OpenMyDeckAsync().WrapperTouchControl().DisableTouch().Commit();
	}

	public void OpenMyHero(CachedHeroInfo cachedHeroInfo, Hero3DViewPanel.EToggle toggle = Hero3DViewPanel.EToggle.Info, Hero3DViewPanel.ENextTask nextTask = Hero3DViewPanel.ENextTask.None, EquipPartType equipPart = EquipPartType.Weapon, bool immediately = false)
	{
		OpenMyHeroAsync(cachedHeroInfo, toggle, nextTask, equipPart, immediately).WrapperTouchControl().DisableTouch().Commit();
	}

	public void OpenInventory(ItemType itemType, EquipPartType partType)
	{
		CachedHeroInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetValues();
		if (values == null || values.Length == 0)
		{
			return;
		}
		CachedHeroInfo cachedHeroInfo = values[0];
		if (cachedHeroInfo == null)
		{
			return;
		}
		OpenMyHeroAsync(cachedHeroInfo, Hero3DViewPanel.EToggle.Equip, Hero3DViewPanel.ENextTask.None, EquipPartType.Weapon, immediately: true).ADDTASK(base.TaskFactory.Run(delegate
		{
			if (GetHeroEquipList() != null)
			{
				GetHeroEquipList().Open(_currentHero, partType, this);
			}
		})).WrapperTouchControl().DisableTouch()
			.Commit();
	}

	public void OpenInventory(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			return;
		}
		CachedHeroInfo cachedHeroInfo = null;
		List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
		if (heroInfoList == null)
		{
			return;
		}
		if (equipItemInfo.IsEquipped)
		{
			cachedHeroInfo = equipItemInfo.EquippedHeroInfo;
		}
		else if (equipItemInfo.IsUniqueTreasure || equipItemInfo.IsUniqueWeapon)
		{
			if (equipItemInfo.EquipItemData == null)
			{
				return;
			}
			if (Vespa_Generic_IList_Extension_Method.IsValidIndex(equipItemInfo.EquipItemData.CreatureIndex, 0))
			{
				int heroIndex = equipItemInfo.EquipItemData.CreatureIndex[0];
				cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
			}
		}
		if (cachedHeroInfo == null)
		{
			int i = 0;
			for (int count = heroInfoList.Count; i < count; i++)
			{
				CachedHeroInfo cachedHeroInfo2 = heroInfoList[i];
				if (cachedHeroInfo2 != null && IsEquipTargetHero(cachedHeroInfo2.CreatureData, equipItemInfo.ItemData as ItemData<EquipItemData>))
				{
					cachedHeroInfo = cachedHeroInfo2;
					if (cachedHeroInfo != null)
					{
						break;
					}
				}
			}
		}
		if (cachedHeroInfo == null && Vespa_Generic_IList_Extension_Method.IsValidIndex(heroInfoList, 0))
		{
			cachedHeroInfo = heroInfoList[0];
		}
		new ParallelTask().ADDTASK(base.TaskFactory.Run<InvenEquipInfoUIContainer>(delegate
		{
			InvenEquipInfoUIContainer.WindowPosType windowPosition = InvenEquipInfoUIContainer.WindowPosType.Right;
			if (equipItemInfo.IsEquipped)
			{
				windowPosition = InvenEquipInfoUIContainer.WindowPosType.Center;
			}
			return CoOpeningInvenEquipInfoUIContainer(windowPosition, equipItemInfo, this);
		})).ADDTASK(OpenMyHeroAsync(cachedHeroInfo, Hero3DViewPanel.EToggle.Equip, Hero3DViewPanel.ENextTask.None, EquipPartType.Weapon, immediately: true, fromInventory: true)).WrapperTouchControl()
			.DisableTouch()
			.Commit();
	}

	private IEnumerator CoOpeningInvenEquipInfoUIContainer(InvenEquipInfoUIContainer.WindowPosType windowPosition, EquipItemInfo equipItemInfo, HeroEquipInfo.Listener listener)
	{
		WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();
		InvenEquipInfoUIContainer ui = GetInstance<InvenEquipInfoUIContainer>();
		if (ui == null)
		{
			yield return waitFrame;
		}
		if (!ui.IsOpen())
		{
			yield return waitFrame;
		}
		if (_currentHero == null)
		{
			yield return waitFrame;
		}
		HeroList heroIterator = GetInstance<HeroList>();
		SetSelectedItemIcon(equipItemInfo);
		ui.Open(windowPosition, _currentHero, equipItemInfo, listener, heroIterator);
	}

	private bool IsEquipTargetHero(CreatureData creatureData, ItemData<EquipItemData> itemData)
	{
		if (itemData.SubData.CreatureIndex != null && itemData.SubData.CreatureIndex.Length != 0)
		{
			return itemData.SubData.CreatureIndex[0] == creatureData.Index;
		}
		return creatureData.HasTag(itemData.SubData.TagType);
	}

	private void OpenEquipListOfType(EquipPartType partType)
	{
		OpenEquipListOfTypeAsync(partType).WrapperTouchControl().DisableTouch().Commit();
	}

	private void OpenPortraitList()
	{
		new GroupTask().ADDTASK(base.TaskFactory.Run(delegate(HeroPortraitList window)
		{
			window.Open(this, _currentHero.HeroIndex);
		})).WrapperTouchControl().DisableTouch()
			.Commit();
	}

	public void CloseEquipUpgrade()
	{
		new GroupTask().ADDTASK(base.TaskFactory.Run(delegate
		{
			if (GetHeroEquipList() != null)
			{
				GetHeroEquipList().Close();
			}
		})).WrapperTouchControl().DisableTouch()
			.Commit();
	}

	public void OpenRunehenge()
	{
		new GroupTask().ADDTASK(base.TaskFactory.Run(delegate
		{
			Hero3DViewPanel hero3DView = GetHero3DView();
			if (hero3DView != null && hero3DView.gameObject.activeSelf)
			{
				hero3DView.SetToggle(_lastViewToggle);
			}
		})).ADDTASK(base.TaskFactory.Run(delegate(HeroRunehengeWindow window)
		{
			if (_currentHero != null)
			{
				window.Open(this, _currentHero);
			}
		})).WrapperTouchControl()
			.DisableTouch()
			.Commit();
	}

	public void OpenEquipRune(HeroEquipInfo caller, bool inventoryMode = false)
	{
		if (caller.EquipItemInfo == null)
		{
			return;
		}
		new GroupTask().ADDTASK(base.TaskFactory.Run(delegate
		{
			Hero3DViewPanel hero3DView = GetHero3DView();
			if (hero3DView != null && hero3DView.gameObject.activeSelf && _lastViewToggle != Hero3DViewPanel.EToggle.EquipStorage && _lastViewToggle != Hero3DViewPanel.EToggle.Equip)
			{
				hero3DView.SetToggle(Hero3DViewPanel.EToggle.Equip);
			}
		})).ADDTASK(base.TaskFactory.Run(delegate(HeroRunehengeWindow window)
		{
			window.Open(this, _currentHero);
		})).WrapperTouchControl()
			.DisableTouch()
			.Commit();
	}

	public void RestoreHeroDetailView(CachedHeroInfo heroInfo, Action onEnd)
	{
		if (heroInfo == null)
		{
			heroInfo = SceneAutoBehaviour<StatDataCache>.instance.GetReferenceHero(HeroDetailView.HeroIndex);
			Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask().ADDTASK(base.TaskFactory.Open(delegate(HeroDeck2 window)
			{
				window.Init(this, HeroDeck2.EPositionTab.NotCompleted);
			}, immediately: true)).ADDTASK(GetInstance<HeroDetailView>().OpenAsync(heroInfo, this, useSceneChanger: false)), onEnd).WrapperTouchControl().DisableTouch()
				.Commit();
		}
		else
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(HeroDetailView.HeroIndex);
			Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask().ADDTASK(base.TaskFactory.Open(delegate(HeroDeck2 window)
			{
				window.Init(this);
			}, immediately: true)).ADDTASK(OpenMyHeroAsync(cachedHeroInfo, Hero3DViewPanel.EToggle.Info, Hero3DViewPanel.ENextTask.None, EquipPartType.Weapon, immediately: true)).ADDTASK(GetInstance<HeroDetailView>().OpenAsync(heroInfo, this, useSceneChanger: false)), onEnd).WrapperTouchControl().DisableTouch()
				.Commit();
		}
	}

	public void OpenHeroSkillDesc(CachedHeroInfo info, int skillSlotIndex, int linkedIndex = 0)
	{
		GetInstance<HeroSkillDesc>().Open(info, skillSlotIndex, linkedIndex, this);
	}

	private GroupTask OpenMyDeckAsync()
	{
		OpenHeroList triggerEvent = new OpenHeroList();
		if (SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent))
		{
			return Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask(), delegate
			{
				_lastViewToggle = Hero3DViewPanel.EToggle.Info;
				_lastEquipPartType = EquipPartType.Weapon;
			}).ADDTASK(base.TaskFactory.Open(delegate(HeroDeck2 window)
			{
				window.TutorialInit(this);
			}));
		}
		return Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask(), delegate
		{
			_lastViewToggle = Hero3DViewPanel.EToggle.Info;
			_lastEquipPartType = EquipPartType.Weapon;
		}).ADDTASK(base.TaskFactory.Open(delegate(HeroDeck2 window)
		{
			window.Init(this);
		}));
	}

	public GroupTask OpenMyHeroAsync(CachedHeroInfo cachedHeroInfo, Hero3DViewPanel.EToggle toggle = Hero3DViewPanel.EToggle.Info, Hero3DViewPanel.ENextTask nextTask = Hero3DViewPanel.ENextTask.None, EquipPartType equipPart = EquipPartType.Weapon, bool immediately = false, bool fromInventory = false, bool enableBookmark = false)
	{
		return Vespa_Task_ExtensionMethod.ADDTASK(Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask(), delegate
		{
			_currentHero = cachedHeroInfo;
			_lastViewToggle = toggle;
			_lastEquipPartType = equipPart;
		}).ADDTASK(new BranchTask().If(base.TaskFactory.IsOpen<HeroList>()).Then(Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask().ADDTASK(base.TaskFactory.Run(delegate(HeroList window)
		{
			window.SelectHero(_currentHero.HeroIndex);
		})), delegate
		{
			if (GetHero3DView() != null)
			{
				GetHero3DView().Init(this, _currentHero, toggle);
			}
		})).Else(base.TaskFactory.Open(delegate(HeroList window)
		{
			window.Init(this, _currentHero.HeroIndex, enableBookmark);
			if (GetHero3DView() != null)
			{
				GetHero3DView().Init(this, _currentHero, toggle);
			}
			ResetScroll();
		}))), delegate
		{
			if (!fromInventory)
			{
				InvenEquipInfoUIContainer invenEquipInfoUIContainer = GetInstance<InvenEquipInfoUIContainer>();
				if (invenEquipInfoUIContainer.IsOpen())
				{
					invenEquipInfoUIContainer.ChangeHero(_currentHero);
				}
				CloseInstance<RecommendEquipmentContainer>();
			}
			if (nextTask == Hero3DViewPanel.ENextTask.OpenRunehenge && !IsOpened<HeroRunehengeWindow>())
			{
				OpenRunehenge();
			}
		});
	}

	private GroupTask OpenEquipListOfTypeAsync(EquipPartType partType)
	{
		return new GroupTask().ADDTASK(base.TaskFactory.Run(delegate
		{
			if (GetHero3DView() != null && _lastViewToggle != Hero3DViewPanel.EToggle.Equip)
			{
				GetHero3DView().SetToggle(Hero3DViewPanel.EToggle.Equip);
			}
		})).ADDTASK(base.TaskFactory.Run(delegate
		{
			HeroEquipListPanel heroEquipList = GetHeroEquipList();
			if (heroEquipList != null && heroEquipList.gameObject.activeSelf && partType != _selectedEquipPartType)
			{
				heroEquipList.Open(_currentHero, partType, this);
				heroEquipList.ResetScroll();
				_selectedEquipPartType = partType;
			}
		}));
	}

	private void ProcessAutoEquip()
	{
		if (_currentHero != null)
		{
			AutoEquip autoEquip = new AutoEquip(_currentHero);
			autoEquip.CollectEquipment();
			if (autoEquip.HasEquipment())
			{
				RequestSetEquip(autoEquip.EquipItemSlotIndexList.ToArray(), _currentHero.HeroIndex, autoEquip.PartIndexList.ToArray());
			}
		}
	}

	private void RequestSetEquip(int equipItemSlotIndex, int heroIndex, int heroPartIndex)
	{
		RequestSetEquip(new int[1] { equipItemSlotIndex }, heroIndex, new int[1] { heroPartIndex });
	}

	public void RequestSetEquip(int[] equipItemSlotIndices, int heroIndex, int[] heroPartIndices, bool isPing = false)
	{
		WebRequestFlag flag = ((!isPing) ? WebRequestFlag.ShowLoadingGauge : WebRequestFlag.Ping);
		NShared.SetEquip.Request request = new NShared.SetEquip.Request
		{
			HeroIndex = heroIndex,
			HeroPartIndex = heroPartIndices,
			EquipItemSlotIndex = equipItemSlotIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, flag, delegate(NShared.SetEquip.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.SetEquip.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SetEquip.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
				UnEquipItems(heroInfo, response.UnEquippedSlotIndex);
				EquipItems(heroInfo, response.EquippedSlotIndex, heroPartIndices);
				CloseInstance<InvenEquipInfoUIContainer>();
				CloseInstance<RecommendEquipmentContainer>();
				GetHeroEquipList().Refresh();
				Hero3DViewPanel hero3DView = GetHero3DView();
				if (hero3DView != null && hero3DView.gameObject.activeSelf)
				{
					hero3DView.SetupEquipItemIcon();
				}
				RefreshHeroEquipTreasure(_currentHero);
			}
		});
	}

	private void RequestSwapEquip(int[] equipItemSlotIndices, int heroIndex, int[] heroPartIndices)
	{
		NShared.SetEquipSwap.Request request = new NShared.SetEquipSwap.Request
		{
			HeroIndex = heroIndex,
			HeroPartIndex = heroPartIndices,
			EquipItemSlotIndex = equipItemSlotIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetEquipSwap.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.SetEquip.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SetEquipSwap.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
				UnEquipItems(heroInfo, response.UnEquippedSlotIndex);
				UnEquipItems(heroInfo, response.EquippedSlotIndex);
				EquipItems(heroInfo, response.EquippedSlotIndex, heroPartIndices);
				CloseInstance<InvenEquipInfoUIContainer>();
				CloseInstance<RecommendEquipmentContainer>();
				GetHeroEquipList().Refresh();
				Hero3DViewPanel hero3DView = GetHero3DView();
				if (hero3DView != null && hero3DView.gameObject.activeSelf)
				{
					hero3DView.SetupEquipItemIcon();
				}
				RefreshHeroEquipTreasure(_currentHero);
			}
		});
	}

	private GroupTask ProcessAutoUnequipAsync()
	{
		if (_currentHero == null)
		{
			return new GroupTask();
		}
		List<int> list = new List<int>();
		foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
		{
			EquipItemInfo equipItemInfoByPartIndex = _currentHero.GetEquipItemInfoByPartIndex(allEquipSlotDatum.Index);
			if (equipItemInfoByPartIndex != null)
			{
				list.Add(equipItemInfoByPartIndex.SlotIndex);
			}
		}
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			return new GroupTask();
		}
		return RequestUnsetEquipAsync(list.ToArray(), _currentHero);
	}

	private GroupTask RequestUnsetEquipAsync(int equipItemSlotIndex, CachedHeroInfo equippedHeroInfo, bool isUnEquipAction = true)
	{
		return RequestUnsetEquipAsync(new int[1] { equipItemSlotIndex }, equippedHeroInfo, isUnEquipAction);
	}

	private void RequestUnsetEquip(int[] equipItemSlotIndices, CachedHeroInfo equippedHeroInfo, bool isUnEquipAction = true, Action onSuccess = null)
	{
		RequestUnsetEquipAsync(equipItemSlotIndices, equippedHeroInfo, isUnEquipAction, onSuccess).Commit();
	}

	private GroupTask RequestUnsetEquipAsync(int[] equipItemSlotIndices, CachedHeroInfo equippedHeroInfo, bool isUnEquipAction = true, Action onSuccess = null)
	{
		WebServiceTask<NShared.UnsetEquip.Request, NShared.UnsetEquip.Response> task = new WebServiceTask<NShared.UnsetEquip.Request, NShared.UnsetEquip.Response>(new NShared.UnsetEquip.Request
		{
			EquipItemSlotIndex = equipItemSlotIndices
		}, delegate(NShared.UnsetEquip.Request request, NShared.UnsetEquip.Response response)
		{
			if (response.Result != NShared.UnsetEquip.ResultType.Success)
			{
				return base.TaskFactory.Run(delegate
				{
					MessagePopup.OpenError(response.Result);
				});
			}
			UnEquipItems(equippedHeroInfo, response.UnequippedSlotIndex);
			return Vespa_Task_ExtensionMethod.ADDTASK(Vespa_Task_ExtensionMethod.ADDTASK(Vespa_Task_ExtensionMethod.ADDTASK(Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask(), delegate
			{
				Hero3DViewPanel hero3DView = GetHero3DView();
				if (hero3DView != null && hero3DView.gameObject.activeSelf && isUnEquipAction)
				{
					hero3DView.SetupEquipItemIcon();
				}
				if (IsOpened<HeroEquipTreasure>())
				{
					RefreshHeroEquipTreasure(_currentHero);
				}
			}), delegate
			{
				HeroEquipListPanel heroEquipList = GetHeroEquipList();
				if (heroEquipList != null)
				{
					heroEquipList.Refresh();
				}
			}), delegate
			{
				CloseInstance<InvenEquipInfoUIContainer>();
				CloseInstance<RecommendEquipmentContainer>();
				if (equippedHeroInfo.HeroIndex != _currentHero.HeroIndex && isUnEquipAction && equipItemSlotIndices.Length == 1)
				{
					int key = equipItemSlotIndices[0];
					EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(key);
					if (equipItemInfo != null)
					{
						ShowEquipInfo(_currentHero, equipItemInfo, isFromEquipListPanel: true);
					}
				}
			}), delegate
			{
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}).WrapperTouchControl().DisableTouch();
		});
		return new GroupTask().ADDTASK(task);
	}

	private bool IsAllUnEquipStorageEquipItem(CachedHeroInfo heroInfo, EquipStorageSlotInfo storageInfo)
	{
		if (storageInfo == null)
		{
			return false;
		}
		bool result = true;
		EquipPartType[] saveEquipParts = EquipStorageSlotInfo.SaveEquipParts;
		foreach (EquipPartType partIndex in saveEquipParts)
		{
			int equipItemSlotIndex = storageInfo.GetEquipItemSlotIndex((int)partIndex);
			if (equipItemSlotIndex != 0)
			{
				EquipItemInfo equipItemInfoByPartIndex = heroInfo.GetEquipItemInfoByPartIndex((int)partIndex);
				if (equipItemInfoByPartIndex != null && equipItemInfoByPartIndex.SlotIndex == equipItemSlotIndex)
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	private void EquipItems(CachedHeroInfo heroInfo, int[] equippedSlotIndex, int[] partIndices)
	{
		if (heroInfo == null || __Extension_Method_ICollection__.COUNT(equippedSlotIndex) == 0)
		{
			return;
		}
		int heroIndex = heroInfo.HeroIndex;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(equippedSlotIndex); i++)
		{
			int num = equippedSlotIndex[i];
			int partIndex = partIndices[i];
			EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(num);
			if (equipItemInfo == null)
			{
				VespaDebug.LogError("equipItem not found : equipItemSlotIndex=" + num);
				continue;
			}
			if (GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex) == null)
			{
				VespaDebug.LogError("equipItemData  not found : itemIndex =" + equipItemInfo.ItemIndex);
				continue;
			}
			CachedHeroInfo heroOfEquipItem = GlobalAutoBehaviour<AccountManager>.instance.GetHeroOfEquipItem(num);
			EquipItemInfo equipItemInfo2 = null;
			if (heroInfo != null)
			{
				equipItemInfo2 = heroInfo.GetEquipItemInfoByPartIndex(partIndex);
				heroInfo.SetEquipItemInfo(partIndex, equipItemInfo);
				AssetSound.PlayItemEquipSound(equipItemInfo.ItemIndex);
			}
			if (equipItemInfo != null)
			{
				int num2 = heroOfEquipItem?.HeroIndex ?? 0;
				if (equipItemInfo2 != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyEquippedHeroIndex(equipItemInfo2, heroIndex, 0);
				}
				if (num2 != 0)
				{
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyEquippedHeroIndex(equipItemInfo, num2, 0);
				}
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyEquippedHeroIndex(equipItemInfo, num2, heroIndex);
			}
		}
	}

	private void UnEquipItems(CachedHeroInfo heroInfo, int[] unequippedSlotIndex)
	{
		if (heroInfo == null || __Extension_Method_ICollection__.COUNT(unequippedSlotIndex) == 0)
		{
			return;
		}
		int heroIndex = heroInfo.HeroIndex;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(unequippedSlotIndex); i++)
		{
			int num = unequippedSlotIndex[i];
			EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(num);
			if (equipItemInfo == null)
			{
				VespaDebug.LogError("EquipItemInfo not found : equipItemSlotIndex=" + num + " HeroIndex=" + heroIndex);
				continue;
			}
			int partIndexOfEquipItem = heroInfo.GetPartIndexOfEquipItem(num);
			if (partIndexOfEquipItem == 0)
			{
				VespaDebug.LogError("Part index not found : equipItemSlotIndex=" + num + " HeroIndex=" + heroIndex);
				continue;
			}
			heroInfo.SetEquipItemInfo(partIndexOfEquipItem, null);
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyEquippedHeroIndex(equipItemInfo, heroIndex, 0);
		}
	}

	private void AskUnlockEquipSlotAsync(int heroIndex, int heroPartIndex)
	{
		string text = "EquipItemUnlockPrice" + heroPartIndex;
		string text2 = "EquipItemUnlockCostType" + heroPartIndex;
		string key = "EquipItemUnlockHeroLevel" + heroPartIndex;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt(key);
		if (valueInt != 0)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
			if (cachedHeroInfo == null)
			{
				VespaDebug.LogError("EquipUnlock Hero not found : heroIndex=" + heroIndex);
				return;
			}
			if (cachedHeroInfo.Level < valueInt)
			{
				MessagePopup.OpenError(LocalizationManager.GetString(Equip.EQUIP_UNLOCK_REQ_HERO_LEVEL, valueInt));
				return;
			}
		}
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt(text);
		if (valueInt2 == 0)
		{
			VespaDebug.LogError("EquipUnlock price is 0 : Key=" + text);
			return;
		}
		PricedMessagePopup.EPriceType valueEnum = GameRoot.Instance.ConstantDataContainer.GetValueEnum<PricedMessagePopup.EPriceType>(text2);
		if (valueEnum == PricedMessagePopup.EPriceType.None)
		{
			VespaDebug.LogError("EquipUnlock costType is None : Key=" + text2);
			return;
		}
		string text3 = LocalizationManager.GetString((EquipPartType)heroPartIndex);
		string message = LocalizationManager.GetString(Equip.EQUIP_ASK_UNLOCK_EQUIP, text3);
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string action = LocalizationManager.GetString(Equip.EQUIP_UNLOCK_ACTION);
		PricedMessagePopup.Open(title, message, action, valueEnum, valueInt2, delegate
		{
			WebServiceManager.Request(new NShared.UnlockEquip.Request
			{
				HeroIndex = heroIndex,
				HeroPartIndex = heroPartIndex
			}, delegate(NShared.UnlockEquip.Response response)
			{
				if (response.Result != NShared.UnlockEquip.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
					GlobalAutoBehaviour<AccountManager>.instance.SetEquipSlotOpened(heroIndex, heroPartIndex);
					SetHero3DViewEquipItemIcon();
				}
			});
		});
	}

	public void SetHero3DViewEquipItemIcon()
	{
		Hero3DViewPanel hero3DView = GetHero3DView();
		if (hero3DView != null)
		{
			hero3DView.SetupEquipItemIcon();
		}
	}

	private static bool ExistDuplicatedSubTypeInList(EquipPartType partType, EquipItemSubType subType, ICollection<int> itemIndexList)
	{
		foreach (int itemIndex in itemIndexList)
		{
			ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemIndex);
			if (data != null && data.SubData.PartType == partType && data.SubData.SubType == subType)
			{
				return true;
			}
		}
		return false;
	}

	private void ShowEquipInfo(ItemIconComponent itemIconComponent, bool isFromEquipListPanel = false)
	{
		if (!(itemIconComponent == null))
		{
			itemIconComponent.SetNew(isNew: false);
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.NewIconRemove(itemIconComponent.EquipItemInfo);
			EquipItemInfo equipItemInfo = itemIconComponent.EquipItemInfo;
			CachedHeroInfo cachedHeroInfo = itemIconComponent.CachedHeroInfo;
			ShowEquipInfo(cachedHeroInfo, equipItemInfo, isFromEquipListPanel);
		}
	}

	private void ShowEquipInfo(CachedHeroInfo cachedHeroInfo, EquipItemInfo equipItemInfo, bool isFromEquipListPanel = false)
	{
		if (equipItemInfo == null)
		{
			return;
		}
		InvenEquipInfoUIContainer invenEquipUIContainer = GetInstance<InvenEquipInfoUIContainer>();
		if (invenEquipUIContainer == null)
		{
			return;
		}
		bool flag = IsEquipped(equipItemInfo);
		if (GetEquipPartType(equipItemInfo) == EquipPartType.Treasure)
		{
			if (flag)
			{
				if (equipItemInfo.GetEquippedHeroIndex() == _currentHero.CreatureData.Index)
				{
					Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask().ADDTASK(base.TaskFactory.Run(delegate
					{
						OpenHeroEquipTreasure(equipItemInfo, cachedHeroInfo);
					})), delegate
					{
						if (isFromEquipListPanel)
						{
							SetSelectedItemIcon(equipItemInfo);
							invenEquipUIContainer.Open(InvenEquipInfoUIContainer.WindowPosType.Center, _currentHero, equipItemInfo, this, null);
						}
					}).WrapperTouchControl().DisableTouch()
						.Commit();
				}
				else
				{
					SetSelectedItemIcon(equipItemInfo);
					invenEquipUIContainer.Open(InvenEquipInfoUIContainer.WindowPosType.Center, _currentHero, equipItemInfo, this, null);
				}
				return;
			}
			if (!equipItemInfo.IsUniqueTreasure)
			{
				SetSelectedItemIcon(equipItemInfo);
				HeroList heroIterator = GetInstance<HeroList>();
				invenEquipUIContainer.Open(InvenEquipInfoUIContainer.WindowPosType.Right, _currentHero, equipItemInfo, this, heroIterator);
				return;
			}
		}
		if (flag)
		{
			SetSelectedItemIcon(equipItemInfo);
			invenEquipUIContainer.Open(InvenEquipInfoUIContainer.WindowPosType.Center, _currentHero, equipItemInfo, this, null);
		}
		else
		{
			SetSelectedItemIcon(equipItemInfo);
			HeroList heroIterator2 = GetInstance<HeroList>();
			invenEquipUIContainer.Open(InvenEquipInfoUIContainer.WindowPosType.Right, _currentHero, equipItemInfo, this, heroIterator2);
		}
		InvenEquipInfo invenEquipInfo = invenEquipUIContainer.GetInvenEquipInfo(InvenEquipInfoUIContainer.WindowPosType.Center);
		InvenEquipInfo invenEquipInfo2 = invenEquipUIContainer.GetInvenEquipInfo(InvenEquipInfoUIContainer.WindowPosType.Right);
		if (invenEquipUIContainer.IsOpenInvenEquipInfo(InvenEquipInfoUIContainer.WindowPosType.Center))
		{
			invenEquipInfo.SetupStat(invenEquipUIContainer.IsOpenInvenEquipInfo(InvenEquipInfoUIContainer.WindowPosType.Right) ? invenEquipInfo2.EquipItemInfo : null);
		}
		if (invenEquipUIContainer.IsOpenInvenEquipInfo(InvenEquipInfoUIContainer.WindowPosType.Right))
		{
			invenEquipInfo2.SetupStat(invenEquipUIContainer.IsOpenInvenEquipInfo(InvenEquipInfoUIContainer.WindowPosType.Center) ? invenEquipInfo.EquipItemInfo : null);
		}
		CloseInstance<RecommendEquipmentContainer>();
	}

	public void RefreshHeroEquipList()
	{
		if (!UIHelper.IsEnabledGameObjet(GetHeroEquipList()))
		{
			GetHeroEquipList().Open(_currentHero, _lastEquipPartType, this);
		}
		else if (CheckCompareHeroByHeroEquipList(_currentHero))
		{
			GetHeroEquipList().Open(_currentHero, _lastEquipPartType, this);
		}
		else
		{
			GetHeroEquipList().Refresh(immediate: true, refreshFilter: false);
		}
	}

	private void SetSelectedItemIcon(EquipItemInfo equipItemInfo, bool isSelected = true)
	{
		if (equipItemInfo != null)
		{
			HeroList heroList = GetInstance<HeroList>();
			if (!(heroList == null) && !(heroList.HeroEquipListPanel == null) && heroList.HeroEquipListPanel.equipListManager != null)
			{
				string selected = (isSelected ? equipItemInfo.Uid : null);
				heroList.HeroEquipListPanel.equipListManager.SetUseSelectedBoarder(isSelected);
				heroList.HeroEquipListPanel.equipListManager.SetSelected(selected);
			}
		}
	}

	private void OpenHeroEquipTreasure(EquipItemInfo equipItemInfo, CachedHeroInfo cachedHeroInfo, bool selectSlotState = false)
	{
		GetInstance<HeroEquipTreasure>().Open(this, equipItemInfo, cachedHeroInfo, selectSlotState);
		CloseInstance<RecommendEquipmentContainer>();
	}

	private void RefreshHeroEquipTreasure(CachedHeroInfo currentHeroInfo)
	{
		HeroEquipTreasure heroEquipTreasure = GetInstance<HeroEquipTreasure>();
		if (heroEquipTreasure.IsOpen())
		{
			heroEquipTreasure.Refresh(currentHeroInfo);
		}
	}

	private bool CheckCompareHeroByHeroEquipList(CachedHeroInfo cachedHeroInfo)
	{
		CachedHeroInfo cachedHeroInfo2 = GetHeroEquipList().CachedHeroInfo;
		if (cachedHeroInfo2 != null)
		{
			return cachedHeroInfo2.HeroIndex != cachedHeroInfo.HeroIndex;
		}
		return true;
	}

	private EquipPartType GetEquipPartType(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			return EquipPartType.None;
		}
		return GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex)?.SubData.PartType ?? EquipPartType.None;
	}

	void HeroDeck2.Listener.OnClickHero(CachedHeroInfo heroInfo)
	{
		OpenMyHeroAsync(heroInfo, Hero3DViewPanel.EToggle.Info, Hero3DViewPanel.ENextTask.None, EquipPartType.Weapon, immediately: false, fromInventory: false, enableBookmark: true).WrapperTouchControl().DisableTouch().Commit();
	}

	void HeroDeck2.Listener.OnClickHero(CreatureData creatureData)
	{
	}

	bool HeroDeck2.Listener.OnCloseRequest(HeroDeck2 closeThis)
	{
		return true;
	}

	void HeroDeck2.Listener.GotoHeroShop()
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_HERO_SHOP_CONFIRM);
		MessagePopup.Open(title, message, delegate
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Hero.ToString());
		}, null);
	}

	void HeroDeck2.Listener.ViewDetail(int heroIndex)
	{
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
		if (cachedHeroInfo == null)
		{
			cachedHeroInfo = SceneAutoBehaviour<StatDataCache>.instance.GetReferenceHero(heroIndex);
		}
		GetInstance<HeroDetailView>().Open(cachedHeroInfo, this);
	}

	void HeroDeck2.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	private void MoveToOrdealOrPurifyRoomShortCut(CachedHeroInfo heroInfo, HeroStatPanel.AwakeStateType awakeState, bool isMaxStar)
	{
		if (GlobalAutoBehaviour<PartyManager>.isValidInstance && GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
		{
			GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
		}
		if (awakeState == HeroStatPanel.AwakeStateType.EnoughMaterial || awakeState == HeroStatPanel.AwakeStateType.InDungeon)
		{
			SceneAutoBehaviour<HeroTrialDungeonManagement>.instance.OpenHeroEnterDungeonShortCut(heroInfo);
			return;
		}
		if (awakeState == HeroStatPanel.AwakeStateType.Purifying)
		{
			SceneAutoBehaviour<HeroTrialDungeonManagement>.instance.OpenPurifyHeroRoom(heroInfo, isShortCut: true);
			return;
		}
		HeroAwakeType heroAwakeType = (isMaxStar ? HeroAwakeType.Transcend : HeroAwakeType.Awaken);
		SceneAutoBehaviour<HeroTrialDungeonManagement>.instance.OpenPurifyHeroSelectWindow(heroAwakeType);
	}

	private void MoveToTemple(bool isOrdeal, bool isMaxStar)
	{
		GlobalAutoBehaviour<GlobalManagement>.instance.CloseAllWindows();
		Type actionType = (isOrdeal ? ((!isMaxStar) ? typeof(EnterClassTrialSelect) : typeof(EnterClassTrialSelectTranscend)) : ((!isMaxStar) ? typeof(EnterPurifyRoom) : typeof(EnterPurifyRoomTranscend)));
		SceneAutoBehaviour<ActivityManager>.instance.Run(new MoveToFunctionNode(NodeFunctionType.Temple, actionType, !isMaxStar));
	}

	private void TryOrdealOrPurify(CachedHeroInfo heroInfo, HeroStatPanel.AwakeStateType awakeState, bool isShortCut)
	{
		if (heroInfo == null)
		{
			return;
		}
		List<CampaignNodeData> allDataOfFunctionType = GameRoot.Instance.CampaignNodeDataContainer.GetAllDataOfFunctionType(NodeFunctionType.Temple);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allDataOfFunctionType))
		{
			return;
		}
		bool isOrdeal = awakeState == HeroStatPanel.AwakeStateType.EnoughMaterial || awakeState == HeroStatPanel.AwakeStateType.InDungeon;
		bool isMaxStar = heroInfo.IsMaxStar();
		int chapterIndex = allDataOfFunctionType[0].ChapterIndex;
		int dungeonIndex = allDataOfFunctionType[0].DungeonIndex;
		if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(chapterIndex) || !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(chapterIndex, dungeonIndex))
		{
			int num = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.FindPrevBattleNode(chapterIndex, dungeonIndex);
			if (num == NGame2.NAccount.UserCampaignManager.InvalidDungeonIndex)
			{
				CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
				return;
			}
			CenterMessage.Open(LocalizationManager.GetString(NodeFunction.CURRENT_NODE_FUNCTION_LOCKED, chapterIndex, num));
			return;
		}
		Hero hero = ((!isOrdeal) ? (isMaxStar ? Hero.HERO_NOTIFY_TRANSCEND_PURIFY : Hero.HERO_NOTIFY_PURIFY) : Hero.HERO_NOTIFY_ORDEAL);
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(hero);
		MessagePopup.Open(title, message, delegate
		{
			if (isShortCut)
			{
				MoveToOrdealOrPurifyRoomShortCut(heroInfo, awakeState, isMaxStar);
			}
			else
			{
				MoveToTemple(isOrdeal, isMaxStar);
			}
		}, null, NShared.NLocalization.Common.COMMON_MOVE, NShared.NLocalization.Common.COMMON_CANCEL);
	}

	bool HeroStatPanel.Listener.OnCloseRequest(HeroStatPanel closeThis)
	{
		ReturnToDeck();
		return false;
	}

	void HeroStatPanel.Listener.OnClickShortCut(CachedHeroInfo heroInfo, HeroStatPanel.AwakeStateType awakeState)
	{
		TryOrdealOrPurify(heroInfo, awakeState, isShortCut: true);
	}

	void HeroStatPanel.Listener.OnClickTranscend(CachedHeroInfo heroInfo, HeroStatPanel.AwakeStateType awakeState)
	{
		TryOrdealOrPurify(heroInfo, awakeState, isShortCut: true);
	}

	void HeroStatPanel.Listener.OnClickLimitBreakLevelUp(CachedHeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			VespaDebug.LogError("LimitBreak error. heroInfo is null");
			return;
		}
		HeroLimitBreakGate window = GetInstance<HeroLimitBreakGate>();
		if (window == null)
		{
			VespaDebug.LogError("LimitBreak error. HeroLimitBreakGate is null");
			return;
		}
		HeroLimitBreakData data = GameRoot.Instance.HeroLimitBreakDataContainer.GetData(new HeroLimitBreakDataKey(heroInfo.HeroIndex, heroInfo.LimitBreakLevel + 1));
		if (data == null)
		{
			VespaDebug.LogError("LimitBreak error. nextLimitBreakData is null. HeroIndex=" + heroInfo.HeroIndex + " LimitBreakLevel=" + heroInfo.LimitBreakLevel);
			return;
		}
		int heroIndex = heroInfo.HeroIndex;
		HeroLimitBreakGate.OpenData openData = new HeroLimitBreakGate.OpenData();
		openData.HeroInfo = heroInfo;
		openData.ReqItemCounts = data.GetMaterialItems();
		openData.OnOpenGate = delegate
		{
			NShared.HeroLimitBreakLevelUp.Request request = new NShared.HeroLimitBreakLevelUp.Request
			{
				HeroIndex = heroIndex
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.HeroLimitBreakLevelUp.Response response)
			{
				window.Close();
				if (response != null)
				{
					if (response.BaseResult != BaseResultType.Success)
					{
						MessagePopup.OpenError(response.BaseResult);
					}
					else if (response.Result != NShared.HeroLimitBreakLevelUp.ResultType.Success)
					{
						MessagePopup.OpenError(response.Result);
					}
					else
					{
						GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ApplyHeroLimitBreakLevel(heroIndex, response.NewHeroLimitBreakLevel);
						GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
						GetNewHero2.OpenData openData2 = GetNewHero2.OpenData.CreateSingleFromHeroInfo(heroInfo, GetNewHero2.EOpenMode.LimitBreak, hideInfo: false);
						SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<GetNewHero2>().Open(openData2, null, null);
					}
				}
			});
		};
		window.Open(openData);
	}

	void HeroStatPanel.Listener.OnClickLimitBreakExpUp(CachedHeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			VespaDebug.LogError("LimitBreak error. heroInfo is null");
			return;
		}
		HeroLimitBreakExpItemUsePopup window = GetInstance<HeroLimitBreakExpItemUsePopup>();
		if (window == null)
		{
			VespaDebug.LogError("LimitBreak error. HeroLimitBreakExpItemUsePopup is null");
			return;
		}
		List<HeroLimitBreakExpItemUsePopup.OpenData.InputItemData> list = new List<HeroLimitBreakExpItemUsePopup.OpenData.InputItemData>();
		int heroIndex = heroInfo.HeroIndex;
		short heroLevel = heroInfo.Level;
		sbyte limitBreakLevel = heroInfo.LimitBreakLevel;
		HeroLimitBreakData data = GameRoot.Instance.HeroLimitBreakDataContainer.GetData(new HeroLimitBreakDataKey(heroIndex, limitBreakLevel));
		if (data == null)
		{
			VespaDebug.LogError("LimitBreak error. limitBreakData is null. heroIndex=" + heroIndex + " limitBreakLevel=" + limitBreakLevel);
			return;
		}
		IList<ItemData> itemDataListByType = GameRoot.Instance.ItemDataContainer.GetItemDataListByType(ItemType.HeroLimitBreakExp);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(itemDataListByType))
		{
			VespaDebug.LogError("LimitBreak error. expItemDataList is empty. heroIndex=" + heroIndex + " limitBreakLevel=" + limitBreakLevel);
			return;
		}
		foreach (ItemData item in itemDataListByType)
		{
			int index = item.Index;
			int itemCount = Helper.GetItemCount(index);
			HeroLimitBreakExpItemData data2 = GameRoot.Instance.HeroLimitBreakExpItemDataContainer.GetData(item.Code);
			if (data2 == null)
			{
				VespaDebug.LogError("LimitBreak error. expItemSubData is null. materialItemIndex=" + index);
				continue;
			}
			list.Add(new HeroLimitBreakExpItemUsePopup.OpenData.InputItemData
			{
				ItemIndex = index,
				MaxCount = itemCount,
				AddExp = data2.ExpAmount
			});
		}
		HeroLimitBreakExpItemUsePopup.OpenData openData = new HeroLimitBreakExpItemUsePopup.OpenData();
		openData.InputItemDatas = list.ToArray();
		openData.CurrentExp = heroInfo.LimitBreakExp;
		openData.TotalExp = data.ReqLocalExp;
		openData.OnUse = delegate(Dictionary<int, int> itemCounts)
		{
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(itemCounts))
			{
				List<int> list2 = new List<int>();
				List<int> list3 = new List<int>();
				foreach (KeyValuePair<int, int> itemCount2 in itemCounts)
				{
					list2.Add(itemCount2.Key);
					list3.Add(itemCount2.Value);
				}
				NShared.HeroLimitBreakExpUp.Request request = new NShared.HeroLimitBreakExpUp.Request
				{
					HeroIndex = heroIndex,
					ExpItemIndices = list2.ToArray(),
					ExpItemCounts = list3.ToArray()
				};
				SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.HeroLimitBreakExpUp.Response response)
				{
					window.Close();
					if (response != null)
					{
						if (response.BaseResult != BaseResultType.Success)
						{
							MessagePopup.OpenError(response.BaseResult);
						}
						else if (response.Result != NShared.HeroLimitBreakExpUp.ResultType.Success)
						{
							MessagePopup.OpenError(response.Result);
						}
						else
						{
							GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ApplyHeroLevel(heroIndex, response.NewHeroLevel);
							GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ApplyHeroLimitBreakExp(heroIndex, response.NewHeroLimitBreakExp);
							GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
							if (response.NewHeroLevel > heroLevel)
							{
								((HeroEquipInfo.IHeroIterator)GetInstance<HeroList>(openedOnly: true))?.Visit((Action<HeroPortraitComponent>)delegate(HeroPortraitComponent heroPortraitComponent)
								{
									if (heroPortraitComponent.HeroInfo != null && heroPortraitComponent.HeroInfo.HeroIndex == heroIndex)
									{
										heroPortraitComponent.PlayLevelUpEffect(play: true);
									}
								});
								CenterMessage.Open(LocalizationManager.GetStringDirect("LIMITBREAK_LEVELUP_INFO", response.NewHeroLevel));
							}
							else
							{
								CenterMessage.Open(LocalizationManager.GetStringDirect("LIMITBREAK_EXPUP_INFO"));
							}
						}
					}
				});
			}
		};
		window.Open(openData);
	}

	private bool IsEnableCraft(string CraftItemCode)
	{
		IList<CraftItemData> categoryData = GameRoot.Instance.CraftItemDataContainer.GetCategoryData(CraftItemCode);
		if (categoryData == null)
		{
			return false;
		}
		foreach (CraftItemData item in categoryData)
		{
			foreach (CraftItemData.MaterialItemInfo materialItem in item.GetMaterialItemList())
			{
				if (materialItem != null && GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(materialItem.ItemCode) < materialItem.ItemCount)
				{
					return false;
				}
			}
		}
		return true;
	}

	void HeroStatPanel.Listener.OnClickCraftItem(ItemIconComponent This)
	{
		if (!(This == null) || This.ItemData != null)
		{
			IsEnableCraft(This.ItemData.Code);
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string empty = string.Empty;
			int reqSelectedItemCount = 0;
			CreatureStarData nextData = GameRoot.Instance.CreatureStarDataContainer.GetNextData(_currentHero.Star, _currentHero.Transcended);
			int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(This.ItemData.Code);
			int num = 0;
			if (This.ItemData.Type == ItemType.AwakeChallenge)
			{
				num = nextData?.HeroAwakeItemCount ?? 0;
			}
			else if (This.ItemData.Type == ItemType.HeroEnhanceMaterial)
			{
				num = nextData?.HeroTranscendItemCount ?? 0;
			}
			HeroStatPanel.AwakeStateType awakeState = GetHeroStat().GetAwakeState();
			if (awakeState != HeroStatPanel.AwakeStateType.InDungeon && awakeState != HeroStatPanel.AwakeStateType.Purifying)
			{
				reqSelectedItemCount = num - itemCount;
			}
			empty = ((reqSelectedItemCount > 0) ? LocalizationManager.GetString(Inventory.INVENTORY_ENOUGH_CRAFT_MSG_NEW, This.ItemData.Name, reqSelectedItemCount) : LocalizationManager.GetString(Inventory.INVENTORY_ENOUGH_CRAFT_MSG));
			MessagePopup.Open(title, empty, delegate
			{
				SceneAutoBehaviour<InventoryManagement>.instance.OpenShortCutCraftWindow(This.ItemData, reqSelectedItemCount);
			}, null, Inventory.INVENTORY_CRAFT_MOVE_MSG, NShared.NLocalization.Common.COMMON_CANCEL);
		}
	}

	void Hero3DViewPanel.Listener.OnToggle(Hero3DViewPanel.EToggle toggle)
	{
		if (_currentHero == null)
		{
			return;
		}
		if (_lastViewToggle != toggle)
		{
			ResetScroll();
		}
		if (toggle != Hero3DViewPanel.EToggle.Info)
		{
			GetHeroStat().Close();
		}
		if (toggle != Hero3DViewPanel.EToggle.Equip)
		{
			GetHeroEquipList().Close();
		}
		if (toggle != Hero3DViewPanel.EToggle.Skill)
		{
			GetHeroSkillList().Close();
		}
		if (toggle != Hero3DViewPanel.EToggle.Attribute)
		{
			GetHeroTranscendSkillList().Close();
		}
		if (toggle != Hero3DViewPanel.EToggle.EquipStorage)
		{
			GetHeroEquipStorageList().Close();
		}
		if (toggle != Hero3DViewPanel.EToggle.Profile)
		{
			GetHeroProfile().Close();
		}
		switch (toggle)
		{
		case Hero3DViewPanel.EToggle.Info:
			CloseBut(GetInstance<HeroList>(), GetInstance<HeroDeck2>());
			GetHeroStat().Open(_currentHero, this);
			break;
		case Hero3DViewPanel.EToggle.Equip:
			CloseBut(GetInstance<HeroList>(), GetInstance<HeroDeck2>(), GetInstance<HeroEquipTreasure>(), GetInstance<InvenEquipInfoUIContainer>());
			RefreshHeroEquipList();
			RefreshHeroEquipTreasure(_currentHero);
			break;
		case Hero3DViewPanel.EToggle.Skill:
			CloseBut(GetInstance<HeroList>(), GetInstance<HeroDeck2>());
			GetHeroSkillList().Open(_currentHero, this);
			break;
		case Hero3DViewPanel.EToggle.Attribute:
			CloseBut(GetInstance<HeroList>(), GetInstance<HeroDeck2>());
			GetHeroTranscendSkillList().Open(_currentHero, this);
			break;
		case Hero3DViewPanel.EToggle.EquipStorage:
		{
			CloseBut(GetInstance<HeroList>(), GetInstance<HeroDeck2>());
			HeroEquipStoragePanel heroEquipStorageList = GetHeroEquipStorageList();
			if (heroEquipStorageList.gameObject.activeSelf)
			{
				int equippedItemExsitSlotIndex = PresetStorageHelper.GetEquippedItemExsitSlotIndex(_currentHero.HeroIndex);
				EquipPresetInfo equipPresetInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.Get(equippedItemExsitSlotIndex);
				EquipPresetInfo selectedEquipPresetInfo = heroEquipStorageList.SelectedEquipPresetInfo;
				if (equippedItemExsitSlotIndex == -1 && selectedEquipPresetInfo == null)
				{
					heroEquipStorageList.SelectPresetStorage(1);
					heroEquipStorageList.UpdateEquipPresetSlotComponent();
				}
				else if (equippedItemExsitSlotIndex != -1 && equipPresetInfo != null)
				{
					heroEquipStorageList.SelectPresetStorage(equipPresetInfo);
					heroEquipStorageList.UpdateEquipPresetSlotComponent();
				}
				else
				{
					heroEquipStorageList.UpdateEquipItems();
					heroEquipStorageList.UpdateButtons();
					heroEquipStorageList.UpdateEquipPresetSlotComponent();
				}
			}
			else
			{
				heroEquipStorageList.Open(this, this);
			}
			break;
		}
		case Hero3DViewPanel.EToggle.Profile:
			CloseBut(GetInstance<HeroList>(), GetInstance<HeroDeck2>());
			GetHeroProfile().Open(_currentHero, this, isEnableButtonInfo: true);
			break;
		default:
			return;
		}
		_lastViewToggle = toggle;
	}

	void Hero3DViewPanel.Listener.OnClickEquipIcon(ItemIconComponent This)
	{
		if (This == null || _currentHero == null)
		{
			return;
		}
		int heroPartEquipSlotIndex = GetHero3DView().GetHeroPartEquipSlotIndex(This);
		if (heroPartEquipSlotIndex == 0)
		{
			return;
		}
		if (This.EquipItemInfo != null)
		{
			EquipPartType equipPartTypeFor = HeroInfo.GetEquipPartTypeFor(heroPartEquipSlotIndex);
			OpenEquipListOfType(equipPartTypeFor);
			ShowEquipInfo(This);
			return;
		}
		int heroIndex = _currentHero.HeroIndex;
		if (GlobalAutoBehaviour<AccountManager>.instance.IsEquipSlotLocked(heroIndex, heroPartEquipSlotIndex))
		{
			AskUnlockEquipSlotAsync(heroIndex, heroPartEquipSlotIndex);
			return;
		}
		EquipPartType equipPartTypeFor2 = HeroInfo.GetEquipPartTypeFor(heroPartEquipSlotIndex);
		OpenEquipListOfType(equipPartTypeFor2);
		if (equipPartTypeFor2 == EquipPartType.Treasure)
		{
			OpenHeroEquipTreasure(null, This.CachedHeroInfo);
		}
	}

	void Hero3DViewPanel.Listener.OnClickClassTagIcon(ClassTagIconComponent This)
	{
	}

	void Hero3DViewPanel.Listener.OnClickDetailView()
	{
		if (_currentHero != null)
		{
			HeroList heroList = GetInstance<HeroList>(openedOnly: true);
			if (heroList != null)
			{
				heroList.OnOpenDetaliView();
			}
			GetInstance<HeroDetailView>().Open(_currentHero, this);
		}
	}

	void Hero3DViewPanel.Listener.OnClickAutoEquip()
	{
		ProcessAutoEquip();
	}

	void Hero3DViewPanel.Listener.OnClickSelectFlask(Hero3DViewPanel caller)
	{
		HeroGetExpComponent.ExpStyle expStyle = HeroGetExpComponent.ExpStyle.HeroExp;
		expStyle = ((caller.CachedHeroInfo.FlaskItemIndex == 0) ? HeroGetExpComponent.ExpStyle.HeroExp : HeroGetExpComponent.ExpStyle.FlaskExp);
		InvenUseHeroExpPotion.Open(expStyle, 0, caller.CachedHeroInfo.HeroIndex);
	}

	void Hero3DViewPanel.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
		switch (notifyInfo)
		{
		case HeroNotifyComponent.ENotifyInfo.CanSkillExtend:
			OpenMyHero(heroInfo, Hero3DViewPanel.EToggle.Skill);
			break;
		case HeroNotifyComponent.ENotifyInfo.CanEquipChange:
			OpenMyHero(heroInfo, Hero3DViewPanel.EToggle.Equip);
			break;
		case HeroNotifyComponent.ENotifyInfo.CanLimitBreak:
			OpenMyHero(heroInfo);
			break;
		}
	}

	void Hero3DViewPanel.Listener.OnClickShowUniqueWeaponOn(Hero3DViewPanel caller)
	{
		ProcHideHeroUniqueWeapon(caller.CachedHeroInfo, 0);
	}

	void Hero3DViewPanel.Listener.OnClickShowUniqueWeaponOff(Hero3DViewPanel caller)
	{
		ProcHideHeroUniqueWeapon(caller.CachedHeroInfo, 1);
	}

	void Hero3DViewPanel.Listener.OnClickProfile(int heroIndex)
	{
	}

	private void ProcHideHeroUniqueWeapon(CachedHeroInfo heroInfo, byte hideUniqueWeapon)
	{
		if (WebServiceRequest.IsProgressing(_currentServiceRequest) || heroInfo == null)
		{
			return;
		}
		if (heroInfo.HideUniqueWeapon == hideUniqueWeapon)
		{
			VespaDebug.LogError("HideUniqueWeapon On/Off is same. buggy situation");
			return;
		}
		NShared.SetHideHeroUniqueWeapon.Request request = new NShared.SetHideHeroUniqueWeapon.Request
		{
			HeroIndex = heroInfo.HeroIndex,
			HideUniqueWeapon = hideUniqueWeapon
		};
		_currentServiceRequest = SingletonUpdater<WebServiceRequester>.instance.Request(request, delegate(NShared.SetHideHeroUniqueWeapon.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.SetHideHeroUniqueWeapon.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetHideUniqueWeapon(heroInfo.HeroIndex, hideUniqueWeapon);
					CenterMessage.Open(LocalizationManager.GetString((hideUniqueWeapon == 0) ? Costume.COSTUME_UNIQUEWEAPON_ON : Costume.COSTUME_UNIQUEWEAPON_OFF));
				}
			}
		});
	}

	void Hero3DViewPanel.Listener.OnClickCostume(int heroIndex)
	{
		SceneAutoBehaviour<CostumeManagement>.instance.OpenCostumeMySet(heroIndex);
	}

	void Hero3DViewPanel.Listener.OnClickPresetStorage()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.Count() > 0)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CONFIRM);
			string stringDirect = LocalizationManager.GetStringDirect("EQUIP_PRESET_NOT_AVAILABLE");
			MessagePopup.Open(title, stringDirect);
			return;
		}
		HeroTranscendSkillListPanel heroTranscendSkillList = GetHeroTranscendSkillList();
		if (heroTranscendSkillList != null && heroTranscendSkillList.gameObject.activeSelf)
		{
			heroTranscendSkillList.ShowRemainTranscendPointAlarm();
			heroTranscendSkillList.SendPrevSelectedTranscendPage(reset: false);
		}
		HeroPresetStorageWindow heroPresetStorageWindow = GetInstance<HeroPresetStorageWindow>();
		if (!(heroPresetStorageWindow == null) && !heroPresetStorageWindow.IsOpen())
		{
			heroPresetStorageWindow.Open(this);
		}
	}

	void HeroDetailView.Listener.OnTryBattle(HeroDetailView sender)
	{
		if (HeroDetailView.TryBattle(sender.HeroInfo))
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(sender.HeroInfo.HeroIndex);
			LobbyScene2.ListReturnState.Add(new MyHeroTryBattleReturnState((cachedHeroInfo != null) ? sender.HeroInfo : null));
		}
	}

	void HeroFullSceneViewBase.Listener.OnOpen()
	{
		if (UIHelper.IsEnabledGameObjet(GetHero3DView()))
		{
			GetHero3DView().ActivatePreview(isActive: false);
		}
	}

	void HeroFullSceneViewBase.Listener.OnClose()
	{
		if (UIHelper.IsEnabledGameObjet(GetHero3DView()))
		{
			GetHero3DView().ActivatePreview(isActive: true);
		}
	}

	private void OnClickItemIcon(ItemIconComponent This)
	{
		if (!(This == null))
		{
			ItemData itemData = This.ItemData;
			if (itemData != null && itemData.Type == ItemType.Equip)
			{
				ShowEquipInfo(This, isFromEquipListPanel: true);
			}
		}
	}

	private void OnClickExtended()
	{
		int inventoryExtend = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.InventoryExtend;
		InventoryExtendData nextDataByExtendCount = GameRoot.Instance.InventoryExtendDataContainer.GetNextDataByExtendCount(inventoryExtend);
		int equipItemExtendSize = nextDataByExtendCount.EquipItemExtendSize;
		int extendPrice = nextDataByExtendCount.EquipItemExtendPrice;
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(Inventory.EQUIP_ASK_EXTEND_MESSAGE, equipItemExtendSize);
		string action = LocalizationManager.GetString(Inventory.EQUIP_EXTEND_ACTION);
		PricedMessagePopup.Open(title, message, action, PricedMessagePopup.EPriceType.Gem, extendPrice, delegate
		{
			NShared.ExtendEquip.Request request = new NShared.ExtendEquip.Request
			{
				ReqGem = extendPrice
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ExtendEquip.Response response)
			{
				if (response != null)
				{
					if (response.BaseResult != BaseResultType.Success)
					{
						MessagePopup.OpenError(response.BaseResult);
					}
					else if (response.Result != NShared.ExtendEquip.ResultType.Success)
					{
						MessagePopup.OpenError(response.Result);
					}
					else
					{
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetInventoryExtend(response.NewInventoryExtend);
						GetHeroEquipList().Refresh();
					}
				}
			});
		});
	}

	private bool NewItemExist(ItemType[] ItemTypes, EquipPartType[] partTypes, bool isInChestBox)
	{
		foreach (ItemType itemType in ItemTypes)
		{
			if (itemType == ItemType.Equip)
			{
				bool flag = false;
				if (partTypes == null)
				{
					flag = true;
				}
				else if (partTypes.Length == 0)
				{
					flag = true;
				}
				if (flag)
				{
					if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.AddedItemExists(itemType))
					{
						return true;
					}
					continue;
				}
				for (int j = 0; j < partTypes.Length; j++)
				{
					if (GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.IsNewEquipByParTypeWithInventoryMode(partTypes[j], isInChestBox))
					{
						return true;
					}
				}
			}
			else if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.AddedItemExists(itemType))
			{
				return true;
			}
		}
		return false;
	}

	void HeroEquipListPanel.Listener.OnClickItemIcon(ItemIconComponent This)
	{
		OnClickItemIcon(This);
	}

	void HeroEquipListPanel.Listener.OnEquipPartChanged(EquipPartType equipPartType)
	{
		_lastEquipPartType = equipPartType;
		_selectedEquipPartType = equipPartType;
	}

	void HeroEquipListPanel.Listener.OnAutoEquip(HeroEquipListPanel caller)
	{
		ProcessAutoEquip();
	}

	void HeroEquipListPanel.Listener.OnAutoUnequip(HeroEquipListPanel caller)
	{
		ProcessAutoUnequipAsync().Commit();
	}

	bool HeroEquipListPanel.Listener.NewItemExist(ItemType itemType, EquipPartType partType)
	{
		if (itemType == ItemType.Equip)
		{
			return GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.DoesExistAddedEquipItemByType(_currentHero, partType);
		}
		return GlobalAutoBehaviour<AccountManager>.instance.ItemManager.AddedItemExists(itemType);
	}

	void HeroEquipListPanel.Listener.OnClickOpenRecommendEquipPopup()
	{
		RecommendEquipmentContainer recommendEquipmentContainer = GetInstance<RecommendEquipmentContainer>();
		if (!(recommendEquipmentContainer == null))
		{
			if (recommendEquipmentContainer.IsOpen())
			{
				recommendEquipmentContainer.Close();
			}
			else
			{
				recommendEquipmentContainer.Open(_currentHero);
			}
		}
	}

	private bool IsEquipped(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.GetHeroOfEquipItem(equipItemInfo.SlotIndex) != null;
	}

	bool HeroEquipInfo.Listener.OnCloseRequest(HeroEquipInfo caller)
	{
		EquipItemInfo equipItemInfo = caller.EquipItemInfo;
		if (equipItemInfo != null)
		{
			InvenEquipInfoUIContainer invenEquipInfoUIContainer = GetInstance<InvenEquipInfoUIContainer>();
			if (invenEquipInfoUIContainer != null)
			{
				bool flag = IsEquipped(equipItemInfo);
				if (flag)
				{
					InvenEquipInfo invenEquipInfo = invenEquipInfoUIContainer.GetInvenEquipInfo(InvenEquipInfoUIContainer.WindowPosType.Right);
					if (invenEquipInfo != null && invenEquipInfo.IsOpen())
					{
						invenEquipInfo.SetupStat();
					}
					else
					{
						invenEquipInfoUIContainer.OnClickClose();
					}
				}
				else if (!flag)
				{
					InvenEquipInfo invenEquipInfo2 = invenEquipInfoUIContainer.GetInvenEquipInfo(InvenEquipInfoUIContainer.WindowPosType.Center);
					if (invenEquipInfo2 != null && invenEquipInfo2.IsOpen())
					{
						invenEquipInfo2.SetupStat();
					}
					else
					{
						invenEquipInfoUIContainer.OnClickClose();
					}
				}
			}
		}
		return true;
	}

	void HeroEquipInfo.Listener.OnAwaken(HeroEquipInfo caller)
	{
	}

	void HeroEquipInfo.Listener.OnEquip(HeroEquipInfo caller)
	{
		EquipItemInfo equipItemInfo = caller.EquipItemInfo;
		if (equipItemInfo == null)
		{
			return;
		}
		int slotIndex = equipItemInfo.SlotIndex;
		_ = caller.ItemData.Name;
		CachedHeroInfo cachedHeroInfo = caller.CachedHeroInfo;
		if (cachedHeroInfo == null)
		{
			return;
		}
		EquipItemData equipItemData = caller.EquipItemData;
		if (equipItemData == null)
		{
			return;
		}
		if (DispatchHelper.IsDispatchedHero(cachedHeroInfo))
		{
			CenterMessage centerMessage = CenterMessage.Open(LocalizationManager.GetString(DispatchBattle.DISPATCH_EQUIP_REPLACE_MSG));
			if (centerMessage != null)
			{
				centerMessage.SetColor(Color.green);
			}
		}
		else if (equipItemData.PartType == EquipPartType.Treasure)
		{
			OpenHeroEquipTreasure(caller.EquipItemInfo, caller.CachedHeroInfo, selectSlotState: true);
		}
		else
		{
			int heroIndex = cachedHeroInfo.HeroIndex;
			RequestSetEquip(slotIndex, heroIndex, equipItemData.PartType.GetEquipSlotIndex());
		}
	}

	void HeroEquipInfo.Listener.OnUnequip(HeroEquipInfo caller)
	{
		EquipItemInfo equipItemInfo = caller.EquipItemInfo;
		if (equipItemInfo == null)
		{
			return;
		}
		int slotIndex = equipItemInfo.SlotIndex;
		CachedHeroInfo heroOfEquipItem = GlobalAutoBehaviour<AccountManager>.instance.GetHeroOfEquipItem(slotIndex);
		if (heroOfEquipItem == null)
		{
			MessagePopup.OpenError(LocalizationManager.GetString(Equip.EQUIP_ERROR_NOT_EQUIPPED));
		}
		else if (DispatchHelper.IsDispatchedHero(heroOfEquipItem))
		{
			CenterMessage centerMessage = CenterMessage.Open(LocalizationManager.GetString(DispatchBattle.DISPATCH_EQUIP_REMOVE_MSG));
			if (centerMessage != null)
			{
				centerMessage.SetColor(Color.green);
			}
		}
		else
		{
			RequestUnsetEquipAsync(slotIndex, heroOfEquipItem).Commit();
		}
	}

	void HeroEquipInfo.Listener.OnSell(HeroEquipInfo closeThis)
	{
		InventoryManagement.ProcessSellEquip(new EquipItemInfo[1] { closeThis.EquipItemInfo }, delegate(bool success)
		{
			if (success)
			{
				closeThis.Close();
			}
		});
	}

	void HeroEquipInfo.Listener.OnLock(HeroEquipInfo closeThis, byte Locked)
	{
		InventoryManagement.ProcessEquipLock(closeThis.EquipItemInfo, Locked);
	}

	private void UpdateStat()
	{
		HeroStatPanel heroStat = GetHeroStat();
		if (!(heroStat == null))
		{
			heroStat.SetupStat();
			heroStat.SetupOptions();
		}
	}

	void HeroRunehengeWindow.IListener.RequestEquipRune(HeroRunehengeWindow caller, int heroIndex, int itemIndex, int equipItemSlotIndex, int preserveEquipItemSlotIndex, RuneSlotType slot, int runePage, bool isPreserve, Action onCompelete)
	{
		NShared.EquipRune.Request request = new NShared.EquipRune.Request
		{
			HeroIndex = heroIndex,
			ItemIndex = itemIndex,
			EquipItemSlotIndex = equipItemSlotIndex,
			SlotNum = (int)slot,
			RunePage = ((runePage == 0) ? RuneHelper.GetDefaultRunePage() : runePage),
			IsPreserve = isPreserve
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.EquipRune.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.EquipRune.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Apply(response.HeroRunePageInfo);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipItemSlotIndex)?.SetEquippedHeroIndex(heroIndex);
				if (!isPreserve)
				{
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(preserveEquipItemSlotIndex, null);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(preserveEquipItemSlotIndex)?.SetEquippedHeroIndex(-1);
				}
				caller.Refresh();
				caller.RightPanelRefresh();
				caller.CloseRuneItemInfos();
				UpdateStat();
				if (onCompelete != null)
				{
					onCompelete();
				}
			}
		});
	}

	void HeroRunehengeWindow.IListener.RequestUnequipRune(HeroRunehengeWindow caller, int heroIndex, int itemIndex, int equipItemSlotIndex, RuneSlotType slot, int runePage, bool isPreserve)
	{
		NShared.UnequipRune.Request request = new NShared.UnequipRune.Request
		{
			HeroIndex = heroIndex,
			SlotNum = (int)slot,
			RunePage = ((runePage == 0) ? RuneHelper.GetDefaultRunePage() : runePage),
			IsPreserve = isPreserve
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UnequipRune.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.UnequipRune.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Apply(response.HeroRunePageInfo);
				EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipItemSlotIndex);
				if (equipItemInfo != null)
				{
					if (isPreserve)
					{
						equipItemInfo.SetEquippedHeroIndex(-1);
					}
					else
					{
						GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(equipItemInfo.SlotIndex, null);
					}
				}
				caller.Refresh();
				caller.RightPanelRefresh();
				caller.CloseRuneItemInfos();
				UpdateStat();
			}
		});
	}

	void HeroRunehengeWindow.IListener.RequestApplyRunePage(HeroRunehengeWindow caller, int heroIndex, int runePageIndex)
	{
		NShared.ApplyHeroRunePage.Request request = new NShared.ApplyHeroRunePage.Request
		{
			HeroIndex = heroIndex,
			RunePage = ((runePageIndex == 0) ? RuneHelper.GetDefaultRunePage() : runePageIndex)
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ApplyHeroRunePage.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.ApplyHeroRunePage.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ApplyHeroRunePage(response.HeroInfo);
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Apply(response.HeroRunePageInfos);
				caller.Refresh();
			}
		});
	}

	void HeroRunehengeWindow.IListener.RequestExtendRunePage(HeroRunehengeWindow caller, int heroIndex, int runePageIndex)
	{
		long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
		RuneHelper.GetMaxRunePage();
		int runePagePrice = RuneHelper.GetRunePagePrice();
		long num = totalGem;
		int num2 = runePagePrice;
		if (num2 > num)
		{
			ShowNotEnoughGemPopup();
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("RUNEPAGE_ADD_SLOT", runePageIndex);
		string action = LocalizationManager.GetString(HeroTranscend.ADD_SKILL_PAGE_CONFIRM_ACTION);
		PricedMessagePopup.Open(title, stringDirect, action, PricedMessagePopup.EPriceType.Gem, runePagePrice, delegate
		{
			NShared.ExtendRunePage.Request request = new NShared.ExtendRunePage.Request
			{
				HeroIndex = heroIndex,
				RunePage = ((runePageIndex == 0) ? RuneHelper.GetDefaultRunePage() : runePageIndex)
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ExtendRunePage.Response response)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.ExtendRunePage.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ApplyHeroRunePage(response.HeroInfo);
					GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Apply(response.HeroRunePageInfos);
					caller.Refresh();
				}
			});
		});
	}

	void HeroSkillListPanel.Listener.OnSelectSkillComponent(int skillSlotIndex, int linkedIndex)
	{
		OpenHeroSkillDesc(_currentHero, skillSlotIndex, linkedIndex);
	}

	public void RefreshSkillList()
	{
		GetHeroSkillList().Refresh();
	}

	public void RefreshOnSkillLevelUp(bool isLevelUp)
	{
		GetHeroSkillList().Refresh();
		GetInstance<HeroSkillDesc>().SetHeroInfo(_currentHero);
		GetInstance<HeroSkillDesc>().Refresh(isLevelUp);
		GetInstance<HeroSkillDesc>().PlayIconEffect();
	}

	private IEnumerator coLearnHeroSkill(int skillSlotIndex)
	{
		Helper.SkillBasicInfo skillBasicInfo = Helper.GetSkillBasicInfo(_currentHero, skillSlotIndex);
		NShared.LearnHeroSkill.Request request = new NShared.LearnHeroSkill.Request
		{
			HeroIndex = _currentHero.HeroIndex,
			SkillIndex = skillBasicInfo.TableIndex
		};
		bool executeEnd = false;
		NShared.LearnHeroSkill.Response response = null;
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.LearnHeroSkill.Response iResponse)
		{
			executeEnd = true;
			response = iResponse;
		});
		while (!executeEnd)
		{
			yield return null;
		}
		_currentServiceRequest = null;
		if (response != null)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				yield break;
			}
			if (response.Result != NShared.LearnHeroSkill.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				yield break;
			}
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
			GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(request.HeroIndex)?.SetSkillLevel(skillSlotIndex, 1);
			RefreshOnSkillLevelUp(isLevelUp: false);
		}
	}

	private IEnumerator coExtendHeroSkill(int skillSlotIndex, int reqSkillExtend)
	{
		Helper.SkillBasicInfo skillBasicInfo = Helper.GetSkillBasicInfo(_currentHero, skillSlotIndex);
		NShared.ExtendHeroSkill.Request request = new NShared.ExtendHeroSkill.Request
		{
			HeroIndex = _currentHero.HeroIndex,
			SkillIndex = skillBasicInfo.TableIndex,
			SkillExtend = reqSkillExtend
		};
		bool executeEnd = false;
		NShared.ExtendHeroSkill.Response response = null;
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ExtendHeroSkill.Response iResponse)
		{
			executeEnd = true;
			response = iResponse;
		});
		while (!executeEnd)
		{
			yield return null;
		}
		_currentServiceRequest = null;
		if (response != null)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				yield break;
			}
			if (response.Result != NShared.ExtendHeroSkill.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				yield break;
			}
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
			GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetSkillExtend(request.HeroIndex, skillSlotIndex, reqSkillExtend);
		}
	}

	void HeroTranscendSkillListPanel.Listener.SelectTranscendSkill(int page, int transcended, int skillSlotIndex, int skillIndex)
	{
		LearnTranscendSkillMessagePopup.Open(_currentHero, skillIndex, delegate
		{
			if (!WebServiceRequest.IsProgressing(_currentServiceRequest))
			{
				VespaCoroutine.Start(this, coLearnHeroTranscendSkill(page, transcended, skillSlotIndex, skillIndex));
			}
		});
	}

	void HeroTranscendSkillListPanel.Listener.SelectTranscendPreset(int heroIndex, int page)
	{
		NShared.ApplyHeroTranscendSkillPage.Request request = new NShared.ApplyHeroTranscendSkillPage.Request
		{
			HeroIndex = heroIndex,
			PageIndex = page
		};
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(request.HeroIndex);
		int oldPage = heroInfo.SkillPage;
		heroInfo.SetTranscendSkillPage(page);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.None, delegate(NShared.ApplyHeroTranscendSkillPage.Response response)
		{
			if (response == null)
			{
				heroInfo.SetTranscendSkillPage(oldPage);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				heroInfo.SetTranscendSkillPage(oldPage);
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.ApplyHeroTranscendSkillPage.ResultType.Success)
			{
				heroInfo.SetTranscendSkillPage(oldPage);
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SelectTranscendSkillPage(request.HeroIndex, page);
			}
		});
	}

	void HeroTranscendSkillListPanel.Listener.AddTranscendPreset(int page)
	{
		int reqGem = 0;
		int openedTranscendSkillPageCount = _currentHero.GetOpenedTranscendSkillPageCount();
		string key = $"AddTranscendSkillPageGem{openedTranscendSkillPageCount + 1}";
		reqGem = GameRoot.Instance.ConstantDataContainer.GetValueInt(key);
		if (reqGem == 0)
		{
			return;
		}
		long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
		int num = reqGem;
		long num2 = totalGem;
		int num3 = num;
		if (num3 > num2)
		{
			ShowNotEnoughGemPopup();
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(HeroTranscend.ADD_SKILL_PAGE_CONFIRM, page);
		string action = LocalizationManager.GetString(HeroTranscend.ADD_SKILL_PAGE_CONFIRM_ACTION);
		PricedMessagePopup.Open(title, message, action, PricedMessagePopup.EPriceType.Gem, reqGem, delegate
		{
			NShared.BuyHeroTranscendSkillPage.Request request = new NShared.BuyHeroTranscendSkillPage.Request
			{
				HeroIndex = _currentHero.HeroIndex,
				PageIndex = page,
				BuyGem = reqGem
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BuyHeroTranscendSkillPage.Response response)
			{
				if (response != null)
				{
					if (response.BaseResult != BaseResultType.Success)
					{
						MessagePopup.OpenError(response.BaseResult);
					}
					else
					{
						if (response.Result == NShared.BuyHeroTranscendSkillPage.ResultType.NotEnoughGem)
						{
							ShowNotEnoughGemPopup();
						}
						else if (response.Result != NShared.BuyHeroTranscendSkillPage.ResultType.Success)
						{
							MessagePopup.OpenError(response.Result);
							return;
						}
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
						GlobalAutoBehaviour<AccountManager>.instance.HeroManager.AddTranscendSkillPage(request.HeroIndex, page);
					}
				}
			});
		});
	}

	void HeroTranscendSkillListPanel.Listener.RecommendTranscend(HeroInfo heroInfo, int page)
	{
		RecommendTranscendContainer recommendTranscendContainer = GetInstance<RecommendTranscendContainer>();
		if (!(recommendTranscendContainer == null))
		{
			if (recommendTranscendContainer.IsOpen())
			{
				recommendTranscendContainer.Close();
			}
			else
			{
				recommendTranscendContainer.Open(heroInfo, page, this);
			}
		}
	}

	private void ShowNotEnoughGemPopup()
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
		string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
		MessagePopup.Open(title, message, delegate
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
		}, null);
	}

	public void LearnHeroTranscendSkill(int heroIndex, int page, List<CreatureData.TranscendSkill> skillList, Action onSuccess)
	{
		VespaCoroutine.Start(this, coLearnHeroTranscendSkill(heroIndex, page, skillList, onSuccess));
	}

	private IEnumerator coLearnHeroTranscendSkill(int heroIndex, int page, List<CreatureData.TranscendSkill> skillList, Action onSuccess)
	{
		CreatureData.TranscendSkill[] array = skillList.Where((CreatureData.TranscendSkill item) => item.Level > 0).ToArray();
		int[] array2 = new int[array.Length * 2];
		for (int num = 0; num < array.Length; num++)
		{
			array2[num * 2] = array[num].Transcended * 10 + array[num].Index;
			array2[num * 2 + 1] = array[num].Level;
		}
		NShared.LearnHeroTranscendSkillPage.Request request = new NShared.LearnHeroTranscendSkillPage.Request
		{
			HeroIndex = heroIndex,
			TranscendSkills = array2,
			PageIndex = page
		};
		bool executeEnd = false;
		NShared.LearnHeroTranscendSkillPage.Response response = null;
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.LearnHeroTranscendSkillPage.Response iResponse)
		{
			executeEnd = true;
			response = iResponse;
		});
		while (!executeEnd)
		{
			yield return null;
		}
		_currentServiceRequest = null;
		if (response != null)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				yield break;
			}
			if (response.Result != NShared.LearnHeroTranscendSkillPage.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				yield break;
			}
			CenterMessage.Open(HeroTranscend.TRANSCEND_POINT_APPLY);
			string transcendSkillPageStringArray = response.Hero.GetTranscendSkillPageStringArray(page);
			GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetTranscendSkill(page, _currentHero.HeroIndex, transcendSkillPageStringArray);
			Vespa_System_Action_Extension_Method.fire(onSuccess);
		}
	}

	private IEnumerator coLearnHeroTranscendSkill(int page, int transcended, int skillSlotIndex, int skillIndex)
	{
		int[] transcendSkills = new int[2]
		{
			transcended * 10 + skillSlotIndex,
			1
		};
		NShared.LearnHeroTranscendSkillPage.Request request = new NShared.LearnHeroTranscendSkillPage.Request
		{
			HeroIndex = _currentHero.HeroIndex,
			TranscendSkills = transcendSkills,
			PageIndex = page
		};
		bool executeEnd = false;
		NShared.LearnHeroTranscendSkillPage.Response response = null;
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.LearnHeroTranscendSkillPage.Response iResponse)
		{
			executeEnd = true;
			response = iResponse;
		});
		while (!executeEnd)
		{
			yield return null;
		}
		_currentServiceRequest = null;
		if (response == null)
		{
			yield break;
		}
		if (response.BaseResult != BaseResultType.Success)
		{
			MessagePopup.OpenError(response.BaseResult);
			yield break;
		}
		if (response.Result != NShared.LearnHeroTranscendSkillPage.ResultType.Success)
		{
			MessagePopup.OpenError(response.Result);
			yield break;
		}
		string transcendSkillPageStringArray = response.Hero.GetTranscendSkillPageStringArray(page);
		GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetTranscendSkill(page, _currentHero.HeroIndex, transcended, skillSlotIndex, transcendSkillPageStringArray);
		RecommendTranscendContainer recommendTranscendContainer = GetInstance<RecommendTranscendContainer>();
		if (recommendTranscendContainer != null && recommendTranscendContainer.IsOpen())
		{
			recommendTranscendContainer.ToggleRecommend(isForce: true);
		}
	}

	void HeroTranscendSkillListPanel.Listener.ResetTranscendSkill(int page)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(HeroTranscend.RESET_ALL_TRANSCENDED_SKILL_CONFIRM);
		string action = LocalizationManager.GetString(HeroTranscend.RESET_ALL_TRANSCENDED_SKILL);
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("ResetTranscendSkillGold");
		PricedMessagePopup.Open(title, message, action, PricedMessagePopup.EPriceType.Gold, valueInt, delegate
		{
			VespaCoroutine.Start(this, coResetAllHeroTranscendSkill(page));
		});
	}

	private IEnumerator coResetAllHeroTranscendSkill(int page)
	{
		NShared.ResetHeroTranscendSkillPage.Request request = new NShared.ResetHeroTranscendSkillPage.Request
		{
			PageIndex = page,
			HeroIndex = _currentHero.HeroIndex
		};
		bool executeEnd = false;
		NShared.ResetHeroTranscendSkillPage.Response response = null;
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ResetHeroTranscendSkillPage.Response iResponse)
		{
			executeEnd = true;
			response = iResponse;
		});
		while (!executeEnd)
		{
			yield return null;
		}
		_currentServiceRequest = null;
		if (response == null)
		{
			yield break;
		}
		if (response.BaseResult != BaseResultType.Success)
		{
			MessagePopup.OpenError(response.BaseResult);
			yield break;
		}
		if (response.Result != NShared.ResetHeroTranscendSkillPage.ResultType.Success)
		{
			MessagePopup.OpenError(response.Result);
			yield break;
		}
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
		GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ResetAllTranscendSkill(_currentHero.HeroIndex, page);
		RecommendTranscendContainer recommendTranscendContainer = GetInstance<RecommendTranscendContainer>();
		if (recommendTranscendContainer != null && recommendTranscendContainer.IsOpen())
		{
			recommendTranscendContainer.ToggleRecommend(isForce: true);
		}
	}

	void HeroList.Listener.OnClickHeroPortrait(HeroPortraitComponent heroPortrait)
	{
		if (IsOpened<HeroRunehengeWindow>())
		{
			HeroRunehengeWindow heroRunehengeWindow = GetInstance<HeroRunehengeWindow>();
			heroRunehengeWindow.CloseRuneItemInfos();
			heroRunehengeWindow.Close();
		}
		OpenMyHero(heroPortrait.HeroInfo, _lastViewToggle, Hero3DViewPanel.ENextTask.None, _lastEquipPartType);
	}

	void HeroList.Listener.ReOpen()
	{
		OpenMyHero(_currentHero, _lastViewToggle, Hero3DViewPanel.ENextTask.None, _lastEquipPartType);
	}

	void HeroList.Listener.OnClickAllHero()
	{
		OpenPortraitList();
	}

	void HeroList.Listener.OnSaveBookmark()
	{
		HeroDeck2 heroDeck = GetInstance<HeroDeck2>();
		if (heroDeck != null)
		{
			heroDeck.Refresh();
		}
	}

	public void OnPositionChange(int from, int to, bool bookmarked)
	{
		HeroPortraitList heroPortraitList = GetInstance<HeroPortraitList>();
		if (heroPortraitList != null)
		{
			heroPortraitList.MoveGridItem(from, to, bookmarked);
		}
	}

	void HeroSkillDesc.Listener.LearnSkill(int skillSlotIndex)
	{
		if (!WebServiceRequest.IsProgressing(_currentServiceRequest))
		{
			VespaCoroutine.Start(this, coLearnHeroSkill(skillSlotIndex));
		}
	}

	void HeroSkillDesc.Listener.ExtendSkill(int skillSlotIndex, int reqSkillExtend, bool isEnoughItem)
	{
		if (isEnoughItem)
		{
			string title = LocalizationManager.GetString(HeroSkill.LEARN_EXTEND_SKILL);
			string message = LocalizationManager.GetString(HeroSkill.CONFIRM_EXTEND_SKILL);
			MessagePopup.Open(title, message, delegate
			{
				if (!WebServiceRequest.IsProgressing(_currentServiceRequest))
				{
					VespaCoroutine.Start(this, coExtendHeroSkill(skillSlotIndex, reqSkillExtend));
				}
			}, null);
			return;
		}
		Helper.SkillDetailInfo skillDetailInfo = Helper.GetSkillDetailInfo(_currentHero, skillSlotIndex);
		Helper.SkillBookInfo skillBookInfo = skillDetailInfo.BookInfo;
		Helper.SkillExtendInfo skillExtendInfo = skillDetailInfo.ExtendInfos[reqSkillExtend - 1];
		if (GameRoot.Instance.DOWDungeonTextDataContainer.GetAllData().FirstOrDefault((DOWDungeonTextData v) => v.MainRewardItemCode == skillBookInfo.BookItemData.Code) == null)
		{
			return;
		}
		UnderPrisonData underPrisonDataByUnderPrisonType = GameRoot.Instance.UnderPrisonDataContainer.GetUnderPrisonDataByUnderPrisonType(UnderPrisonType.SkillBook);
		if (underPrisonDataByUnderPrisonType == null)
		{
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect(underPrisonDataByUnderPrisonType.Name);
		int num = skillExtendInfo.ExtendItemCount - skillBookInfo.CurItemCount;
		string title2 = LocalizationManager.GetString(HeroSkill.LEARN_EXTEND_SKILL);
		string message2 = LocalizationManager.GetString(HeroSkill.NOT_ENOUGH_ITEM_WARNING, skillBookInfo.BookItemData.Name, num);
		string message3 = LocalizationManager.GetString(HeroSkill.NOT_ENOUGH_ITEM_DESC, skillBookInfo.BookItemData.Name, stringDirect);
		IconMessagePopup.Open(title2, message2, message3, delegate
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.CloseAllWindows();
			_ = GameRoot.Instance.CampaignNodeDataContainer;
			int chapterIndex = 0;
			int dungeonIndex = 0;
			using (IEnumerator<CampaignNodeData> enumerator = GameRoot.Instance.CampaignNodeDataContainer.GetData_by_ContentsType(ContentsStatusType.Prison).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					CampaignNodeData current = enumerator.Current;
					chapterIndex = current.ChapterIndex;
					dungeonIndex = current.DungeonIndex;
				}
			}
			SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(chapterIndex, dungeonIndex, delegate
			{
				SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(null);
			});
		}).AddItem(skillBookInfo.BookItemData.Index, skillExtendInfo.ExtendItemCount);
	}

	bool HeroPortraitList.Listener.OnCloseRequest(HeroPortraitList caller)
	{
		return true;
	}

	void HeroPortraitList.Listener.OnClick(HeroPortraitComponent comp)
	{
		int heroIndex = comp.HeroInfo.HeroIndex;
		HeroList heroList = GetInstance<HeroList>();
		if (heroList != null && heroList.SelectHero(heroIndex))
		{
			OpenMyHero(comp.HeroInfo, _lastViewToggle, Hero3DViewPanel.ENextTask.None, _lastEquipPartType);
		}
		HeroPortraitList heroPortraitList = GetInstance<HeroPortraitList>();
		if (heroPortraitList != null)
		{
			heroPortraitList.Close();
		}
	}

	void HeroEquipTreasure.Listener.OnClickUnsetSlot(int[] equipItemSlotIndices, CachedHeroInfo cachedHeroInfo, Action onSuccess)
	{
		RequestUnsetEquip(equipItemSlotIndices, cachedHeroInfo, isUnEquipAction: true, onSuccess);
	}

	void HeroEquipTreasure.Listener.OnClickSwapSlot(int[] equipItemSlotIndices, int heroIndex, int[] heroPartIndices)
	{
		RequestSwapEquip(equipItemSlotIndices, heroIndex, heroPartIndices);
	}

	void HeroEquipTreasure.Listener.OnClickEquipSlot(int itemSlotIndex, int heroIndex, int equipslotIndex)
	{
		RequestSetEquip(itemSlotIndex, heroIndex, equipslotIndex);
	}

	void HeroEquipTreasure.Listener.OnClickItemIcon(ItemIconComponent caller)
	{
		if (!(caller == null))
		{
			SetSelectedItemIcon(caller.EquipItemInfo);
			GetInstance<InvenEquipInfoUIContainer>().Open(InvenEquipInfoUIContainer.WindowPosType.Center, caller.CachedHeroInfo, caller.EquipItemInfo, this, null);
		}
	}

	bool HeroEquipTreasure.Listener.OnClickClose(HeroEquipTreasure caller)
	{
		return true;
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.AddedHero)
		{
			HeroDeck2 heroDeck = GetInstance<HeroDeck2>();
			if (heroDeck != null && heroDeck.IsOpen())
			{
				HeroDeck2.EPositionTab curPositionTab = heroDeck.CurPositionTab;
				CreaturePositionType positionType = GameRoot.Instance.CreatureDataContainer.GetData(changedInfo.NewValue)?.PositionType ?? CreaturePositionType.Forward;
				if (curPositionTab == HeroDeck2.EPositionTab.Total || HeroDeck2.GetPositionTabType(positionType) == curPositionTab)
				{
					heroDeck.Refresh();
				}
			}
			HeroList heroList = GetInstance<HeroList>();
			if (heroList != null && heroList.IsOpen())
			{
				int selectedHeroIndex = heroList.SelectedHeroIndex;
				heroList.Init(this, selectedHeroIndex);
			}
		}
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.HideUniqueWeapon)
		{
			Hero3DViewPanel hero3DView = GetHero3DView();
			if (hero3DView != null)
			{
				hero3DView.EquipImpl.RefreshShowUniqueWeapon();
			}
		}
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo info)
	{
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void HeroEquipStoragePanel.IListener.OnClickEquip(int slotIndex)
	{
		if (_currentHero == null)
		{
			return;
		}
		EquipPresetInfo equipPresetInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.Get(slotIndex);
		if (equipPresetInfo != null)
		{
			if (PresetStorageHelper.IsEmptySlotEquipPresetInfo(equipPresetInfo))
			{
				LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			}
			else
			{
				SingletonAuto<EquipPresetWebManager>.instance.RequestLoadEquipPresetStorageSlot(_currentHero.HeroIndex, slotIndex, null);
			}
		}
	}

	void HeroEquipStoragePanel.IListener.OnClickSave(int slotIndex)
	{
		if (_currentHero == null)
		{
			return;
		}
		EquipPartType[] saveEquipParts = EquipPresetInfo.SaveEquipParts;
		List<int> partIndices = new List<int>();
		List<int> equipItemSlotIndices = new List<int>();
		EquipPartType[] array = saveEquipParts;
		foreach (EquipPartType equipPartType in array)
		{
			int item = 0;
			EquipItemInfo equipItemInfoByPartType = _currentHero.GetEquipItemInfoByPartType(equipPartType);
			if (equipItemInfoByPartType != null)
			{
				item = equipItemInfoByPartType.SlotIndex;
			}
			partIndices.Add((int)equipPartType);
			equipItemSlotIndices.Add(item);
		}
		if (IsSavableEquipItem(_currentHero))
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("EQUIP_STORAGE_SLOT_SAVE_CONFIRM");
			MessagePopup.Open(title, stringDirect, delegate
			{
				SingletonAuto<EquipPresetWebManager>.instance.RequestSaveEquipPresetStorageSlot(_currentHero.HeroIndex, slotIndex, partIndices.ToArray(), equipItemSlotIndices.ToArray());
			}, null);
		}
		else
		{
			LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		}
	}

	public static bool IsSavableEquipItem(CachedHeroInfo hero)
	{
		EquipPartType[] saveEquipParts = EquipStorageSlotInfo.SaveEquipParts;
		bool result = false;
		EquipPartType[] array = saveEquipParts;
		foreach (EquipPartType partType in array)
		{
			if (hero.GetEquipItemInfoByPartType(partType) != null)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	void HeroEquipStoragePanel.IListener.OnChagnedName(int slotIndex, string name)
	{
		SingletonAuto<EquipPresetWebManager>.instance.RequestChangeNameEquipPresetStorage(slotIndex, name);
	}

	void HeroEquipStoragePanel.IListener.OnClickUnlock()
	{
		int currentSlotCount = GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.CurrentSlotCount;
		StorageSlotExtendData dataBySlotCount = GameRoot.Instance.StorageSlotExtendDataContainer.GetDataBySlotCount(currentSlotCount + 1);
		if (dataBySlotCount == null)
		{
			return;
		}
		int extendPrice = dataBySlotCount.ExtendPrice;
		long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
		int num = extendPrice;
		if (totalGem < num)
		{
			ShowNotEnoughGemPopup();
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("EQUIP_STORAGE_SLOT_BUY_CONFIRM");
		string action = LocalizationManager.GetString(Inventory.EQUIP_EXTEND_ACTION);
		PricedMessagePopup.Open(title, stringDirect, action, PricedMessagePopup.EPriceType.Gem, extendPrice, delegate
		{
			SingletonAuto<EquipPresetWebManager>.instance.RequestBuyEquipPresetStorageSlot(ShowNotEnoughGemPopup);
		});
	}

	void HeroEquipStoragePanel.IListener.OnClickReset(int slotIndex)
	{
		if (!PresetStorageHelper.IsEmptySlotEquipPresetInfo(GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.Get(slotIndex)))
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("EQUIP_STORAGE_SLOT_RESET");
			MessagePopup.Open(title, stringDirect, delegate
			{
				SingletonAuto<EquipPresetWebManager>.instance.RequestResetEquipPresetStorage(slotIndex);
			}, null);
		}
	}

	void HeroEquipStoragePanel.IListener.OnClickEquipIcon(ItemIconComponent icon)
	{
		ShowEquipInfo(icon);
	}

	int HeroEquipStoragePanel.IDataProvider.GetSelectedHeroIndex()
	{
		if (_currentHero == null)
		{
			return -1;
		}
		return _currentHero.HeroIndex;
	}

	void InvenEquipInfo.Listener.OnMove(InvenEquipInfo component)
	{
	}

	void InvenEquipInfo.Listener.OnSell(InvenEquipInfo component)
	{
		EquipItemInfo equipItemInfo = component.EquipItemInfo;
		if (equipItemInfo == null)
		{
			return;
		}
		InventoryManagement.ProcessSellEquip(equipItemInfo, delegate(bool success)
		{
			if (success)
			{
				component.Close();
			}
		});
	}

	void InvenEquipInfo.Listener.OnBreak(InvenEquipInfo component)
	{
		EquipItemInfo equipItemInfo = component.EquipItemInfo;
		if (equipItemInfo == null)
		{
			return;
		}
		Action OnAccepted = delegate
		{
			List<EquipItemInfo> equipItemInfos = new List<EquipItemInfo> { equipItemInfo };
			InventoryManagement.BreakEquipItems(this, equipItemInfos, delegate(bool success)
			{
				if (success)
				{
					component.Close();
				}
			}, delegate(HeroList window)
			{
				window.HeroEquipListPanel.Refresh();
			});
		};
		if ((equipItemInfo.IsUniqueWeapon || equipItemInfo.IsUniqueTreasure) && equipItemInfo.IsBreakable)
		{
			BreakConfirmPopup.Open(equipItemInfo, delegate
			{
				OnAccepted();
			}, null);
		}
		else
		{
			OnAccepted();
		}
	}

	void InvenEquipInfo.Listener.OnKeep(InvenEquipInfo component)
	{
	}

	void InvenEquipInfo.Listener.OnBring(InvenEquipInfo component)
	{
	}

	void HeroEquipInfo.Listener.OnClose(EquipItemInfo equipItemInfo)
	{
		SetSelectedItemIcon(equipItemInfo, isSelected: false);
	}

	void InvenEquipInfo.Listener.OpenSoulWeaponLibearationWindow(EquipItemInfo equipItemInfo)
	{
		SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenSoulWeaponLiberationWindow(equipItemInfo, isSelectFirst: false);
	}

	void InvenEquipInfo.Listener.OpenSoulWeaponManageWindow(EquipItemInfo equipItemInfo)
	{
		SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenSoulWeaponManageWindow(equipItemInfo);
	}

	void InvenEquipInfo.Listener.CancelSoulWeapon(EquipItemInfo equipItemInfo)
	{
		SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenBreakSoulWeaponPopup(equipItemInfo);
	}

	void InvenEquipInfo.Listener.TechnoMagicIdentified(EquipItemInfo equipItemInfo)
	{
		SceneAutoBehaviour<ValanceManagement>.instance.OpenIdentifiedInven(equipItemInfo);
	}

	void RecommendTranscendContainer.Listener.LearnRecommendTranscend(List<int> recommendIndex, int page)
	{
		if (_currentHero == null || __Extension_Method_ICollection__.IsNullOrEmpty(recommendIndex) || _currentHero.Transcended == 0)
		{
			return;
		}
		Queue<CreatureData.TranscendSkill> transcendSkills = new Queue<CreatureData.TranscendSkill>();
		CreatureData creatureData = _currentHero.CreatureData;
		for (int i = 0; i < recommendIndex.Count; i++)
		{
			CreatureData.TranscendSkill transcendSkillData = creatureData.GetTranscendSkillData(recommendIndex[i]);
			if (transcendSkillData != null && _currentHero.Transcended >= transcendSkillData.Transcended)
			{
				transcendSkills.Enqueue(transcendSkillData);
			}
		}
		if (transcendSkills.Count == 0)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("HERO_TRANSCEND.RECOMMEND_FAIL_01");
			MessagePopup.Open(title, stringDirect);
		}
		else if (_currentHero.AnyTranscendSkillLearned(page))
		{
			string title2 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(HeroTranscend.RESET_RECOMMEND_ALL_TRANSCENDED_SKILL_CONFIRM);
			string action = LocalizationManager.GetString(HeroTranscend.RESET_ALL_TRANSCENDED_SKILL_01);
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("ResetTranscendSkillGold");
			PricedMessagePopup.Open(title2, message, action, PricedMessagePopup.EPriceType.Gold, valueInt, delegate
			{
				VespaCoroutine.Start(coResetAllHeroTranscendSkill(page), delegate
				{
					VespaCoroutine.Start(coLearnRecommendTranscend(transcendSkills, page));
				});
			});
		}
		else
		{
			string title3 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message2 = LocalizationManager.GetString(HeroTranscend.RECOMMEND_ALL_TRANSCENDED_SKILL_CONFIRM, creatureData.Name);
			string stringDirect2 = LocalizationManager.GetStringDirect("COMMON_CONFIRM_01");
			MessagePopup.OpenEnabled(title3, message2, delegate
			{
				VespaCoroutine.Start(coLearnRecommendTranscend(transcendSkills, page));
			}, null, stringDirect2, bYesEnabled: true);
		}
	}

	void ArchiveHeroProfilePanel.Listener.OpenMonthlyHeroWindow(int creatureIndex)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.OpenMonthlyHeroWindow(creatureIndex);
	}

	void InvenEquipInfo.Listener.ClickHeroSpecialEquipInfo(InvenEquipInfoUIContainer.WindowPosType windowPosType, EquipItemInfo equipItemInfo)
	{
		if (GetInstance<InvenEquipInfoUIContainer>() != null)
		{
			GetInstance<InvenEquipInfoUIContainer>().Open(windowPosType, equipItemInfo);
		}
	}

	private IEnumerator coLearnRecommendTranscend(Queue<CreatureData.TranscendSkill> transcendSkills, int page)
	{
		yield return null;
	}

	public void CloseInvenEquipInfoUIContainer()
	{
		InvenEquipInfoUIContainer invenEquipInfoUIContainer = GetInstance<InvenEquipInfoUIContainer>();
		if (invenEquipInfoUIContainer != null)
		{
			invenEquipInfoUIContainer.Close();
		}
	}

	void HeroPresetStorageWindow.IListener.OnBuyHeroStorageSlot(string storageKey)
	{
		SingletonAuto<HeroPresetStorageWebManager>.instance.RequestBuyHeroSotrageSlot(storageKey);
	}

	void HeroPresetStorageWindow.IListener.OnAddHeroStorageSlot(string storageKey)
	{
		SingletonAuto<HeroPresetStorageWebManager>.instance.RequestAddHeroStorageSlot(storageKey);
	}

	void HeroPresetStorageWindow.IListener.OnChangeHeroStorageSlot(string staorageKey, string name)
	{
		SingletonAuto<HeroPresetStorageWebManager>.instance.RequestChangeHeroStorageSlot(staorageKey, name);
	}

	void HeroPresetStorageWindow.IListener.OnRemoveHeroStoraeSlot(string storageKey)
	{
	}

	void HeroPresetStorageWindow.IListener.OnSetHeroStorageSlot(string storageKey)
	{
		SingletonAuto<HeroPresetStorageWebManager>.instance.RequestSetHeroStorageSlot(storageKey);
	}

	void HeroPresetStorageWebManager.IListener.OnBuyHeroStorageSlot(HeroPresetStorageInfo heroPresetStorageInfo)
	{
		HeroPresetStorageWindow heroPresetStorageWindow = GetInstance<HeroPresetStorageWindow>();
		if (!(heroPresetStorageWindow == null) && heroPresetStorageWindow.IsOpen())
		{
			heroPresetStorageWindow.OnAddSlot();
		}
	}

	void HeroPresetStorageWebManager.IListener.OnAddHeroStorageSlot(HeroPresetStorageInfo heroPresetStorageInfo)
	{
		HeroPresetStorageWindow heroPresetStorageWindow = GetInstance<HeroPresetStorageWindow>();
		if (!(heroPresetStorageWindow == null) && heroPresetStorageWindow.IsOpen())
		{
			heroPresetStorageWindow.OnAddSlot();
		}
	}

	void HeroPresetStorageWebManager.IListener.OnChangeHeroStorageSlot(string staorageKey, string name)
	{
		HeroPresetStorageWindow heroPresetStorageWindow = GetInstance<HeroPresetStorageWindow>();
		if (!(heroPresetStorageWindow == null) && heroPresetStorageWindow.IsOpen())
		{
			heroPresetStorageWindow.OnChangeSlotName();
		}
	}

	void HeroPresetStorageWebManager.IListener.OnSetHeroStorageSlot()
	{
		Hero3DViewPanel hero3DView = GetHero3DView();
		if (hero3DView != null && hero3DView.gameObject.activeSelf)
		{
			hero3DView.SetupEquipItemIcon();
		}
		HeroTranscendSkillListPanel heroTranscendSkillList = GetHeroTranscendSkillList();
		if (heroTranscendSkillList != null && heroTranscendSkillList.gameObject.activeSelf)
		{
			heroTranscendSkillList.SelectPreset(_currentHero.SkillPage);
		}
		HeroStatPanel heroStat = GetHeroStat();
		if (heroStat != null && heroStat.gameObject.activeSelf)
		{
			heroStat.Open(_currentHero, this);
		}
		HeroEquipStoragePanel heroEquipStorageList = GetHeroEquipStorageList();
		if (heroEquipStorageList != null && heroEquipStorageList.gameObject.activeSelf)
		{
			heroEquipStorageList.UpdateButtons();
			heroEquipStorageList.UpdateEquipItems();
			heroEquipStorageList.UpdateEquipPresetSlotComponent();
		}
	}

	void HeroList.Listener.OnClickRunehengeIcon()
	{
		OpenRunehenge();
	}

	public void SprkRefreshTechnomagicGear()
	{
		if (_currentHero != null)
		{
			GetHero3DView(openedOnly: true)?.SetupEquipItemIcon();
			GetHeroEquipList(openedOnly: true)?.Refresh();
			GetInstance<InvenEquipInfoUIContainer>(openedOnly: true)?.ChangeHero(_currentHero);
			RefreshHeroEquipTreasure(_currentHero);
		}
	}
}
