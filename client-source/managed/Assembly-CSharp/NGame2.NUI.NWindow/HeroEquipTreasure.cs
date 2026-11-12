using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow.NEquipTreasure;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[UIEvent(AllowDuringPatchDownloading = true)]
public class HeroEquipTreasure : BaseWindow, TreasureSlotComponent.Listener, TreasureStatViewer.IDataProvider, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>, IUserObserver
{
	public interface Listener
	{
		void OnClickSwapSlot(int[] equipItemSlotIndices, int heroIndex, int[] heroPartIndices);

		void OnClickUnsetSlot(int[] equipItemSlotIndices, CachedHeroInfo cachedHeroInfo, Action onSuccess);

		void OnClickEquipSlot(int itemSlotIndex, int heroIndex, int equipslotIndex);

		void OnClickItemIcon(ItemIconComponent caller);

		bool OnClickClose(HeroEquipTreasure caller);
	}

	[Serializable]
	public class SlotUI
	{
		public Transform RootSlotComponent;

		public UISprite SpriteLock;
	}

	private enum SlotSelectState
	{
		None,
		Equip,
		Swap
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroEquipTreasure";

	public SlotUI[] TreasureSlotUIs;

	public UIPanel Panel_SelectSlot;

	public UIButton ButtonMainTreasureSwap;

	public UIPanel Panel_Stat;

	public UIScrollView ScrollView_Stat;

	private CachedHeroInfo _selectedCachedHeroInfo;

	private EquipItemInfo _toEquipItemInfo;

	private List<TreasureSlotComponent> _treasureComponentList = new List<TreasureSlotComponent>();

	private Dictionary<SlotSelectState, NGame2.NUI.NWindow.NEquipTreasure.ITreasureSlotStateHandler> _stateHelperDictionary = new Dictionary<SlotSelectState, NGame2.NUI.NWindow.NEquipTreasure.ITreasureSlotStateHandler>();

	private NGame2.NUI.NWindow.NEquipTreasure.ITreasureSlotStateHandler _stateHelper;

	private Listener _listener;

	private TreasureStatViewer _treasureStatViewer;

	private int MainTreasureEquipSlotIndex => EquipPartType.Treasure.GetEquipSlotIndex();

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	private void Clear()
	{
		FactoryBase.ReleaseAndClearList(_treasureComponentList);
		ClearStatList();
	}

	private void ClearStatList()
	{
		if (_treasureStatViewer != null)
		{
			_treasureStatViewer.Clear();
		}
	}

	public void Open(Listener listener, EquipItemInfo equipItemInfo, CachedHeroInfo cachedHeroInfo, bool selectSlotState = false)
	{
		if (listener == null)
		{
			VespaDebug.LogError("HeroEquipTreasure::Open listener is null");
			return;
		}
		if (cachedHeroInfo == null)
		{
			VespaDebug.LogError("HeroEquipTreasure::Open listener is null");
			return;
		}
		_listener = listener;
		_selectedCachedHeroInfo = cachedHeroInfo;
		_toEquipItemInfo = equipItemInfo;
		Open(delegate
		{
			Init(selectSlotState);
		}, null);
	}

	private void Init(bool selectSlotState)
	{
		if (_treasureStatViewer == null)
		{
			_treasureStatViewer = new TreasureStatViewer(this);
		}
		RegistStateHandler();
		ChangeState(selectSlotState ? SlotSelectState.Equip : SlotSelectState.None);
		Setup();
	}

	private void Setup()
	{
		SetupTreasureSlotComponent();
		SetupSlotSelectPanel();
		SetupStat();
		SetMainTreatureSwapButton();
	}

	private void RegistStateHandler()
	{
		_stateHelperDictionary[SlotSelectState.None] = new NoneState(_listener);
		_stateHelperDictionary[SlotSelectState.Equip] = new EquipState(_listener, _selectedCachedHeroInfo, _toEquipItemInfo);
		_stateHelperDictionary[SlotSelectState.Swap] = new SwapState(_listener, MainTreasureEquipSlotIndex, _selectedCachedHeroInfo, _treasureComponentList);
	}

	private void ChangeState(SlotSelectState state)
	{
		_stateHelper = GetStateHelper(state);
	}

	private NGame2.NUI.NWindow.NEquipTreasure.ITreasureSlotStateHandler GetStateHelper(SlotSelectState state)
	{
		if (!_stateHelperDictionary.ContainsKey(state))
		{
			return _stateHelperDictionary[SlotSelectState.None];
		}
		return _stateHelperDictionary[state];
	}

