using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow.NEquipTreasure;

public class EquipState : NGame2.NUI.NWindow.NEquipTreasure.ITreasureSlotStateHandler
{
	private HeroEquipTreasure.Listener _listener;

	private CachedHeroInfo _selectedCachedHeroInfo;

	private EquipItemInfo _toEquipItemInfo;

	public EquipState(HeroEquipTreasure.Listener listener, CachedHeroInfo selectedCachedHeroInfo, EquipItemInfo equipItemInfo)
	{
		_listener = listener;
		_selectedCachedHeroInfo = selectedCachedHeroInfo;
		_toEquipItemInfo = equipItemInfo;
		if (listener == null)
		{
			VespaDebug.LogError("listener must be not null");
		}
		if (_toEquipItemInfo == null)
		{
			VespaDebug.LogError("toEquipItemInfo must be not null");
		}
	}

	public void OnChangeState(TreasureSlotComponent component)
	{
		if (!(component == null))
		{
			component.SetEquipState(_toEquipItemInfo);
		}
	}

	public void OnSelect(TreasureSlotComponent component)
	{
		OnEquip(component);
	}

	public bool ShowSelectPanel()
	{
		return true;
	}

	private void OnEquip(TreasureSlotComponent selectedSlotComponent)
	{
		if (_listener == null || _toEquipItemInfo == null || selectedSlotComponent == null)
		{
			return;
		}
		if (!selectedSlotComponent.isSelectableSlot || !selectedSlotComponent.IsValidSlot())
		{
			VespaDebug.LogError("is invalid slot");
			return;
		}
		string duplicationItemname = string.Empty;
		switch (EquipSlotHelper.CanEquip(_toEquipItemInfo, selectedSlotComponent.EquipSlotIndex, _selectedCachedHeroInfo, ref duplicationItemname))
		{
		case EquipSlotHelper.CanEquipResult.CanEquip:
			_listener.OnClickEquipSlot(_toEquipItemInfo.SlotIndex, _selectedCachedHeroInfo.HeroIndex, selectedSlotComponent.EquipSlotIndex);
			break;
		case EquipSlotHelper.CanEquipResult.SubtypeDuplication:
			OpenDuplicationConfirmPopup(selectedSlotComponent.EquipSlotIndex, duplicationItemname);
			break;
		}
	}

	private void OpenDuplicationConfirmPopup(int equipSlotIndex, string duplicationoItemName)
	{
		string stringDirect = LocalizationManager.GetStringDirect("TREASURE_RELEASE_SAME_SUBTYPE_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("TREASURE_RELEASE_SAME_SUBTYPE_DESC", duplicationoItemName);
		MessagePopup.Open(stringDirect, stringDirect2, delegate
		{
			if (_listener != null)
			{
				_listener.OnClickEquipSlot(_toEquipItemInfo.SlotIndex, _selectedCachedHeroInfo.HeroIndex, equipSlotIndex);
			}
		}, null);
	}
}
