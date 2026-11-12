using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGame2.NUI.NComponent2.NItemIconSub;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class RuneItemDetailComponent : UIScrollItemBehaviour, EquipRuneSlotInfoComponent.IDataProvider, ItemIconComponent.Listener, InitOnCreate
{
	[Serializable]
	public class DualStatUiData
	{
		public UILabel Label_DualStatType;

		public UILabel Label_DualStatValue;
	}

	public interface Listener
	{
		void OnClick(ItemIconComponent component);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Rune/RuneItemDetailComponent";

	public static PrefabAuto<RuneItemDetailComponent> PrefabAuto = new PrefabAuto<RuneItemDetailComponent>("Resources/Prefab2/Component/Rune/RuneItemDetailComponent");

	public UILabel Label_ItemName;

	public UILabel Label_StatType;

	public UILabel Label_StatValue;

	public UILabel Label_Stat;

	public List<DualStatUiData> DualStatList = new List<DualStatUiData>();

	public Transform SingleStatRoot;

	public Transform DualStatRoot;

	public UISimpleGrid ItemGrid;

	public EquipRuneSlotInfoComponent EquipRuneSlotInfoComponent;

	private ItemIconComponent _itemIconComponent;

	private ItemData _itemData;

	private Listener _listener;

	[HideInInspector]
	public ItemIconComponent ItemIconComponent => _itemIconComponent;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public bool Open(ItemInfo itemInfo, ItemData itemData, Listener listener)
	{
		_listener = listener;
		_itemData = itemData;
		_itemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
		_itemIconComponent.Open(itemData, this);
		ItemIconComponent itemIconComponent = _itemIconComponent;
		int count = itemInfo.Count;
		Color? color = null;
		int num = count;
		itemIconComponent.SetItemCount(num, color);
		SetupRuneStat(itemData);
		return true;
	}

	public void SetupRuneStat(ItemData itemData)
	{
		UIHelper.SetLabelText(Label_ItemName, ItemNameBuilder.GetItemName(itemData));
		ItemData<RuneItemData> obj = itemData as ItemData<RuneItemData>;
		string empty = string.Empty;
		string empty2 = string.Empty;
		List<KeyValuePair<Enum, long>> list = obj.SubData.GetStatTypeAndValue().ToList();
		_ = list.Count;
		System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			empty = LocalizationManager.GetString(list[i].Key);
			empty2 = StatData.GetStatDisplayValue(list[i].Key, list[i].Value);
			if (i >= 1)
			{
				stringBuilder.Append("\n");
			}
			stringBuilder.Append(empty);
			stringBuilder.Append(" ");
			stringBuilder.Append(empty2);
		}
		UIHelper.SetLabelText(Label_Stat, stringBuilder.ToString());
		EquipRuneSlotInfoComponent.Open(this);
	}

	public void SetPopSubCompnent(int count, PopSubComponent.Listener listener = null)
	{
		_itemIconComponent.SetPopSubCompnent(count, listener);
	}

	public void SetItemCount(long count)
	{
		_itemIconComponent.SetItemCount(count);
	}

	public void SetSelected(bool selected)
	{
		_itemIconComponent.SetSelected(selected);
	}

	public void Close()
	{
		FactoryBase.ReleaseComponent(_itemIconComponent);
		_itemIconComponent = null;
	}

	public void OnClick()
	{
		_listener.OnClick(_itemIconComponent);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconCompnent)
	{
		_listener.OnClick(itemIconCompnent);
	}

	ItemData EquipRuneSlotInfoComponent.IDataProvider.GetItemData()
	{
		return _itemData;
	}
}
