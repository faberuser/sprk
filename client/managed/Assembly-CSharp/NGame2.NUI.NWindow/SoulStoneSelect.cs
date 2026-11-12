using System;
using System.Collections;
using NGame2.NUI.NComponent2;
using NGame2.NUtil.NStringProxy;
using NGameAsset;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class SoulStoneSelect : WeaponUniqueSelect
{
	public interface IListener
	{
		void OnSelectSoulStone(int useItemIndex, int selectItemIndex, Action onSelect);
	}

	public new const string PrefabPath = "Prefab2/Window/Inventory/SoulStoneSelect";

	private IListener _iListener;

	public void Open(ItemData itemData, IListener listener)
	{
		base.Open();
		_iListener = listener;
		Init(itemData);
		if (SelectFilter != null)
		{
			WeaponUniqueSelectItemData selectItemData = GetSelectItemData(itemData);
			if (((selectItemData != null && __Extension_Method_ICollection__.COUNT(selectItemData.ItemIndices) > 0) ? selectItemData.ItemIndices : null) != null)
			{
				SelectType = SelectFilter.SelectType.SELECT_SOULSTONE;
				SelectFilter.InitFilter(SelectType, AddSoulWeaponPackageByFilter);
			}
		}
		else
		{
			UIHelper.EnableGameObject(SelectFilter, enable: false);
		}
	}

	protected override WeaponUniqueSelectItemData GetSelectItemData(ItemData itemData)
	{
		return GameRoot.Instance.WeaponUniqueSelectItemDataContainer.GetData(itemData.Code);
	}

	protected override void SetItemDescription(ItemData itemData)
	{
		ResetItemDescription();
		if (itemData == null)
		{
			ItemDesc.SetEmpty();
			return;
		}
		ItemDesc.ActivateCommon(Active: true);
		_selectedItemIcon = UIHelper.AddGridItem(ItemDesc.ItemGrid, ItemIconComponent.PrefabAuto);
		_selectedItemIcon.Open(itemData);
		SetupItem(itemData);
		UIHelper.EnableButton(Button_Buy, enable: true, grayscaleIfDisable: true);
	}

	private void SetupItem(ItemData itemData)
	{
		UIHelper.EnableGameObject(ItemDesc.WidgetEquipSubInfo, enable: true);
		UIHelper.EnableGameObject(ItemDesc.Label_ItemType, enable: false);
		UIHelper.EnableGameObject(ItemDesc.StatRootWidget, enable: false);
		UIHelper.EnableGameObject(ItemDesc.Label_Description, enable: true);
		UIHelper.EnableGameObject(ItemDesc.Label_EquipLevel, enable: true);
		string itemName = ItemNameBuilder.GetItemName(itemData);
		UIHelper.SetLabelText(ItemDesc.Label_ItemName, itemName);
		UIHelper.SetLabelText(ItemDesc.Label_Description, itemData.Description2);
		UIHelper.SetLabelText(ItemDesc.Label_EquipLevel, 1);
		int valueInt = itemData.GetValueInt(0);
		SetupUniqueWidget(valueInt);
	}

	private void SetupUniqueWidget(int heroIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		UIHelper.SetLabelText(ItemDesc.Label_CreatureLimit, data.Name);
		AssetTexture heroIcon = UIHelper.GetHeroIcon2(heroIndex, 3);
		UIHelper.ResetPlaneAssetTextureAsync(this, ItemDesc.UniqueHeroTexture, heroIcon);
	}

	private void AddSoulWeaponPackageItemComponent(ItemData itemData, int star, int level)
	{
		if (itemData != null)
		{
			WeaponUniqueItemComponent weaponUniqueItemComponent = UIHelper.AddGridItem(ItemGrid, WeaponUniqueItemComponent.PrefabAuto);
			weaponUniqueItemComponent.Open(itemData, star, level, this);
			_shopItemComponent.Add(weaponUniqueItemComponent);
		}
	}

	private void AddSoulWeaponPackageByFilter()
	{
		FactoryBase.ReleaseAndClearList(_shopItemComponent);
		if (_useItemData == null)
		{
			return;
		}
		WeaponUniqueSelectItemData selectItemData = GetSelectItemData(_useItemData);
		if (selectItemData == null || __Extension_Method_ICollection__.IsNullOrEmpty(selectItemData.ItemIndices))
		{
			return;
		}
		ItemData itemData = null;
		IEnumerator enumerator = selectItemData.ItemIndices.GetEnumerator();
		while (enumerator.MoveNext())
		{
			itemData = GameRoot.Instance.ItemDataContainer.GetData((int)enumerator.Current);
			if (itemData != null && CheckAddSoulStoneItem(itemData))
			{
				AddSoulWeaponPackageItemComponent(itemData, selectItemData.Star, selectItemData.Level);
			}
		}
		UIHelper.RepositionGrid(ItemGrid);
		SetNoneEquipItem();
	}

	private bool CheckAddSoulStoneItem(ItemData itemData)
	{
		if (!SprkTicketSearch.MatchSoulStone(this, itemData))
		{
			return false;
		}
		if (SelectFilter == null)
		{
			return true;
		}
		return SelectFilter.CheckAddSoulStoneItem(itemData);
	}

	public new void OnClickBuy()
	{
		if (_iListener != null && _buyItemData != null)
		{
			_iListener.OnSelectSoulStone(_useItemData.Index, _buyItemData.Index, Close);
		}
	}
}
