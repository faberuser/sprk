using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NPet;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class PetIncubatorPopup : BaseWindow, PetIncubatorComponent.IListener
{
	public interface IListener
	{
		void OnClickEquipIncubator(int incubatorindex, int slotIndex, Action<PetIncubatorSlotInfo> onSuccess);

		void OnClickBuyIncubator(int incubatorindex, int slotIndex, Action<PetIncubatorInfo> onSuccess);
	}

	public static string PrefabPath = "Prefab2/Window/Pet/Popup/PetIncubatorPopup";

	public UIGrid ChangeList_Grid;

	public UIScrollView ChangeListScrollView;

	private IListener _listener;

	private List<PetIncubatorComponent> _compList;

	private int _slotIndex;

	private int _equippedIndex;

	public void Open(IListener listener, int slotIndex)
	{
		base.Open();
		_listener = listener;
		_slotIndex = slotIndex;
		_equippedIndex = -1;
		InitIncubatorList();
	}

	private void InitIncubatorList()
	{
		IList<PetIncubatorData> allData = GameRoot.Instance.PetIncubatorDataContainer.GetAllData();
		PetIncubatorSlotInfo petIncubatorSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(_slotIndex);
		int num = __Extension_Method_ICollection__.COUNT(allData);
		_compList = new List<PetIncubatorComponent>(num);
		for (int i = 0; i < num; i++)
		{
			PetIncubatorData petIncubatorData = allData[i];
			bool flag = petIncubatorSlotInfo != null && petIncubatorSlotInfo.MountingIndex == petIncubatorData.Index;
			bool isOwned = flag || GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorManager.Get(_slotIndex, petIncubatorData.Index) != null;
			_equippedIndex = (flag ? i : _equippedIndex);
			PetIncubatorComponent petIncubatorComponent = PetIncubatorComponent.PrefabAuto.Factory.Create(ChangeList_Grid.transform);
			petIncubatorComponent.Open(this, petIncubatorData.Index, flag, isOwned);
			_compList.Add(petIncubatorComponent);
		}
		ChangeList_Grid.Reposition();
		ChangeListScrollView.ResetPosition();
		if (_equippedIndex != -1)
		{
			Vector3 localPosition = _compList[_equippedIndex].transform.localPosition;
			UIHelper.MoveGridPositionToSelectedItem(ChangeList_Grid, localPosition);
			ChangeListScrollView.RestrictWithinBounds(instant: true);
		}
	}

	public void EquipRefresh(PetIncubatorSlotInfo petIncubatorSlotInfo)
	{
		if (petIncubatorSlotInfo == null)
		{
			VespaDebug.LogError("PetIncubatorSlotInfo is null");
			return;
		}
		if (_slotIndex != petIncubatorSlotInfo.SlotIndex)
		{
			VespaDebug.LogError("SlotIndex is not match");
			return;
		}
		if (_equippedIndex != -1)
		{
			_compList[_equippedIndex].Refresh(isUsed: false, isOwned: true);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_compList); i++)
		{
			PetIncubatorComponent petIncubatorComponent = _compList[i];
			if (petIncubatorComponent.PetIncubatorData.Index == petIncubatorSlotInfo.MountingIndex)
			{
				petIncubatorComponent.Refresh(isUsed: true, isOwned: true);
				_equippedIndex = i;
				break;
			}
		}
	}

	public void PurchaseRefresh(PetIncubatorInfo petIncubatorInfo)
	{
		if (petIncubatorInfo == null)
		{
			VespaDebug.LogError("PetIncubatorInfo is null");
			return;
		}
		if (_slotIndex != petIncubatorInfo.SlotIndex)
		{
			VespaDebug.LogError("SlotIndex is not match");
			return;
		}
		foreach (PetIncubatorComponent comp in _compList)
		{
			if (comp.PetIncubatorData.Index == petIncubatorInfo.IncubatorIndex)
			{
				comp.Refresh(isUsed: false, isOwned: true);
			}
			else
			{
				comp.Refresh(comp.IsUsed, comp.IsOwned);
			}
		}
	}

	void PetIncubatorComponent.IListener.OnClickEquip(int incubatorIndex)
	{
		if (_listener != null)
		{
			_listener.OnClickEquipIncubator(incubatorIndex, _slotIndex, delegate(PetIncubatorSlotInfo petIncubatorSlotInfo)
			{
				EquipRefresh(petIncubatorSlotInfo);
			});
		}
	}

	void PetIncubatorComponent.IListener.OnClickPurchase(int incubatorIndex)
	{
		if (_listener != null)
		{
			_listener.OnClickBuyIncubator(incubatorIndex, _slotIndex, delegate(PetIncubatorInfo petIncubatorInfo)
			{
				PurchaseRefresh(petIncubatorInfo);
			});
		}
	}

	public override void Close()
	{
		ClearComponentList();
		base.Close();
	}

	private void ClearComponentList()
	{
		if (_compList != null)
		{
			FactoryBase.ReleaseAndClearList(_compList);
		}
	}
}
