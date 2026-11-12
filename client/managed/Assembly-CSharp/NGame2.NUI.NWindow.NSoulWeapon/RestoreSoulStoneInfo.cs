using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class RestoreSoulStoneInfo
{
	public Transform SoulStoneItemIconComponentRootTransform;

	public UILabel SoulStoneNameLabel;

	public UILabel SoulStoneItemCountLabel;

	private ItemData _soulStoneItemData;

	private ItemIconComponent _soulStoneItemIconComponent;

	public bool IsPossibleRestoreSoulStone { get; private set; }

	public int SoulStonItemIndex
	{
		get
		{
			if (_soulStoneItemData == null)
			{
				return 0;
			}
			return _soulStoneItemData.Index;
		}
	}

	public void Open()
	{
		Init();
	}

	public void Close()
	{
		_soulStoneItemData = null;
		FactoryBase.SafeReleaseComponent(ref _soulStoneItemIconComponent);
	}

	private void Init()
	{
		_soulStoneItemData = null;
		IsPossibleRestoreSoulStone = false;
		SetRestoreSoulStoneInfo();
		SetSoulStoneItemIconComponent();
	}

	private void SetRestoreSoulStoneInfo()
	{
		IList<SoulStoneRestoreData> allData = GameRoot.Instance.SoulStoneRestoreDataContainer.GetAllData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			return;
		}
		SoulStoneRestoreData soulStoneRestoreData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			soulStoneRestoreData = allData[i];
			if (soulStoneRestoreData != null)
			{
				break;
			}
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(soulStoneRestoreData.MaterialItemCode);
		if (dataByCode != null)
		{
			_soulStoneItemData = dataByCode;
		}
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(soulStoneRestoreData.MaterialItemCode);
		IsPossibleRestoreSoulStone = itemCount >= soulStoneRestoreData.MaterialItemCount;
		SetSoulStoneItemName(dataByCode.Name);
		SetSoulStoneItemCountLabel(itemCount, soulStoneRestoreData.MaterialItemCount);
	}

	private void SetSoulStoneItemName(string itemName)
	{
		UIHelper.SetLabelText(SoulStoneNameLabel, itemName);
	}

	private void SetSoulStoneItemCountLabel(int itemCount, int reqItemCount)
	{
		UIHelper.SetLabelText(SoulStoneItemCountLabel, itemCount + " / " + reqItemCount);
	}

	private void SetSoulStoneItemIconComponent()
	{
		if (SoulStoneItemIconComponentRootTransform == null)
		{
			return;
		}
		if (_soulStoneItemIconComponent == null)
		{
			_soulStoneItemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(SoulStoneItemIconComponentRootTransform);
			if (_soulStoneItemIconComponent == null)
			{
				return;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_soulStoneItemIconComponent.transform);
		}
		_soulStoneItemIconComponent.Open(_soulStoneItemData);
	}
}
