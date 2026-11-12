using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class IncubatorSlotListPanel : MonoBehaviour, IIncubatorManagePanel
{
	public UIScrollGrid IncubatorSlotListGrid;

	private List<PetIncubatorSlotIconComponent> _petSlotCompList = new List<PetIncubatorSlotIconComponent>();

	private IIncubatorDataProvider _dataProvider;

	private PetIncubatorSlotIconComponent.IListener _slotComplistener;

	public void Open(IIncubatorDataProvider dataProvider, PetIncubatorSlotIconComponent.IListener slotCompListener)
	{
		_dataProvider = dataProvider;
		_slotComplistener = slotCompListener;
		SetSlotComp();
	}

	public void Close()
	{
		_dataProvider = null;
		_slotComplistener = null;
		FactoryBase.ReleaseAndClearList(_petSlotCompList);
	}

	private void SetSlotComp()
	{
		int selectedSlotIndex = _dataProvider.GetSelectedSlotIndex();
		FactoryBase.ReleaseAndClearList(_petSlotCompList);
		List<PetIncubatorSlotData> allDataByOpenNum = GameRoot.Instance.PetIncubatorSlotDataContainer.GetAllDataByOpenNum();
		if (allDataByOpenNum == null)
		{
			return;
		}
		bool flag = true;
		foreach (PetIncubatorSlotData item in allDataByOpenNum)
		{
			int index = item.Index;
			PetIncubatorSlotIconComponent petIncubatorSlotIconComponent = UIHelper.AddGridItem(IncubatorSlotListGrid, PetIncubatorSlotIconComponent.PrefabAuto);
			PetIncubatorSlotIconComponent.ESlotOpenState slotOpenState = PetIncubatorSlotIconComponent.ESlotOpenState.Open;
			PetIncubatorSlotInfo petIncubatorSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(index);
			if (petIncubatorSlotInfo == null)
			{
				slotOpenState = (flag ? PetIncubatorSlotIconComponent.ESlotOpenState.CanOpen : PetIncubatorSlotIconComponent.ESlotOpenState.Disable);
				flag = false;
			}
			petIncubatorSlotIconComponent.Open(_slotComplistener, index, petIncubatorSlotInfo, slotOpenState);
			petIncubatorSlotIconComponent.SetSelection(selectedSlotIndex == index);
			_petSlotCompList.Add(petIncubatorSlotIconComponent);
		}
		UIHelper.RepositionGrid(IncubatorSlotListGrid);
	}

	public void RefreshSlotComponent()
	{
		int selectedSlotIndex = _dataProvider.GetSelectedSlotIndex();
		if (GameRoot.Instance.PetIncubatorSlotDataContainer.GetAllDataByOpenNum() == null)
		{
			return;
		}
		bool flag = true;
		foreach (PetIncubatorSlotIconComponent petSlotComp in _petSlotCompList)
		{
			int slotIndex = petSlotComp.SlotIndex;
			PetIncubatorSlotIconComponent.ESlotOpenState slotOpenState = PetIncubatorSlotIconComponent.ESlotOpenState.Open;
			PetIncubatorSlotInfo petIncubatorSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(slotIndex);
			if (petIncubatorSlotInfo == null)
			{
				slotOpenState = (flag ? PetIncubatorSlotIconComponent.ESlotOpenState.CanOpen : PetIncubatorSlotIconComponent.ESlotOpenState.Disable);
				flag = false;
			}
			petSlotComp.ResetPortrait(petIncubatorSlotInfo, slotOpenState);
			petSlotComp.SetSelection(selectedSlotIndex == slotIndex);
		}
	}

	public void UpdtaeSlotGauge()
	{
		if (_petSlotCompList == null)
		{
			return;
		}
		foreach (PetIncubatorSlotIconComponent petSlotComp in _petSlotCompList)
		{
			petSlotComp.SetGauge();
		}
	}
}
