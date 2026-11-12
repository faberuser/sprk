using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPet;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NPet;
using NGameAsset;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NPet;

public class PetIncubatorManageWindow : RenewResourceWindow, IIncubatorDataProvider, PetIncubatorSlotIconComponent.IListener, IncubatorSlotStatePanel.IListener, ItemIconComponent.Listener, PetEggListPanel.IListener
{
	public interface IListener
	{
		void OnClickStartIncubating(int slotIndex, int incubatorIndex, int eggIndex);

		void OnClickCancelIncubating(int slotIndex, int incubatorIndex);

		void OnClickFinishIncubating(int slotIndex, int incubatorIndex, int itemIndex);

		void OnClickChangeIncubator(int slotIndex);

		void OnClickAddSlot(int slotIndex);

		void OnClickEggRewardPopup(int eggIndex);

		void RequestAddSlot(int slotIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Pet/PetIncubatorManageWindow";

	public UICreatureWidget CreatureWidget;

	public IncubatorSlotListPanel SlotListPanel;

	public IncubatorSlotStatePanel SlotStatePanel;

	public PetEggListPanel PetEggListPanel;

	public IncubatorProfilePanel IncubatorProfilePanel;

	public UIWidget Widget_ButtonRoot;

	public UIToggle Toggle_BtnIncubator;

	public UIToggle Toggle_BtnEgg;

	private IListener _listener;

	private int _selectedSlotIndex;

	private PetIncubatorSlotInfo _selectedSlotInfo;

	private int _buySlotIndex;

	private PetIncubatorObjectInfo _incubatorObjectInfo;

	private ItemIconComponent _selectedItemIconComp;

	public void Open(IListener listener, int slotIndex)
	{
		base.Open();
		_buySlotIndex = 0;
		_selectedItemIconComp = null;
		_listener = listener;
		_selectedSlotIndex = slotIndex;
		PetIncubatorSlotInfo selectedSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(_selectedSlotIndex);
		_selectedSlotInfo = selectedSlotInfo;
		Init();
		InitIncubatorManageUI();
	}

	private void InitIncubatorManageUI()
	{
		Init();
		InitCreatureWidgetComponent();
		OpenPanels();
		RefreshBtnTab();
	}

	private void OpenPanels()
	{
		SlotListPanel.Open(this, this);
		SlotStatePanel.Open(this, this);
		IncubatorProfilePanel.Open(this);
		PetEggListPanel.Open(this, this, this);
	}

	private void RefreshBtnTab()
	{
		UnSetSelectedItemIcon();
		EIncubatingState selectedIncubatingState = GetSelectedIncubatingState();
		bool flag = false;
		switch (selectedIncubatingState)
		{
		case EIncubatingState.None:
		case EIncubatingState.EmptyEgg:
		case EIncubatingState.SelectEgg:
			flag = false;
			break;
		case EIncubatingState.Incubating:
		case EIncubatingState.Complete:
			flag = true;
			break;
		}
		if (flag)
		{
			Toggle_BtnEgg.Set(state: true);
		}
		else
		{
			Toggle_BtnIncubator.Set(state: true);
		}
		EnableTabRoot(flag);
	}

	private void InitCreatureWidgetComponent()
	{
		if (CreatureWidget != null)
		{
			CreatureWidget.Close();
			_incubatorObjectInfo = null;
		}
		if (_selectedSlotInfo != null)
		{
			PetIncubatorData data = GameRoot.Instance.PetIncubatorDataContainer.GetData(_selectedSlotInfo.MountingIndex);
			if (data != null)
			{
				CreatureData dataByCode = GameRoot.Instance.CreatureDataContainer.GetDataByCode(data.CreatureCode);
				CreatureWidget.Open(dataByCode, 5, 0, CreatureStock.ELoadLevel.IncludeFacial, 0f);
				CreatureWidget.SetLocalPosition(data.CreaturePosition);
				CreatureWidget.SetLocalRotation(data.CreatureRotation);
				CreatureWidget.SetLocalScale(data.CreatureScale);
				PetIncubatorObjectInfo.IncubatorStateType incubatorState = PetHouseObjectManager.GetIncubatorState(_selectedSlotInfo);
				PetIncubatorObjectInfo petIncubatorObjectInfo = new PetIncubatorObjectInfo();
				petIncubatorObjectInfo.SlotIndex = _selectedSlotInfo.SlotIndex;
				petIncubatorObjectInfo.SlotInfo = _selectedSlotInfo;
				petIncubatorObjectInfo.RenderFacade = CreatureWidget.RenderFacade;
				petIncubatorObjectInfo.PlayIncubatorActionByState(incubatorState);
				_incubatorObjectInfo = petIncubatorObjectInfo;
			}
		}
	}

	private void RefreshIncubatorManageUI()
	{
		InitCreatureWidgetComponent();
		RefreshBtnTab();
		RefreshAllPanels();
	}

	private void RefreshAllPanels()
	{
		SlotListPanel.RefreshSlotComponent();
		SlotStatePanel.SetIncubatorStateUI();
		IncubatorProfilePanel.SetIncubatorProfile();
		PetEggListPanel.RefreshByState();
	}

	public void ChangeIncubatorState(PetIncubatorObjectInfo.IncubatorStateType incubatorStateType, Action onEnd = null, bool needInfoUpdate = false)
	{
		if (_incubatorObjectInfo != null)
		{
			_incubatorObjectInfo.PlayIncubatorActionByState(incubatorStateType, onEnd, needInfoUpdate);
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_incubatorObjectInfo != null)
		{
			_incubatorObjectInfo.UpdateIncubatorState();
		}
		SlotListPanel.UpdtaeSlotGauge();
		EIncubatingState selectedIncubatingState = GetSelectedIncubatingState();
		if (selectedIncubatingState == EIncubatingState.Incubating || selectedIncubatingState == EIncubatingState.Complete)
		{
			SlotStatePanel.SetIncubatorStateUI();
			PetEggListPanel.RefreshByState();
		}
	}

	public void EnableTouch(bool isEnable)
	{
		if (!isEnable)
		{
			AddTouchDisabler("PlaySuccessAction");
		}
		else
		{
			RemoveTouchDisabler("PlaySuccessAction");
		}
	}

	public override void Close()
	{
		base.Close();
		CreatureWidget.Close();
		_incubatorObjectInfo = null;
		CloseAllPanel();
	}

	public void CloseAllPanel()
	{
		foreach (IIncubatorManagePanel petPanel in GetPetPanels())
		{
			petPanel?.Close();
		}
	}

	public IEnumerable<IIncubatorManagePanel> GetPetPanels()
	{
		yield return SlotListPanel;
		yield return SlotStatePanel;
		yield return PetEggListPanel;
		yield return IncubatorProfilePanel;
	}

	private void EnableTabRoot(bool eggTabSelected)
	{
		IncubatorProfilePanel.EnableRoot(!eggTabSelected);
		PetEggListPanel.EnableRoot(eggTabSelected);
	}

	private void UnSetSelectedItemIcon()
	{
		if (_selectedItemIconComp != null)
		{
			_selectedItemIconComp.SetSelected(selected: false);
		}
		_selectedItemIconComp = null;
	}

	public void OnClickIncubatorTab()
	{
		EnableTabRoot(eggTabSelected: false);
	}

	public void OnClickEggListTab()
	{
		EnableTabRoot(eggTabSelected: true);
	}

	public void OnEndStartIncubating()
	{
		ChangeIncubatorState(PetIncubatorObjectInfo.IncubatorStateType.Phase1Start, null, needInfoUpdate: true);
		PetIncubatorSlotInfo selectedSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(_selectedSlotIndex);
		_selectedSlotInfo = selectedSlotInfo;
		_selectedItemIconComp = null;
		PetEggListPanel.SetEggList();
		RefreshAllPanels();
	}

	public void OnEndCancelIncubating()
	{
		ChangeIncubatorState(PetIncubatorObjectInfo.IncubatorStateType.Phase0, null, needInfoUpdate: true);
		PetIncubatorSlotInfo selectedSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(_selectedSlotIndex);
		_selectedSlotInfo = selectedSlotInfo;
		PetEggListPanel.SetEggList();
		RefreshAllPanels();
	}

	public void OnCompleteAddSlot()
	{
		if (_buySlotIndex != 0 && _listener != null)
		{
			if (_listener != null)
			{
				_listener.RequestAddSlot(_buySlotIndex);
			}
			_buySlotIndex = 0;
		}
	}

	public void OnSuccessAddIncubatorSlot(int slotIndex)
	{
		_selectedSlotIndex = slotIndex;
		PetIncubatorSlotInfo selectedSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(_selectedSlotIndex);
		_selectedSlotInfo = selectedSlotInfo;
		UnSetSelectedItemIcon();
		RefreshIncubatorManageUI();
	}

	public void OnStartPlayFinishIncubating(Action onEnd)
	{
		ChangeIncubatorState(PetIncubatorObjectInfo.IncubatorStateType.Open, onEnd, needInfoUpdate: true);
		PetIncubatorSlotInfo selectedSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(_selectedSlotIndex);
		_selectedSlotInfo = selectedSlotInfo;
		_selectedItemIconComp = null;
		RefreshAllPanels();
	}

	public void OnEndPlayFinishIncubating()
	{
		ChangeIncubatorState(PetIncubatorObjectInfo.IncubatorStateType.Phase0, null, needInfoUpdate: true);
	}

	int IIncubatorDataProvider.GetSelectedEggIndex()
	{
		if (_selectedItemIconComp == null)
		{
			return -1;
		}
		return _selectedItemIconComp.ItemIndex;
	}

	int IIncubatorDataProvider.GetSelectedSlotIndex()
	{
		return _selectedSlotIndex;
	}

	PetIncubatorSlotInfo IIncubatorDataProvider.GetSelectedSlotInfo()
	{
		return _selectedSlotInfo;
	}

	EIncubatingState IIncubatorDataProvider.GetIncubatingState()
	{
		return GetSelectedIncubatingState();
	}

	private EIncubatingState GetSelectedIncubatingState()
	{
		if (_selectedSlotInfo == null || _selectedSlotInfo.MountingIndex == 0)
		{
			return EIncubatingState.None;
		}
		if (_selectedSlotInfo.SetItemIndex == 0)
		{
			if (_selectedItemIconComp == null)
			{
				return EIncubatingState.EmptyEgg;
			}
			return EIncubatingState.SelectEgg;
		}
		if (PetHelper.GetIncubatorRemainTime(_selectedSlotInfo).Value.TotalSeconds <= 0.0)
		{
			return EIncubatingState.Complete;
		}
		return EIncubatingState.Incubating;
	}

	void PetIncubatorSlotIconComponent.IListener.OnClick(PetIncubatorSlotIconComponent slotComponent)
	{
		switch (slotComponent.SlotOpenState)
		{
		case PetIncubatorSlotIconComponent.ESlotOpenState.CanOpen:
			if (_listener != null)
			{
				int slotIndex = (_buySlotIndex = slotComponent.SlotIndex);
				_listener.OnClickAddSlot(slotIndex);
			}
			break;
		case PetIncubatorSlotIconComponent.ESlotOpenState.Open:
			_selectedSlotIndex = slotComponent.SlotIndex;
			_selectedSlotInfo = slotComponent.SlotInfo;
			UnSetSelectedItemIcon();
			RefreshIncubatorManageUI();
			break;
		}
	}

	void IncubatorSlotStatePanel.IListener.OnClickRewardInfo()
	{
		if (_selectedSlotInfo == null || _selectedSlotInfo.MountingIndex == 0)
		{
			return;
		}
		int num = 0;
		if (_selectedSlotInfo.SetItemIndex == 0)
		{
			if (_selectedItemIconComp == null)
			{
				return;
			}
			num = _selectedItemIconComp.ItemIndex;
		}
		else
		{
			num = _selectedSlotInfo.SetItemIndex;
		}
		if (_listener != null)
		{
			_listener.OnClickEggRewardPopup(num);
		}
	}

	void IncubatorSlotStatePanel.IListener.OnClickChangeIncubator()
	{
		if (_selectedSlotInfo != null && _selectedSlotInfo.MountingIndex != 0 && _listener != null)
		{
			_listener.OnClickChangeIncubator(_selectedSlotInfo.SlotIndex);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (!(_selectedItemIconComp != null) || _selectedItemIconComp.ItemIndex != itemIconComponent.ItemIndex)
		{
			if (_selectedItemIconComp != null)
			{
				_selectedItemIconComp.SetSelected(selected: false);
			}
			_selectedItemIconComp = itemIconComponent;
			_selectedItemIconComp.SetSelected(selected: true);
			PetEggListPanel.EnableIncubatingBtn(isEnable: true);
			SlotStatePanel.SetIncubatorStateUI();
		}
	}

	void PetEggListPanel.IListener.OnClickStartIncubating()
	{
		if (_listener != null && _selectedSlotInfo != null && !(_selectedItemIconComp == null))
		{
			_listener.OnClickStartIncubating(_selectedSlotInfo.SlotIndex, _selectedSlotInfo.MountingIndex, _selectedItemIconComp.ItemIndex);
		}
	}

	void PetEggListPanel.IListener.OnClickCancel()
	{
		if (_selectedSlotInfo != null && _selectedSlotInfo.MountingIndex != 0 && _listener != null)
		{
			_listener.OnClickCancelIncubating(_selectedSlotInfo.SlotIndex, _selectedSlotInfo.MountingIndex);
		}
	}

	void PetEggListPanel.IListener.OnClickComplete()
	{
		if (_selectedSlotInfo != null && _selectedSlotInfo.MountingIndex != 0 && _listener != null)
		{
			_listener.OnClickFinishIncubating(_selectedSlotInfo.SlotIndex, _selectedSlotInfo.MountingIndex, _selectedSlotInfo.SetItemIndex);
		}
	}
}
