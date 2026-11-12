using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUtil.NStringProxy;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class WeaponUniqueSelect : BaseWindow, WeaponUniqueItemComponent.Listener
{
	public interface Listener
	{
		void OnSelectWeaponUnique(int useItemIndex, int buyItemIndex, Action onSelect);
	}

	public const string PrefabPath = "Prefab2/Window/Inventory/WeaponUniqueSelect";

	public Shop.ItemDescription ItemDesc;

	public UIButton Button_Buy;

	public UIScrollGrid ItemGrid;

	public UILabel Label_Title;

	protected ItemIconComponent _selectedItemIcon;

	protected StatListComponent _statListComponent;

	protected List<WeaponUniqueItemComponent> _shopItemComponent = new List<WeaponUniqueItemComponent>();

	private List<ClassTagIconComponent> _classTagIconComponentList = new List<ClassTagIconComponent>();

	private Listener _listener;

	protected ItemData _buyItemData;

	protected ItemData _useItemData;

	[SerializeField]
	protected SelectFilter SelectFilter;

	[SerializeField]
	private UILabel LbDescriptionNoneEquip;

	protected SelectFilter.SelectType SelectType;

	public void Open(ItemData itemData, Listener listener)
	{
		Open();
		_listener = listener;
		Init(itemData);
		if (SelectFilter != null)
		{
			WeaponUniqueSelectItemData selectItemData = GetSelectItemData(itemData);
			int[] array = ((selectItemData != null && __Extension_Method_ICollection__.COUNT(selectItemData.ItemIndices) > 0) ? selectItemData.ItemIndices : null);
			if (array != null)
			{
				SelectType = SelectFilter.GetEquipItemSelectType(array);
				SelectFilter.InitFilter(SelectType, AddEquipItemByFilter);
			}
		}
		else
		{
			UIHelper.EnableGameObject(SelectFilter, enable: false);
		}
	}

	protected virtual WeaponUniqueSelectItemData GetSelectItemData(ItemData itemData)
	{
		return itemData.GetSubData<WeaponUniqueSelectItemData>();
	}

	protected bool Init(ItemData itemData)
	{
		if (itemData == null)
		{
			return false;
		}
		WeaponUniqueSelectItemData selectItemData = GetSelectItemData(itemData);
		if (selectItemData == null || __Extension_Method_ICollection__.IsNullOrEmpty(selectItemData.ItemIndices))
		{
			return false;
		}
		_useItemData = itemData;
		_buyItemData = null;
		SetTitle(selectItemData);
		SetItemDescription(null);
		return true;
	}

	private bool CheckAddEquipItem(EquipItemData equipItemData)
	{
		if (!SprkTicketSearch.MatchEquip(this, equipItemData))
		{
			return false;
		}
		if (SelectFilter != null)
		{
			switch (SelectType)
			{
			case SelectFilter.SelectType.SELECT_EQUIP_UNIQUE:
			case SelectFilter.SelectType.SELECT_EQUIP_ARTIFACT:
				return SelectFilter.CheckAddItem(equipItemData);
			case SelectFilter.SelectType.SELECT_EQUIP_OPTION:
				return SelectFilter.CheckAddEquipItemByPartType(equipItemData);
			}
		}
		return true;
	}

	protected void AddEquipItemByFilter()
	{
		SprkArtifactSourceFilter.BeforeList(this);
		FactoryBase.ReleaseAndClearList(_shopItemComponent);
		if (_useItemData == null)
		{
			SprkArtifactSourceFilter.AfterList(this);
			return;
		}
		WeaponUniqueSelectItemData selectItemData = GetSelectItemData(_useItemData);
		if (selectItemData == null || __Extension_Method_ICollection__.IsNullOrEmpty(selectItemData.ItemIndices))
		{
			SprkArtifactSourceFilter.AfterList(this);
			return;
		}
		ItemData itemData = null;
		EquipItemData equipItemData = null;
		IEnumerator enumerator = selectItemData.ItemIndices.GetEnumerator();
		while (enumerator.MoveNext())
		{
			itemData = GameRoot.Instance.ItemDataContainer.GetData((int)enumerator.Current);
			if (itemData != null)
			{
				equipItemData = GameRoot.Instance.EquipItemDataContainer.GetData(itemData.Code);
				if (equipItemData != null && CheckAddEquipItem(equipItemData))
				{
					AddEquipItemComponent(itemData, selectItemData.Star, selectItemData.Level);
				}
			}
		}
		UIHelper.RepositionGrid(ItemGrid);
		SetNoneEquipItem();
		SprkArtifactSourceFilter.AfterList(this);
	}

	private void AddEquipItemComponent(ItemData itemData, int star, int level)
	{
		if (itemData != null)
		{
			WeaponUniqueItemComponent weaponUniqueItemComponent = UIHelper.AddGridItem(ItemGrid, WeaponUniqueItemComponent.PrefabAuto);
			weaponUniqueItemComponent.Open(itemData, star, level, this);
			_shopItemComponent.Add(weaponUniqueItemComponent);
		}
	}

	protected void SetNoneEquipItem()
	{
		if (LbDescriptionNoneEquip != null)
		{
			UIHelper.EnableGameObject(LbDescriptionNoneEquip, _shopItemComponent.Count <= 0);
		}
	}

	protected virtual void SetTitle(WeaponUniqueSelectItemData selectItemData)
	{
		string key = (String_Extension_Method.IsNullOrEmpty(selectItemData.TitleKey) ? "ITEM_NAME_2000" : selectItemData.TitleKey);
		UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect(key));
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_shopItemComponent);
		ResetItemDescription();
	}

	protected void ResetItemDescription()
	{
		FactoryBase.ReleaseComponent(_selectedItemIcon);
		FactoryBase.SafeReleaseComponent(ref _statListComponent);
		FactoryBase.ReleaseAndClearList(_classTagIconComponentList);
		_selectedItemIcon = null;
	}

	private void SetupUniqueWidget(int heroIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		UIHelper.SetLabelText(ItemDesc.Label_CreatureLimit, data.Name);
		AssetTexture heroIcon = UIHelper.GetHeroIcon2(heroIndex, 3);
		UIHelper.ResetPlaneAssetTextureAsync(this, ItemDesc.UniqueHeroTexture, heroIcon);
	}

	private void SetupClassTagIcon(EquipItemData itemData)
	{
		if (ItemDesc.ClassTagGrid == null || itemData == null)
		{
			return;
		}
		CreatureTagType[] tagType = itemData.TagType;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(tagType))
		{
			return;
		}
		if (GameRoot.Instance.EquipItemDataContainer.ContainsAllClassTag(itemData.EquipCode))
		{
			ClassTagIconComponent classTagIconComponent = UIHelper.AddGridItem(ItemDesc.ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			_classTagIconComponentList.Add(classTagIconComponent);
			classTagIconComponent.OpenAllClassTag(null);
			UIHelper.RepositionGrid(ItemDesc.ClassTagGrid);
			return;
		}
		for (int num = tagType.Length - 1; num >= 0; num--)
		{
			CreatureTagType tagType2 = tagType[num];
			ClassTagIconComponent classTagIconComponent2 = UIHelper.AddGridItem(ItemDesc.ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			if (!(classTagIconComponent2 == null))
			{
				_classTagIconComponentList.Add(classTagIconComponent2);
				classTagIconComponent2.Open(tagType2, null);
			}
		}
		UIHelper.RepositionGrid(ItemDesc.ClassTagGrid);
	}

	protected void SetupUniqueAndClass(EquipItemData itemData)
	{
		if (itemData != null)
		{
			bool flag = !__Extension_Method_ICollection__.IsNullOrEmpty(itemData.CreatureIndex);
			if (flag)
			{
				SetupUniqueWidget(itemData.CreatureIndex[0]);
			}
			else
			{
				SetupClassTagIcon(itemData);
			}
			UIHelper.EnableGameObject(ItemDesc.ClassTagGrid, !flag);
			UIHelper.EnableGameObject(ItemDesc.UniqueWidget, flag);
		}
	}

	protected virtual void SetupEquipItem(ItemData itemData)
	{
		UIHelper.EnableGameObject(ItemDesc.WidgetEquipSubInfo, enable: true);
		UIHelper.EnableGameObject(ItemDesc.Label_ItemType, enable: false);
		UIHelper.EnableGameObject(ItemDesc.StatRootWidget, enable: true);
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemData.Code);
		if (data == null)
		{
			return;
		}
		WeaponUniqueSelectItemData selectItemData = GetSelectItemData(_useItemData);
		if (selectItemData != null)
		{
			SetupUniqueAndClass(data.SubData);
			UIHelper.SetLabelText(ItemDesc.Label_EquipLevel, (data.SubData.ReqLevel <= 0) ? 1 : data.SubData.ReqLevel);
			StatData statData = new StatData();
			statData.AddEquipItemStat(data.SubData, selectItemData.Star, data.SubData.DetailIndex, selectItemData.Level);
			_statListComponent = StatListComponent.Create(ItemDesc.StatRootWidget, 5);
			_statListComponent.AddStat(statData);
			int num = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex)?.OptionCount ?? 0;
			if (num > 0)
			{
				_statListComponent.AddOptionSubject(LocalizationManager.GetString(EquipEnchant.RANDOM_OPTION));
			}
			for (int i = 1; i <= num; i++)
			{
				GameRoot.Instance.EquipItemLevelDataContainer.GetEquipItemOptionLevel(i);
				string typeName = LocalizationManager.GetString(Equip.EQUIP_OPTION_STAT_TEXT);
				_statListComponent.AddCustomStat(typeName, null, _statListComponent.OptionClosedStatValueColor);
			}
			StatListComponent.AddFixedOptionStat(_statListComponent, data.GetSubData<EquipItemData>(), selectItemData.Star);
		}
	}

	protected virtual void SetItemDescription(ItemData itemData)
	{
		ResetItemDescription();
		if (itemData == null)
		{
			ItemDesc.SetEmpty();
			return;
		}
		ItemDesc.ActivateCommon(Active: true);
		UIHelper.SetLabelText(ItemDesc.Label_ItemName, ItemNameBuilder.GetItemName(itemData));
		_selectedItemIcon = UIHelper.AddGridItem(ItemDesc.ItemGrid, ItemIconComponent.PrefabAuto);
		_selectedItemIcon.Open(itemData);
		WeaponUniqueSelectItemData selectItemData = GetSelectItemData(_useItemData);
		if (selectItemData != null && selectItemData.Star > 0)
		{
			_selectedItemIcon.SetStar(selectItemData.Star);
		}
		UIHelper.EnableGameObject(ItemDesc.Label_Description, itemData.Type != ItemType.Equip);
		if (itemData.Type == ItemType.Equip)
		{
			SetupEquipItem(itemData);
		}
		UIHelper.EnableButton(Button_Buy, enable: true, grayscaleIfDisable: true);
	}

	public void OnClickBuy()
	{
		if (_listener != null && _buyItemData != null)
		{
			_listener.OnSelectWeaponUnique(_useItemData.Index, _buyItemData.Index, Close);
		}
	}

	public void OnClickExitButton()
	{
		Close();
	}

	void WeaponUniqueItemComponent.Listener.OnClick(ItemData itemData)
	{
		OnSelect(itemData);
	}

	public virtual void OnSelect(ItemData itemData)
	{
		_buyItemData = itemData;
		SetItemDescription(itemData);
		foreach (WeaponUniqueItemComponent item in _shopItemComponent)
		{
			item.SetSelected(itemData.Index);
		}
	}
}
