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

public class PetIncubatorScene : RenewResourceWindow, IUserObserver<UserManager.UserStatInfo>, IUserObserver, PetIncubatorStateComponent.IListener
{
	public interface Listener
	{
		void OnClickFinishIncubating(PetIncubatorSlotInfo slotInfo);

		void OnClickIncubator(int slotIndex);

		void OnClose();

		void OnClickOpenPetTotalBuffPopup();

		void OnClickAddSlot(int slotIndex);

		void RequestAddSlot(int slotIndex);
	}

	[Serializable]
	public class IncubatorSlot
	{
		public int SlotIndex;

		public Transform Transform_Slot;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Pet/PetIncubatorScene";

	public IncubatorSlot[] IncubatorSlots;

	private List<PetIncubatorStateComponent> _slotComponetList = new List<PetIncubatorStateComponent>();

	private Listener _listener;

	private int _buySlotIndex;

	public override void ReOpen()
	{
		base.ReOpen();
		InitIncubatorUI();
	}

	public void Open(Listener listener)
	{
		_listener = listener;
		_buySlotIndex = 0;
		InitIncubatorUI();
		Init();
		base.Open();
	}

	public void InitIncubatorUI()
	{
		FactoryBase.ReleaseAndClearList(_slotComponetList);
		List<PetIncubatorSlotData> allDataByOpenNum = GameRoot.Instance.PetIncubatorSlotDataContainer.GetAllDataByOpenNum();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allDataByOpenNum))
		{
			VespaDebug.LogError("SlotData is Empty");
			return;
		}
		bool flag = true;
		foreach (PetIncubatorSlotData item in allDataByOpenNum)
		{
			if (item == null)
			{
				continue;
			}
			int index = item.Index;
			PetIncubatorSlotInfo petIncubatorSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(index);
			IncubatorSlot slot = GetSlot(index);
			if (slot != null)
			{
				PetIncubatorStateComponent petIncubatorStateComponent = PetIncubatorStateComponent.PrefabAuto.Factory.Create(slot.Transform_Slot);
				petIncubatorStateComponent.CanOpen = false;
				if (petIncubatorSlotInfo == null)
				{
					petIncubatorStateComponent.CanOpen = (flag ? true : false);
					flag = false;
				}
				petIncubatorStateComponent.Open(index, petIncubatorSlotInfo, this);
				_slotComponetList.Add(petIncubatorStateComponent);
			}
		}
	}

	private IncubatorSlot GetSlot(int slotIndex)
	{
		IncubatorSlot[] incubatorSlots = IncubatorSlots;
		foreach (IncubatorSlot incubatorSlot in incubatorSlots)
		{
			if (incubatorSlot.SlotIndex == slotIndex)
			{
				return incubatorSlot;
			}
		}
		return null;
	}

	public void RefreshComponents()
	{
		InitIncubatorUI();
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

	public void OnStartPlayOpenIncubator(int slotIndex)
	{
		foreach (PetIncubatorStateComponent slotComponet in _slotComponetList)
		{
			if (slotComponet.SlotIndex == slotIndex)
			{
				PetIncubatorSlotInfo slotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(slotIndex);
				slotComponet.RefreshIncubatorState(slotInfo);
				break;
			}
		}
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

	public override void Close()
	{
		if (!base.PreventTouchAnotherButton)
		{
			_listener.OnClose();
			base.Close();
			FactoryBase.ReleaseAndClearList(_slotComponetList);
		}
	}

	public override void OnSystemBackButton()
	{
		Close();
	}

	void PetIncubatorStateComponent.IListener.OnClickFinishIncubating(PetIncubatorSlotInfo petIncubatorSlotInfo)
	{
		if (_listener != null)
		{
			_listener.OnClickFinishIncubating(petIncubatorSlotInfo);
		}
	}

	void PetIncubatorStateComponent.IListener.OnClickIncubator(int slotIndex)
	{
		if (_listener != null)
		{
			_listener.OnClickIncubator(slotIndex);
		}
	}

	void PetIncubatorStateComponent.IListener.OnClickAddSlot(int slotIndex)
	{
		if (_listener != null)
		{
			_buySlotIndex = slotIndex;
			_listener.OnClickAddSlot(slotIndex);
		}
	}
}
