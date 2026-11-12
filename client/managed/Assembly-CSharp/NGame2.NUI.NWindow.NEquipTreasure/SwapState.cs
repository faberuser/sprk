using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow.NEquipTreasure;

public class SwapState : NGame2.NUI.NWindow.NEquipTreasure.ITreasureSlotStateHandler
{
	private HeroEquipTreasure.Listener _listener;

	private int _targetEquipSlotIndex;

	private CachedHeroInfo _selectedCachedHeroInfo;

	private List<TreasureSlotComponent> _treasureComponentList;

	public SwapState(HeroEquipTreasure.Listener listener, int targetEquipSlotIndex, CachedHeroInfo selectedCachedHeroInfo, List<TreasureSlotComponent> treasureComponentList)
	{
		_listener = listener;
		_selectedCachedHeroInfo = selectedCachedHeroInfo;
		_targetEquipSlotIndex = targetEquipSlotIndex;
		_treasureComponentList = treasureComponentList;
	}

	public void OnChangeState(TreasureSlotComponent treasureSlotComponent)
	{
		if (!(treasureSlotComponent == null))
		{
			treasureSlotComponent.SetSwapState(_targetEquipSlotIndex);
		}
	}

	public void OnSelect(TreasureSlotComponent selectedTreasureSlot)
	{
		OnSwap(selectedTreasureSlot);
	}

	public bool ShowSelectPanel()
	{
		return true;
	}

	private void OnSwap(TreasureSlotComponent selectedTreasureSlot)
	{
		if (_listener == null || selectedTreasureSlot == null || !selectedTreasureSlot.isSelectableSlot)
		{
			return;
		}
		if (!selectedTreasureSlot.IsEquippedSlot())
		{
			VespaDebug.LogError("is invalid slot");
			return;
		}
		TreasureSlotComponent targetSlotTreasureComponent = _treasureComponentList.Find((TreasureSlotComponent component) => component.IsValidSlot() && component.EquipSlotIndex == _targetEquipSlotIndex);
		if (targetSlotTreasureComponent == null || !targetSlotTreasureComponent.IsValidSlot())
		{
			VespaDebug.LogError("targetSlotTreasureComponent is invalid slot");
		}
		else if (!CanSwapTargetToSelected(targetSlotTreasureComponent, selectedTreasureSlot))
		{
			OpenSwapConfirmPopup(delegate
			{
				RequestSwap(selectedTreasureSlot, targetSlotTreasureComponent);
			});
		}
		else if (targetSlotTreasureComponent.SlotInfo.EquipItemInfo == null)
		{
			int itemSlotIndex = selectedTreasureSlot.EquipItemInfo.SlotIndex;
			_listener.OnClickUnsetSlot(new int[1] { itemSlotIndex }, _selectedCachedHeroInfo, delegate
			{
				_listener.OnClickEquipSlot(itemSlotIndex, _selectedCachedHeroInfo.HeroIndex, _targetEquipSlotIndex);
			});
		}
		else
		{
			RequestSwap(targetSlotTreasureComponent, selectedTreasureSlot);
		}
	}

	private void RequestSwap(TreasureSlotComponent selectedTreasureSlot, TreasureSlotComponent targetSlotTreasureComponent)
	{
		if (_listener != null)
		{
			int slotIndex = targetSlotTreasureComponent.EquipItemInfo.SlotIndex;
			int equipSlotIndex = targetSlotTreasureComponent.EquipSlotIndex;
			int slotIndex2 = selectedTreasureSlot.EquipItemInfo.SlotIndex;
			int equipSlotIndex2 = selectedTreasureSlot.EquipSlotIndex;
			_listener.OnClickSwapSlot(new int[2] { slotIndex, slotIndex2 }, _selectedCachedHeroInfo.HeroIndex, new int[2] { equipSlotIndex2, equipSlotIndex });
		}
	}

	private bool CanSwapTargetToSelected(TreasureSlotComponent targetSlotTreasureComponent, TreasureSlotComponent selectedTreasureSlot)
	{
		if (targetSlotTreasureComponent.EquipItemInfo == null)
		{
			return true;
		}
		return targetSlotTreasureComponent.IsSwapAbleSlot(selectedTreasureSlot.EquipSlotIndex);
	}

	private void OpenSwapConfirmPopup(Action onConfirm)
	{
		string stringDirect = LocalizationManager.GetStringDirect("TREASURE_RELEASE_SAME_SUBTYPE_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("TREASURE_RELEASE_GENERAL_TREASURE_DESC");
		MessagePopup.Open(stringDirect, stringDirect2, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onConfirm);
		}, null);
	}
}
