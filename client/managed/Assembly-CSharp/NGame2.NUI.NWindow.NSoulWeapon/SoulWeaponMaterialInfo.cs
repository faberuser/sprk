using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class SoulWeaponMaterialInfo : ItemIconComponent.Listener
{
	public interface IListener
	{
		void SelectMaterial();
	}

	public Transform SoulWeaponRootTranform;

	public Transform MaterialRootTransform;

	public UILabel SoulWeaponCountLabel;

	public UILabel MaterialCountLabel;

	private IListener _iListener;

	private ItemIconComponent _soulWeaponItemIconComponent;

	private ItemIconComponent _materialItemIconComponent;

	private bool _isPossibleUpgrade;

	public bool IsMoreMyItemCount => _isPossibleUpgrade;

	public void Open(IListener iListener, int equipItemSlotIndex, SoulWeaponInfo soulWeaponInfo, bool enableItemCount)
	{
		Init(iListener, equipItemSlotIndex, soulWeaponInfo, enableItemCount);
	}

	public void Close()
	{
		_iListener = null;
		FactoryBase.SafeReleaseComponent(ref _soulWeaponItemIconComponent);
		FactoryBase.SafeReleaseComponent(ref _materialItemIconComponent);
	}

	private void Init(IListener iListener, int equipItemSlotIndex, SoulWeaponInfo soulWeaponInfo, bool enableItemCount)
	{
		_iListener = iListener;
		_isPossibleUpgrade = false;
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipItemSlotIndex);
		if (equipItemInfo != null)
		{
			SetSoulWeaponMaterialInfo(equipItemInfo, enableItemCount: false);
			SetMertialInfo(soulWeaponInfo, enableItemCount);
		}
	}

	private void SetSoulWeaponMaterialInfo(EquipItemInfo equipItemInfo, bool enableItemCount)
	{
		SetMaterialCount(ref SoulWeaponCountLabel, enableItemCount, 0, 1);
		SetItemIconComponent(ref _soulWeaponItemIconComponent, ref SoulWeaponRootTranform);
		if (_soulWeaponItemIconComponent == null)
		{
			return;
		}
		_soulWeaponItemIconComponent.Open(equipItemInfo, (_iListener != null) ? this : null);
		if (enableItemCount)
		{
			int materialCount = 0;
			if (GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager != null)
			{
				materialCount = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetSpecificSpecialWeaponCount(equipItemInfo.ItemIndex);
			}
			SetMaterialCount(ref SoulWeaponCountLabel, enableItemCount, materialCount, 1);
		}
	}

	private void SetMertialInfo(SoulWeaponInfo soulWeaponInfo, bool enableItemCount)
	{
		SetMaterialCount(ref MaterialCountLabel, enableItemCount, 0, 0);
		if (soulWeaponInfo == null)
		{
			return;
		}
		SoulWeaponUpgradePriceData soulWeaponUpgradePriceData = soulWeaponInfo.GetSoulWeaponUpgradePriceData();
		if (soulWeaponUpgradePriceData == null)
		{
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(soulWeaponUpgradePriceData.MaterialItemCode);
		if (dataByCode == null)
		{
			return;
		}
		SetItemIconComponent(ref _materialItemIconComponent, ref MaterialRootTransform);
		if (!(_materialItemIconComponent == null))
		{
			_materialItemIconComponent.Open(dataByCode, (_iListener != null) ? this : null);
			if (enableItemCount)
			{
				int myMaterialItemCount = GetMyMaterialItemCount(soulWeaponInfo);
				int materialItemCount = soulWeaponUpgradePriceData.MaterialItemCount;
				SetMaterialCount(ref MaterialCountLabel, enableItemCount, myMaterialItemCount, materialItemCount);
				_isPossibleUpgrade = myMaterialItemCount >= materialItemCount;
			}
		}
	}

	private void SetItemIconComponent(ref ItemIconComponent itemIconComponent, ref Transform rootTransform)
	{
		if (!(rootTransform == null) && itemIconComponent == null)
		{
			itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(rootTransform);
			if (!(itemIconComponent == null))
			{
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
			}
		}
	}

	private void SetMaterialCount(ref UILabel countLabel, bool enable, int materialCount, int reqMaterialCount)
	{
		UIHelper.SetLabelText(countLabel, "-");
		if (!enable)
		{
			UIHelper.EnableGameObject(countLabel, enable: false);
			return;
		}
		UIHelper.EnableGameObject(countLabel, enable: true);
		if (reqMaterialCount > 0)
		{
			UIHelper.SetLabelText(countLabel, materialCount + " / " + reqMaterialCount);
		}
	}

	public int GetMyMaterialItemCount(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo == null)
		{
			return 0;
		}
		SoulWeaponUpgradePriceData soulWeaponUpgradePriceData = soulWeaponInfo.GetSoulWeaponUpgradePriceData();
		if (soulWeaponUpgradePriceData == null)
		{
			return 0;
		}
		if (GameRoot.Instance.ItemDataContainer.GetDataByCode(soulWeaponUpgradePriceData.MaterialItemCode) == null)
		{
			return 0;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(soulWeaponUpgradePriceData.MaterialItemCode);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (!(itemIconComponent == null) && _iListener != null)
		{
			_iListener.SelectMaterial();
		}
	}
}
