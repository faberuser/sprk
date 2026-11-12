using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroPresetStorageSlotComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	private enum EButtonState
	{
		FreeEmpty,
		Opened,
		Purchasable,
		PurchaeDisable
	}

	public interface IListener
	{
		void OnclickSlot(HeroPresetStorageSlotComponent caller);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Hero/HeroPresetStorageSlotComponent";

	public static PrefabAuto<HeroPresetStorageSlotComponent> PrefabAuto = new PrefabAuto<HeroPresetStorageSlotComponent>("Resources/Prefab2/Component/Hero/HeroPresetStorageSlotComponent");

	public GameObject GoEmptyRoot;

	public UIButton ButtonEmpty;

	public GameObject GoPurchasableRoot;

	public GameObject GoSlotRoot;

	public GameObject GoSelectedRoot;

	public GameObject GoLock;

	public UILabel LabelSlotName;

	public int SlotIndex;

	private IListener _listener;

	private string _storageKey;

	private string _slotName;

	public string GetStorageKey()
	{
		return _storageKey;
	}

	public void Open(HeroPresetStorageSlotData data, HeroPresetStorageInfo info, int slotIndex, IListener listener)
	{
		SlotIndex = slotIndex;
		_listener = listener;
		SetSlot(data, info);
	}

	private string GetName(HeroPresetStorageSlotData data, HeroPresetStorageInfo info)
	{
		if (info != null && !String_Extension_Method.IsNullOrEmpty(info.Name))
		{
			return info.Name;
		}
		return LocalizationManager.GetStringDirect(data.NameKey) + data.Index;
	}

	public void SetEmptyButton(bool isEnable)
	{
		UIHelper.EnableButton(ButtonEmpty, isEnable);
	}

	public void SetValues(string storageKey, string slotName)
	{
		_storageKey = storageKey;
		_slotName = slotName;
	}

	public void SetSlot(HeroPresetStorageSlotData data, HeroPresetStorageInfo info)
	{
		string storageKey = data.StorageKey;
		string slotName = GetName(data, info);
		bool flag = !data.IsPaid;
		bool flag2 = info == null;
		bool flag3 = !flag && flag2;
		EButtonState buttonState = GetButtonState(flag, flag2, data);
		SetSlot(storageKey, slotName, buttonState);
		SetLock(flag3);
	}

	private EButtonState GetButtonState(bool isFree, bool isEmpty, HeroPresetStorageSlotData data)
	{
		if (isFree)
		{
			return EButtonState.Opened;
		}
		if (isEmpty)
		{
			HeroPresetStorageSlotData prevPurchaseSlotData = GameRoot.Instance.HeroPresetStorageSlotDataContainer.GetPrevPurchaseSlotData(data);
			if (prevPurchaseSlotData != null && GlobalAutoBehaviour<AccountManager>.instance.HeroPresetStorageManager.Get(prevPurchaseSlotData.StorageKey) == null)
			{
				return EButtonState.PurchaeDisable;
			}
			return EButtonState.Purchasable;
		}
		return EButtonState.Opened;
	}

	private void SetSlot(string storageKey, string slotName, EButtonState buttonState)
	{
		SetValues(storageKey, slotName);
		UIHelper.EnableGameObject(GoEmptyRoot, buttonState == EButtonState.FreeEmpty || buttonState == EButtonState.PurchaeDisable);
		UIHelper.EnableGameObject(GoPurchasableRoot, buttonState == EButtonState.Purchasable);
		UIHelper.EnableGameObject(GoSlotRoot, buttonState == EButtonState.Opened);
		UIHelper.SetLabelText(LabelSlotName, slotName);
		UIHelper.EnableButton(ButtonEmpty, buttonState != EButtonState.PurchaeDisable);
	}

	public void SetLock(bool isLock)
	{
		UIHelper.EnableGameObject(GoLock, isLock);
	}

	public void SetSelected(bool isSeleted)
	{
		UIHelper.EnableGameObject(GoSelectedRoot, isSeleted);
	}

	public void OnClickSlot()
	{
		if (_listener != null)
		{
			_listener.OnclickSlot(this);
		}
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		_slotName = "";
		_storageKey = "";
		SetSelected(isSeleted: false);
	}
}