	private void SetupTreasureSlotComponent()
	{
		FactoryBase.ReleaseAndClearList(_treasureComponentList);
		List<EquipSlotInfo> treasureSlotInfoList = GetTreasureSlotInfoList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(treasureSlotInfoList) && i < __Extension_Method_ICollection__.COUNT(TreasureSlotUIs); i++)
		{
			if (TreasureSlotUIs[i] == null)
			{
				VespaDebug.LogError("TreasureSlotUIs[i] == null");
				continue;
			}
			UIHelper.EnableGameObject(TreasureSlotUIs[i].SpriteLock, enable: false);
			Transform rootSlotComponent = TreasureSlotUIs[i].RootSlotComponent;
			TreasureSlotComponent treasureSlotComponent = TreasureSlotComponent.PrefabAuto.Factory.Create(rootSlotComponent);
			EquipSlotInfo equipSlotinfo = treasureSlotInfoList[i];
			treasureSlotComponent.Open(this, equipSlotinfo, _selectedCachedHeroInfo);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(treasureSlotComponent.transform);
			treasureSlotComponent.AdjustToParentWidgetScale();
			_treasureComponentList.Add(treasureSlotComponent);
		}
	}

	private List<EquipSlotInfo> GetTreasureSlotInfoList()
	{
		List<EquipSlotInfo> treasureSlotInfoList = new List<EquipSlotInfo>();
		Array.ForEach(EquipPartType.Treasure.GetEquipSlotIndices(), delegate(int slotIndex)
		{
			EquipItemInfo equipItemInfoByPartIndex = _selectedCachedHeroInfo.GetEquipItemInfoByPartIndex(slotIndex);
			treasureSlotInfoList.Add(new EquipSlotInfo(slotIndex, equipItemInfoByPartIndex));
		});
		return treasureSlotInfoList;
	}

	public void SetupStat()
	{
		_treasureStatViewer.SetupStat();
	}

	private void SetMainTreatureSwapButton()
	{
		bool enable = _treasureComponentList.Exists((TreasureSlotComponent treasureComponent) => treasureComponent.IsSwapAbleSlot(MainTreasureEquipSlotIndex));
		UIHelper.EnableButton(ButtonMainTreasureSwap, enable);
	}

	private void SetupSlotSelectPanel()
	{
		UIHelper.EnableGameObject(Panel_SelectSlot, _stateHelper.ShowSelectPanel());
		_treasureComponentList.ForEach(delegate(TreasureSlotComponent item)
		{
			SetTreasureComponentState(item);
		});
	}

	private void SetTreasureComponentState(TreasureSlotComponent component)
	{
		if (!(component == null))
		{
			_stateHelper.OnChangeState(component);
		}
	}

	public void Refresh()
	{
		if (_selectedCachedHeroInfo != null)
		{
			ChangeState(SlotSelectState.None);
			Setup();
		}
	}

	public void Refresh(CachedHeroInfo currentHeroInfo)
	{
		if (currentHeroInfo == null || currentHeroInfo != _selectedCachedHeroInfo)
		{
			Close();
			return;
		}
		ChangeState(SlotSelectState.None);
		Setup();
	}

	public void OnClickClose()
	{
		if (_listener == null || _listener.OnClickClose(this))
		{
			Close();
		}
	}

	public void OnClickMainSwap()
	{
		if (DispatchHelper.IsDispatchedHero(_selectedCachedHeroInfo))
		{
			CenterMessage centerMessage = CenterMessage.Open(LocalizationManager.GetString(DispatchBattle.DISPATCH_EQUIP_REPLACE_MSG));
			if (centerMessage != null)
			{
				centerMessage.SetColor(Color.green);
			}
		}
		else
		{
			ChangeState(SlotSelectState.Swap);
			SetupSlotSelectPanel();
		}
	}

	public void OnClickSelectCancel()
	{
		ChangeState(SlotSelectState.None);
		SetupSlotSelectPanel();
	}

	void TreasureSlotComponent.Listener.OnClick(TreasureSlotComponent caller)
	{
		if (_listener != null && !(caller == null))
		{
			_stateHelper.OnSelect(caller);
		}
	}

	int TreasureStatViewer.IDataProvider.GetMainSlotIndex()
	{
		return MainTreasureEquipSlotIndex;
	}

	MonoBehaviour TreasureStatViewer.IDataProvider.GetMonoBehaviour()
	{
		return this;
	}

	List<EquipSlotInfo> TreasureStatViewer.IDataProvider.GetHeroEquippedTreasureList()
	{
		return GetTreasureSlotInfoList();
	}

	UIPanel TreasureStatViewer.IDataProvider.GetPanel()
	{
		return Panel_Stat;
	}

	UIScrollView TreasureStatViewer.IDataProvider.GetScrollView()
	{
		return ScrollView_Stat;
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo changedInfo)
	{
		TreasureSlotComponent treasureSlotComponent = _treasureComponentList.Find((TreasureSlotComponent item) => item.EquipItemInfo != null && item.EquipItemInfo.SlotIndex == changedInfo.SlotIndex);
		if (treasureSlotComponent != null)
		{
			treasureSlotComponent.ItemIconComponent.Refresh();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
