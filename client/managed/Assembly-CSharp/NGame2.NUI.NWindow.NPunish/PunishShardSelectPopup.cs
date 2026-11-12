using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPunish;

public class PunishShardSelectPopup : BaseWindow, CountInputField2.IListener, ItemIconComponent.Listener
{
	public interface IListenenr
	{
		void ApplySelectItem(int itemIndex, long itemCount);
	}

	[Serializable]
	public class ShardUi
	{
		public UIWidget Widget_ItemList;

		public UIScrollGrid ItemListGrid;

		public UIScrollGrid SelectItemDescGrid;

		public UILabel Label_Desc;
	}

	public static string PrefabPath = "Resources/Prefab2/Window/Raid/ShardSelectPopup";

	public UILabel Label_Title;

	public Transform Transform_SelectItem;

	public UILabel Label_SelectItemName;

	public CountInputField2 CountInputField;

	public UIButton Button_Unselect;

	public ShardUi ShardUi_UseCountInputField;

	public ShardUi ShardUi_NoneCountInputField;

	public UIButton Button_Confirm;

	private PunishmentRaidPartySetting.IDataProvider _dataProvider;

	private IListenenr _listener;

	private PunishmentGroupData _punishmentGroupData;

	private List<ItemIconComponent> _itemList = new List<ItemIconComponent>();

	private ItemIconComponent _selectItemIcon;

	private ItemData _selectItemData;

	private ItemIconComponent _selectListItemIcon;

	private long _selectItemCount;

	public void Open(IListenenr listenenr, PunishmentGroupData punishmentGroupData, PunishmentRaidPartySetting.IDataProvider dataProvider)
	{
		if (listenenr != null && punishmentGroupData != null && dataProvider != null)
		{
			base.Open();
			_listener = listenenr;
			_dataProvider = dataProvider;
			_punishmentGroupData = punishmentGroupData;
			Init();
		}
	}

	private void Init()
	{
		SetTitle();
		SetDefaultSelectItemInfo();
		InitShardItemList();
		SetSelectItemInfo();
	}

	private void SetTitle()
	{
		string text = ((_punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Karma) ? LocalizationManager.GetStringDirect("POPUP_TITLE_SHARD_FLASK_SELECT") : LocalizationManager.GetStringDirect("POPUP_TITLE_SHARD_ITEM_SELECT"));
		UIHelper.SetLabelText(Label_Title, text);
	}

	private void SetDefaultSelectItemInfo()
	{
		int num = _dataProvider.GetShardItemIndex();
		int shardItemCount = _dataProvider.GetShardItemCount();
		if (_punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Boss && num == 0)
		{
			IList<ShardItemData> shardDataList = GameRoot.Instance.ShardItemDataContainer.GetShardDataList(_punishmentGroupData.GroupIndex);
			if (__Extension_Method_ICollection__.IsNullOrEmpty(shardDataList))
			{
				return;
			}
			num = shardDataList[0].Index;
		}
		_selectItemData = ((num == 0) ? null : GameRoot.Instance.ItemDataContainer.GetData(num));
		int num2 = shardItemCount;
		_selectItemCount = num2;
	}

