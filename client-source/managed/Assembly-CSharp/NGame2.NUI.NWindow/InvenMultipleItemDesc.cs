using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NItemIconSub;
using NGame2.NUI.NManager;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InvenMultipleItemDesc : BaseWindow, ItemIconComponent.Listener, CurrencyIconComponent.Listener
{
	public interface Listener
	{
		void OnClickDetailButton(ItemData<CostumeSelectItemData> itemData);
	}

	public class ItemCompData
	{
		public ItemData itemData;

		public long itemAmount;
	}

	public class CurrencyData
	{
		public CurrencyType CurrencyType;

		public long CurrencyAmount;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/InvenMultipleItemDesc";

	[Header("[Desc Base]")]
	public UILabel Label_Title;

	public UIScrollView ItemScrollView;

	public UISimpleGrid MainItem;

	public UIButton Button_Detail;

	[Header("[Focus Item Info]")]
	public UILabel Label_ItemName;

	public UILabel Label_Desc;

	private Listener _listener;

	private List<ItemCompData> _itemDataList = new List<ItemCompData>();

	private List<CurrencyData> _currencyDataList = new List<CurrencyData>();

	private List<LinkFactoryComponent> _compList = new List<LinkFactoryComponent>();

	private LinkFactoryComponent _currcomp;

	public void Open(Listener listener, string title, List<ItemCompData> itemDatas = null, List<CurrencyData> currencyDatas = null, int selectIndex = 0)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(itemDatas) || !__Extension_Method_ICollection__.IsNullOrEmpty(currencyDatas))
		{
			_listener = listener;
			UIHelper.SetLabelText(Label_Title, title);
			Vespa_Generic_ICollection_Extension_Method.CLEAR(_itemDataList);
			Vespa_Generic_ICollection_Extension_Method.CLEAR(_currencyDataList);
			_itemDataList.AddRange(itemDatas);
			_currencyDataList.AddRange(currencyDatas);
			base.Open(delegate
			{
				Init(selectIndex);
			});
		}
	}

	public void Open(Listener listener, string title, List<ItemIconComponent> itemIconComp = null, List<CurrencyIconComponent> currencyIconComp = null, int selectIndex = 0)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(itemIconComp) && __Extension_Method_ICollection__.IsNullOrEmpty(currencyIconComp))
		{
			return;
		}
		_listener = listener;
		UIHelper.SetLabelText(Label_Title, title);
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_itemDataList);
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_currencyDataList);
		foreach (ItemIconComponent item3 in itemIconComp)
		{
			ItemData itemData = item3.ItemData;
			long itemCount = item3.ItemCount;
			if (itemData != null)
			{
				ItemCompData item = new ItemCompData
				{
					itemData = itemData,
					itemAmount = itemCount
				};
				_itemDataList.Add(item);
			}
		}
		foreach (CurrencyIconComponent item4 in currencyIconComp)
		{
			CurrencyType currencyType = item4.currencyType;
			long currencyAmount = item4._currencyAmount;
			int num = 0;
			if (currencyAmount > num)
			{
				CurrencyData item2 = new CurrencyData
				{
					CurrencyType = currencyType,
					CurrencyAmount = currencyAmount
				};
				_currencyDataList.Add(item2);
			}
		}
		base.Open(delegate
		{
			Init(selectIndex);
		});
	}

	public void Init(int selectIndex = 0)
	{
		FactoryBase.ReleaseAndClearList(_compList);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_itemDataList); i++)
		{
			ItemCompData itemCompData = _itemDataList[i];
			if (itemCompData != null)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(MainItem, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(itemCompData.itemData, this);
				itemIconComponent.SetSelected(selected: false, SelectSubComponent.ESelectType.Select_BorderBook);
				itemIconComponent.SetItemCount(itemCompData.itemAmount);
				_compList.Add(itemIconComponent);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_currencyDataList); j++)
		{
			CurrencyData currencyData = _currencyDataList[j];
			if (currencyData != null)
			{
				CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(MainItem, CurrencyIconComponent.PrefabAuto);
				currencyIconComponent.Open(currencyData.CurrencyType, currencyData.CurrencyAmount, this);
				currencyIconComponent.SetSelected(selected: false);
				_compList.Add(currencyIconComponent);
			}
		}
		if (selectIndex < 0 || selectIndex >= __Extension_Method_ICollection__.COUNT(_compList))
		{
			selectIndex = 0;
		}
		SelectComponent(_compList[selectIndex]);
		MainItem.RepositionGrid();
		ItemScrollView.ResetPosition();
	}

	public override void Close()
	{
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_itemDataList);
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_currencyDataList);
		FactoryBase.ReleaseAndClearList(_compList);
		base.Close();
	}

	public void OnClickDetailButton()
	{
		if (_listener == null || !(_currcomp is ItemIconComponent))
		{
			return;
		}
		ItemIconComponent itemIconComponent = _currcomp as ItemIconComponent;
		switch (itemIconComponent.ItemData.Type)
		{
		case ItemType.CostumeSelectItem:
		{
			ItemData<CostumeSelectItemData> data = GameRoot.Instance.ItemDataContainer.GetData<CostumeSelectItemData>(itemIconComponent.ItemData.Code);
			if (data != null)
			{
				_listener.OnClickDetailButton(data);
			}
			break;
		}
		case ItemType.AccessorySelectItem:
		{
			ItemData itemData = itemIconComponent.ItemData;
			SceneAutoBehaviour<InventoryManagement>.instance.OnUseAccessorySelectItem(itemData, disableConfime: true);
			break;
		}
		}
		Close();
	}

	public void SelectComponent(LinkFactoryComponent comp)
	{
		if (comp is ItemIconComponent)
		{
			ItemData itemData = (comp as ItemIconComponent).ItemData;
			if (itemData == null)
			{
				return;
			}
			UIHelper.SetLabelText(Label_ItemName, ItemNameBuilder.GetItemName(itemData));
			UIHelper.SetLabelText(Label_Desc, itemData.Description2);
			ItemType type = itemData.Type;
			if (type != ItemType.CostumeSelectItem && type != ItemType.AccessorySelectItem)
			{
				UIHelper.EnableButton(Button_Detail, enable: false);
			}
			else
			{
				UIHelper.EnableButton(Button_Detail, enable: true);
			}
		}
		else if (comp is CurrencyIconComponent)
		{
			CurrencyIconComponent currencyIconComponent = comp as CurrencyIconComponent;
			string key = $"TOOLTIP_INFO_NAME_{currencyIconComponent.currencyType.ToString().ToUpper()}";
			string key2 = $"TOOLTIP_INFO_TEXT_{currencyIconComponent.currencyType.ToString().ToUpper()}";
			UIHelper.SetLabelText(Label_ItemName, LocalizationManager.GetStringDirect(key));
			UIHelper.SetLabelText(Label_Desc, LocalizationManager.GetStringDirect(key2));
			UIHelper.EnableButton(Button_Detail, enable: false);
		}
		CurrentCompToggleSelect(isActive: false);
		_currcomp = comp;
		CurrentCompToggleSelect(isActive: true);
	}

	private void CurrentCompToggleSelect(bool isActive)
	{
		if (_currcomp == null)
		{
			return;
		}
		if (isActive)
		{
			if (_currcomp is ItemIconComponent)
			{
				(_currcomp as ItemIconComponent).SetSelected(selected: true, SelectSubComponent.ESelectType.Select_BorderBook);
			}
			else if (_currcomp is CurrencyIconComponent)
			{
				(_currcomp as CurrencyIconComponent).SetSelected(selected: true, SelectSubComponent.ESelectType.Select_BorderBook);
			}
		}
		else if (_currcomp is ItemIconComponent)
		{
			(_currcomp as ItemIconComponent).SetSelected(selected: false);
		}
		else if (_currcomp is CurrencyIconComponent)
		{
			(_currcomp as CurrencyIconComponent).SetSelected(selected: false);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (itemIconComponent.ItemData != null)
		{
			SelectComponent(itemIconComponent);
		}
	}

	void CurrencyIconComponent.Listener.OnClick(CurrencyIconComponent currencyIconComponent)
	{
		SelectComponent(currencyIconComponent);
	}
}