	private void InitShardItemList()
	{
		ClearItemList();
		UIHelper.EnableGameObject(ShardUi_NoneCountInputField.Widget_ItemList, _punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Karma);
		UIHelper.EnableGameObject(ShardUi_UseCountInputField.Widget_ItemList, _punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Boss);
		switch (_punishmentGroupData.PunishmentDungeonType)
		{
		case EPunishmentDungeonType.Karma:
		{
			IList<ShardFlaskItemData> shardFlaskDataList = GameRoot.Instance.ShardFlaskItemDataContainer.GetShardFlaskDataList(_punishmentGroupData.GroupIndex);
			if (shardFlaskDataList == null)
			{
				break;
			}
			foreach (ShardFlaskItemData item in shardFlaskDataList)
			{
				if (item == null)
				{
					continue;
				}
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(item.Index);
				if (data != null)
				{
					ItemIconComponent itemIconComponent2 = UIHelper.AddGridItem(ShardUi_NoneCountInputField.ItemListGrid, ItemIconComponent.PrefabAuto);
					itemIconComponent2.Open(data, this);
					int count2 = 0;
					ItemInfo itemInfo2 = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(item.Index);
					if (itemInfo2 != null)
					{
						count2 = itemInfo2.Count;
					}
					if (!item.IsDefault)
					{
						itemIconComponent2.SetItemCount_anycount(count2);
					}
					else
					{
						Color? color = null;
						int num = 0;
						itemIconComponent2.SetItemCount(num, color);
					}
					if (_selectItemData != null && _selectItemData.Index == item.Index)
					{
						_selectListItemIcon = itemIconComponent2;
						itemIconComponent2.SetSelected(selected: true);
					}
					_itemList.Add(itemIconComponent2);
				}
			}
			UIHelper.RepositionGrid(ShardUi_NoneCountInputField.ItemListGrid);
			break;
		}
		case EPunishmentDungeonType.Boss:
		{
			IList<ShardItemData> shardDataList = GameRoot.Instance.ShardItemDataContainer.GetShardDataList(_punishmentGroupData.GroupIndex);
			if (shardDataList == null)
			{
				break;
			}
			foreach (ShardItemData item2 in shardDataList)
			{
				if (item2 == null)
				{
					continue;
				}
				ItemData itemData = null;
				itemData = GameRoot.Instance.ItemDataContainer.GetData(item2.Index);
				if (itemData != null)
				{
					ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ShardUi_UseCountInputField.ItemListGrid, ItemIconComponent.PrefabAuto);
					itemIconComponent.Open(itemData, this);
					int count = 0;
					ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(item2.Index);
					if (itemInfo != null)
					{
						count = itemInfo.Count;
					}
					itemIconComponent.SetItemCount_anycount(count);
					if (_selectItemData != null && _selectItemData.Index == item2.Index)
					{
						_selectListItemIcon = itemIconComponent;
						itemIconComponent.SetSelected(selected: true);
					}
					_itemList.Add(itemIconComponent);
				}
			}
			UIHelper.RepositionGrid(ShardUi_UseCountInputField.ItemListGrid);
			break;
		}
		}
	}

	private void SetSelectItemInfo()
	{
		InitSelectItemIcon();
		InitCountInputField();
		InitItemDsec();
		SetUnselectButton();
		SetConfirmButton();
	}

	private void InitSelectItemIcon()
	{
		ClearSelectItemIcon();
		if (_selectItemData != null)
		{
			_selectItemIcon = ItemIconComponent.PrefabAuto.Factory.Create(Transform_SelectItem);
			_selectItemIcon.Open(_selectItemData);
			_selectItemIcon.ResetPosition();
			UIHelper.SetLabelText(Label_SelectItemName, _selectItemData.Name);
		}
	}

	private void InitCountInputField()
	{
		if (CountInputField == null)
		{
			return;
		}
		if (_punishmentGroupData.PunishmentDungeonType != EPunishmentDungeonType.Boss)
		{
			UIHelper.EnableGameObject(CountInputField, enable: false);
			return;
		}
		UIHelper.EnableGameObject(CountInputField, enable: true);
		int num = 0;
		long num2 = num;
		if (_selectItemData != null)
		{
			ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(_selectItemData.Index);
			if (itemInfo != null)
			{
				int count = itemInfo.Count;
				num2 = count;
			}
		}
		CountInputField2 countInputField = CountInputField;
		CountInputField2.CountInputFieldInfo countInputFieldInfo = new CountInputField2.CountInputFieldInfo();
		long num3 = num2;
		int num4 = 0;
		countInputFieldInfo.MinCount = ((num3 > num4) ? 1 : 0);
		countInputFieldInfo.MaxCount = num2;
		countInputFieldInfo.InitCount = _selectItemCount;
		countInputField.Open(this, countInputFieldInfo);
	}

	private void InitItemDsec()
	{
		UIHelper.EnableGameObject(ShardUi_UseCountInputField.SelectItemDescGrid, _selectItemData != null && _punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Boss);
		UIHelper.EnableGameObject(ShardUi_NoneCountInputField.SelectItemDescGrid, _selectItemData != null && _punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Karma);
		if (_selectItemData != null)
		{
			switch (_punishmentGroupData.PunishmentDungeonType)
			{
			case EPunishmentDungeonType.Boss:
				UIHelper.SetLabelText(ShardUi_UseCountInputField.Label_Desc, _selectItemData.Description2);
				UIHelper.RepositionGrid(ShardUi_UseCountInputField.SelectItemDescGrid);
				break;
			case EPunishmentDungeonType.Karma:
				UIHelper.SetLabelText(ShardUi_NoneCountInputField.Label_Desc, _selectItemData.Description2);
				UIHelper.RepositionGrid(ShardUi_NoneCountInputField.SelectItemDescGrid);
				break;
			}
		}
	}

	private void SetUnselectButton()
	{
		UIHelper.EnableGameObject(Button_Unselect, _punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Boss);
		if (_punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Boss)
		{
			bool enable = !CheckUnselectEnable();
			UIHelper.EnableButton(Button_Unselect, enable);
		}
	}

	private void SetConfirmButton()
	{
		bool enable = CheckConfirmButtonEnable();
		UIHelper.EnableButton(Button_Confirm, enable, grayscaleIfDisable: true);
	}

	private bool CheckUnselectEnable()
	{
		int shardItemIndex = _dataProvider.GetShardItemIndex();
		int shardItemCount = _dataProvider.GetShardItemCount();
		if (shardItemIndex == 0)
		{
			return shardItemCount == 0;
		}
		return false;
	}

	private bool CheckConfirmButtonEnable()
	{
		if (_selectItemData == null)
		{
			return false;
		}
		PunishmentGroupData punishmentGroupData = _dataProvider.GetPunishmentGroupData();
		if (punishmentGroupData == null)
		{
			return false;
		}
		switch (punishmentGroupData.PunishmentDungeonType)
		{
		case EPunishmentDungeonType.Karma:
		{
			ShardFlaskItemData data = GameRoot.Instance.ShardFlaskItemDataContainer.GetData(_selectItemData.Index);
			if (data != null && data.IsDefault)
			{
				return true;
			}
			ItemInfo itemInfo2 = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(_selectItemData.Index);
			if (itemInfo2 != null && itemInfo2.Count > 0)
			{
				return true;
			}
			break;
		}
		case EPunishmentDungeonType.Boss:
		{
			ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(_selectItemData.Index);
			if (itemInfo == null || itemInfo.Count <= 0)
			{
				break;
			}
			long selectItemCount = _selectItemCount;
			int num = 0;
			if (selectItemCount > num)
			{
				int count = itemInfo.Count;
				long selectItemCount2 = _selectItemCount;
				int num2 = count;
				if (num2 >= selectItemCount2)
				{
					return true;
				}
			}
			break;
		}
		}
		return false;
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		_dataProvider = null;
		_punishmentGroupData = null;
		ClearSelectItemIcon();
		ClearItemList();
	}

	private void ClearSelectItemIcon()
	{
		if (!(_selectItemIcon == null))
		{
			FactoryBase.SafeReleaseComponent(ref _selectItemIcon);
			_selectItemIcon = null;
			UIHelper.SetLabelText(Label_SelectItemName, string.Empty);
		}
	}

	private void ClearItemList()
	{
		FactoryBase.ReleaseAndClearList(_itemList);
		_itemList.Clear();
	}

	public void OnClickUnSelect()
	{
		if (_listener != null)
		{
			_dataProvider.SetShardItemIndex(0);
			PunishmentRaidPartySetting.IDataProvider dataProvider = _dataProvider;
			int num = 0;
			dataProvider.SetShardItemCount(num);
			_listener.ApplySelectItem(_selectItemData.Index, _selectItemCount);
		}
	}

	public void OnClickConfirm()
	{
		if (_listener != null && _selectItemData != null)
		{
			_dataProvider.SetShardItemIndex(_selectItemData.Index);
			if (_punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Boss)
			{
				_dataProvider.SetShardItemCount(_selectItemCount);
			}
			_listener.ApplySelectItem(_selectItemData.Index, _selectItemCount);
		}
	}

	void CountInputField2.IListener.ChangeCount()
	{
		_selectItemCount = CountInputField.CurrentCount;
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (itemIconComponent.ItemData != null && (_selectItemData == null || _selectItemData.Index != itemIconComponent.ItemData.Index))
		{
			_selectItemData = itemIconComponent.ItemData;
			if (_selectListItemIcon != null)
			{
				_selectListItemIcon.SetSelected(selected: false);
			}
			_selectListItemIcon = itemIconComponent;
			_selectListItemIcon.SetSelected(selected: true);
			if (_punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Boss)
			{
				int num = 1;
				_selectItemCount = num;
			}
			else
			{
				_selectItemCount = itemIconComponent.ItemCount;
			}
			SetSelectItemInfo();
		}
	}
}
