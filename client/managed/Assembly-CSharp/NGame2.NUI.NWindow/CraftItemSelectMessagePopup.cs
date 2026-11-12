using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NManager;
using NGame2.NUtil.NStringProxy;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class CraftItemSelectMessagePopup : BaseWindow, CraftItemSelectComponent.Listener, CraftSortComponent.Listener
{
	public enum EToggle
	{
		None,
		Growth,
		Equip,
		Rune,
		ETC,
		SoulWeapon
	}

	[Serializable]
	public class Category
	{
		public EToggle Type;

		public UIToggle Toggle;

		public ItemType[] ItemTypes;
	}

	[Serializable]
	public class CraftUiData
	{
		public EToggle Toggle;

		public Category[] Categories;

		public short Tier;

		public EquipPartType PartType;

		public CraftItemData SelectedData;

		public UIScrollGrid SelectGrid;

		public UIPanel ClippingPanel;

		public UIPanel EquipPanel;

		public UIPanel OtherPanel;

		public Transform WidgetDropMenu;

		public UISimpleGrid TierHead;

		public UISimpleGrid TierGrid;

		public UISimpleGrid EquipPartHead;

		public UISimpleGrid EquipPartGrid;

		public UIPopupRoot TierRoot;

		public UIPopupRoot EquipPartRoot;
	}

	[Serializable]
	public class ItemDescUiData
	{
		public Transform WidgetItemDesc;

		public UILabel Label_ItemName;

		public UIButton Button_DescClose;

		public UISimpleGrid ItemGrid;

		public UILabel Label_MainStatType;

		public UILabel Label_MainStatValue;

		public UILabel Label_MainFullyEnhancedStat;

		public UIWidget WidgetEquipSubInfo;

		public UILabel Label_EquipLevel;

		public UIWidget WidgetUnique;

		public UITexture UniqueHeroTexture;

		public UILabel Label_CreatureLimit;

		public UISimpleGrid ClassTagGrid;

		public UIWidget WidgetStatRoot;
	}

	[Serializable]
	public class MaterialUiData
	{
		public Transform WidgetMaterial;

		public UILabel Label_CraftTime;

		public UILabel Label_CraftCost;

		public Transform WidgetCountable;

		public UIInput NumberInput;

		public UIButton Button_Plus;

		public UIButton Button_Minus;

		public Transform WidgetUncountable;

		public UIButton Button_Craft;

		public UIScrollGrid SelectGrid;

		public GameObject SelectRoot;

		public GameObject DeselectRoot;

		public UILabel Label_MaterialTitle;

		public UILabel Label_MaterialRequired;

		public void ShowSelectRoot(bool isShow)
		{
			UIHelper.EnableGameObject(SelectRoot, isShow);
			UIHelper.EnableGameObject(DeselectRoot, !isShow);
		}
	}

	public const string PrefabPath = "Prefab2/Window/MessagePopup/CraftItemSelectMessagePopup";

	public CraftUiData CraftUi = new CraftUiData();

	public ItemDescUiData ItemDescUi = new ItemDescUiData();

	public MaterialUiData MaterialUi = new MaterialUiData();

	private ItemIconComponent _selectedItemIcon;

	private StatListComponent _statListComponent;

	private List<ClassTagIconComponent> _classTagIconComponentList = new List<ClassTagIconComponent>();

	private List<CraftItemSelectComponent> _craftItemSelectComponentList = new List<CraftItemSelectComponent>();

	private List<CraftItemMaterialComponent> _materialItemComponentList = new List<CraftItemMaterialComponent>();

	private CraftSortComponent _tierHead;

	private List<CraftSortComponent> _tierList = new List<CraftSortComponent>();

	private CraftSortComponent _equipPartHead;

	private List<CraftSortComponent> _equipPartList = new List<CraftSortComponent>();

	private int _min;

	private long _max;

	private long _currentCount;

	private int _pricePerItem;

	private Action<int, int, int> _onCraft;

	public static void Open(Action<int, int, int> onCraft, ItemData selectedItemData = null, int selectedItemReqCount = 0)
	{
		WindowPrefab<CraftItemSelectMessagePopup> windowPrefab = new WindowPrefab<CraftItemSelectMessagePopup>("Prefab2/Window/MessagePopup/CraftItemSelectMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(onCraft, selectedItemData, selectedItemReqCount);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal(Action<int, int, int> onCraft, ItemData selectedItemData, int selectedItemReqCount)
	{
		base.Open();
		_onCraft = onCraft;
		ClearCraftItem();
		ClearMaterial();
		ClearItemDesc();
		int num = 1;
		_currentCount = num;
		_pricePerItem = 0;
		UIHelper.EnableGameObject(MaterialUi.WidgetMaterial, enable: true);
		UIHelper.EnableGameObject(ItemDescUi.WidgetItemDesc, enable: false);
		if (selectedItemData != null)
		{
			EToggle selectedItemToggleType = GetSelectedItemToggleType(selectedItemData);
			SelectCategory(selectedItemToggleType);
			ForceSelectCraftItem(selectedItemData);
			SetMaterialInfoTitle(selectedItemReqCount);
			SprkEssenceCraftTickets.Prefill(this, selectedItemReqCount);
		}
		else
		{
			SelectCategory(EToggle.Growth);
			SprkEssenceCraftTickets.Prefill(this, selectedItemReqCount);
		}
	}

	public override void Reset()
	{
		base.Reset();
		_onCraft = null;
		CraftUi.SelectedData = null;
		FactoryBase.SafeReleaseComponent(ref _tierHead);
		FactoryBase.SafeReleaseComponent(ref _equipPartHead);
		FactoryBase.ReleaseAndClearList(_tierList);
		FactoryBase.ReleaseAndClearList(_equipPartList);
		ClearCraftItem();
		ClearMaterial();
		ClearItemDesc();
		InitUI();
	}

	private void ClearCraftItem()
	{
		FactoryBase.ReleaseAndClearList(_craftItemSelectComponentList);
	}

	private void ClearMaterial()
	{
		FactoryBase.ReleaseAndClearList(_materialItemComponentList);
		int num = 1;
		SetNumber(num);
		MaterialUi.ShowSelectRoot(isShow: false);
		UIHelper.SetLabelText(MaterialUi.Label_CraftTime, string.Empty);
		UIHelper.SetLabelText(MaterialUi.Label_CraftCost, string.Empty);
		UIHelper.EnableButton(MaterialUi.Button_Craft, enable: false, grayscaleIfDisable: true);
	}

	private void ClearItemDesc()
	{
		FactoryBase.ReleaseComponent(_selectedItemIcon);
		FactoryBase.SafeReleaseComponent(ref _statListComponent);
		FactoryBase.ReleaseAndClearList(_classTagIconComponentList);
		_selectedItemIcon = null;
	}

	private void InitUI()
	{
		CraftSortData[] categoryData = GameRoot.Instance.CraftSortDataContainer.GetCategoryData(CraftSortCategory.Tier);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(categoryData))
		{
			return;
		}
		CraftSortData[] categoryData2 = GameRoot.Instance.CraftSortDataContainer.GetCategoryData(CraftSortCategory.EquipPartType);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(categoryData2))
		{
			_tierHead = UIHelper.AddGridItem(CraftUi.TierHead, CraftSortComponent.PrefabAuto);
			_tierHead.Open(this, categoryData[0], onHead: true);
			for (int i = 0; i < categoryData.Length; i++)
			{
				CraftSortComponent craftSortComponent = UIHelper.AddGridItem(CraftUi.TierGrid, CraftSortComponent.PrefabAuto);
				craftSortComponent.Open(this, categoryData[i]);
				_tierList.Add(craftSortComponent);
			}
			UIHelper.RepositionGrid(CraftUi.TierHead);
			UIHelper.RepositionGrid(CraftUi.TierGrid);
			_equipPartHead = UIHelper.AddGridItem(CraftUi.EquipPartHead, CraftSortComponent.PrefabAuto);
			_equipPartHead.Open(this, categoryData2[0], onHead: true);
			for (int j = 0; j < categoryData2.Length; j++)
			{
				CraftSortComponent craftSortComponent2 = UIHelper.AddGridItem(CraftUi.EquipPartGrid, CraftSortComponent.PrefabAuto);
				craftSortComponent2.Open(this, categoryData2[j]);
				_equipPartList.Add(craftSortComponent2);
			}
			UIHelper.RepositionGrid(CraftUi.EquipPartHead);
			UIHelper.RepositionGrid(CraftUi.EquipPartGrid);
		}
	}

	private void SelectCategory(EToggle toggle)
	{
		Category[] categories = CraftUi.Categories;
		foreach (Category category in categories)
		{
			if ((bool)category.Toggle)
			{
				category.Toggle.value = category.Type == toggle;
			}
		}
		Category category2 = GetCategory(toggle);
		IList<CraftItemData> allData = GameRoot.Instance.CraftItemDataContainer.GetAllData();
		if (allData == null)
		{
			return;
		}
		List<CraftItemData> list = new List<CraftItemData>();
		ItemType[] itemTypes = category2.ItemTypes;
		foreach (ItemType itemType in itemTypes)
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(allData); j++)
			{
				CraftItemData craftItemData = allData[j];
				if (craftItemData.ItemType != itemType || !craftItemData.IsOpen)
				{
					continue;
				}
				if (toggle == EToggle.Equip)
				{
					ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(craftItemData.ItemCode);
					if (dataByCode == null || dataByCode.Type != ItemType.Equip)
					{
						continue;
					}
					ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(dataByCode.Code);
					if (data == null || data.SubData == null || (CraftUi.Tier != 0 && data.SubData.Tier != CraftUi.Tier) || (CraftUi.PartType != EquipPartType.None && data.SubData.PartType != CraftUi.PartType))
					{
						continue;
					}
				}
				list.Add(craftItemData);
			}
		}
		list.Sort((CraftItemData l, CraftItemData r) => l.ShowIndex.CompareTo(r.ShowIndex));
		if (toggle == EToggle.Equip)
		{
			CraftUi.ClippingPanel.SetRect(CraftUi.EquipPanel.finalClipRegion.x, CraftUi.EquipPanel.finalClipRegion.y, CraftUi.EquipPanel.width, CraftUi.EquipPanel.height);
		}
		else
		{
			CraftUi.ClippingPanel.SetRect(CraftUi.OtherPanel.finalClipRegion.x, CraftUi.OtherPanel.finalClipRegion.y, CraftUi.OtherPanel.width, CraftUi.OtherPanel.height);
		}
		ClearCraftItem();
		foreach (CraftItemData item in list)
		{
			if (item != null)
			{
				CraftItemSelectComponent craftItemSelectComponent = UIHelper.AddGridItem(CraftUi.SelectGrid, CraftItemSelectComponent.PrefabAuto);
				craftItemSelectComponent.Open(this, item.CraftIndex, item.ItemCode);
				_craftItemSelectComponentList.Add(craftItemSelectComponent);
			}
		}
		UIHelper.RepositionGrid(CraftUi.SelectGrid);
		UIHelper.EnableGameObject(CraftUi.WidgetDropMenu, toggle == EToggle.Equip);
		UIHelper.EnableGameObject(MaterialUi.WidgetCountable, enable: false);
		UIHelper.EnableGameObject(MaterialUi.WidgetUncountable, enable: true);
		SetMaterialInfoTitle(0);
		CraftUi.Toggle = toggle;
	}

	private Category GetCategory(EToggle toggle)
	{
		Category[] categories = CraftUi.Categories;
		foreach (Category category in categories)
		{
			if (category.Type == toggle)
			{
				return category;
			}
		}
		return null;
	}

	private void SetMinMax()
	{
		CraftItemData selectedData = CraftUi.SelectedData;
		if (selectedData == null)
		{
			return;
		}
		_min = 1;
		int num = 1;
		_max = num;
		if (!selectedData.Countable)
		{
			return;
		}
		if (selectedData.ReqGold != 0)
		{
			_max = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold / selectedData.ReqGold;
			long max;
			if (_max == 0L)
			{
				int num2 = 1;
				max = num2;
			}
			else
			{
				max = _max;
			}
			_max = max;
		}
		foreach (CraftItemData.MaterialItemInfo materialItem in selectedData.GetMaterialItemList())
		{
			if (GameRoot.Instance.ItemDataContainer.GetDataByCode(materialItem.ItemCode) == null)
			{
				continue;
			}
			int num3 = SprkEssenceCraftTickets.Available(GlobalAutoBehaviour<AccountManager>.instance.ItemManager, materialItem.ItemCode, this);
			if (materialItem.ItemCount != 0)
			{
				int num4 = num3 / materialItem.ItemCount;
				long max2 = _max;
				int num5 = num4;
				if (num5 < max2)
				{
					int num6 = ((num4 == 0) ? 1 : num4);
					_max = num6;
				}
			}
		}
	}

	private void SetMaterial()
	{
		FactoryBase.ReleaseAndClearList(_materialItemComponentList);
		CraftItemData selectedData = CraftUi.SelectedData;
		if (selectedData == null)
		{
			return;
		}
		bool countable = selectedData.Countable;
		bool enable = true;
		int num = 0;
		foreach (CraftItemData.MaterialItemInfo materialItem in selectedData.GetMaterialItemList())
		{
			num++;
			CraftItemMaterialComponent craftItemMaterialComponent = UIHelper.AddGridItem(MaterialUi.SelectGrid, CraftItemMaterialComponent.PrefabAuto);
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(materialItem.ItemCode);
			if (dataByCode == null)
			{
				int curCount = 0;
				int num2 = 0;
				craftItemMaterialComponent.Open(null, num2, curCount);
				_materialItemComponentList.Add(craftItemMaterialComponent);
				continue;
			}
			int itemCount = materialItem.ItemCount;
			long currentCount = _currentCount;
			int num3 = itemCount;
			long num4 = checked(num3 * currentCount);
			int num5 = SprkEssenceCraftTickets.Available(GlobalAutoBehaviour<AccountManager>.instance.ItemManager, materialItem.ItemCode, this);
			craftItemMaterialComponent.Open(dataByCode, num4, num5);
			_materialItemComponentList.Add(craftItemMaterialComponent);
			long num6 = num4;
			int num7 = num5;
			if (num7 < num6)
			{
				enable = false;
			}
		}
		for (int i = 0; i < 8 - num; i++)
		{
			CraftItemMaterialComponent craftItemMaterialComponent2 = UIHelper.AddGridItem(MaterialUi.SelectGrid, CraftItemMaterialComponent.PrefabAuto);
			int curCount2 = 0;
			int num8 = 0;
			craftItemMaterialComponent2.Open(null, num8, curCount2);
			_materialItemComponentList.Add(craftItemMaterialComponent2);
		}
		int reqGold = selectedData.ReqGold;
		long currentCount2 = _currentCount;
		int num9 = reqGold;
		long num10 = checked(num9 * currentCount2);
		bool isEnoughGold = true;
		if (GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold < num10)
		{
			enable = false;
			isEnoughGold = false;
		}
		SetSinglePrice(selectedData.ReqGold, isEnoughGold);
		TimeSpan timeSpan = new TimeSpan(0, 0, selectedData.ReqTime);
		int num11 = (int)timeSpan.TotalHours;
		string text = "";
		text = ((num11 >= 100) ? $"{num11:D3}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}" : $"{num11:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}");
		UIHelper.RepositionGrid(MaterialUi.SelectGrid);
		UIHelper.SetLabelText(MaterialUi.Label_CraftTime, text);
		UIHelper.EnableButton(MaterialUi.Button_Craft, enable, grayscaleIfDisable: true);
		UIHelper.EnableGameObject(MaterialUi.WidgetMaterial, enable: true);
		UIHelper.EnableGameObject(ItemDescUi.WidgetItemDesc, enable: false);
		UIHelper.EnableGameObject(MaterialUi.WidgetCountable, countable);
		UIHelper.EnableGameObject(MaterialUi.WidgetUncountable, !countable);
	}

	public int GetNumber()
	{
		int result = 0;
		string value = MaterialUi.NumberInput.value;
		if (!string.IsNullOrEmpty(value))
		{
			int.TryParse(value, out result);
		}
		return result;
	}

	private void ValidateNumber()
	{
		int number = GetNumber();
		long number2 = number;
		SetNumber(number2);
	}

	private void SetNumber(long number)
	{
		int min = _min;
		long max = _max;
		int min2 = min;
		int num = checked(Mathf.Clamp((int)number, min2, (int)max));
		_currentCount = num;
		MaterialUi.NumberInput.value = _currentCount.ToString();
		SetMaterial();
	}

	public void SetSinglePrice(int price, bool isEnoughGold)
	{
		_pricePerItem = price;
		UpdatePrice(isEnoughGold);
	}

	private void UpdatePrice(bool isEnoughGold)
	{
		string text = Vespa_Long_Extension_Method.ToCommaString(checked(_pricePerItem * _currentCount));
		if (!isEnoughGold)
		{
			text = Vespa_String_Extension_Method.NguiColor(text, Color.red, isAbsolute: true);
		}
		UIHelper.SetLabelText(MaterialUi.Label_CraftCost, text);
	}

	private void SetItemDesc(CraftItemData craftItemData)
	{
		ClearItemDesc();
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(craftItemData.ItemCode);
		if (dataByCode != null && dataByCode.Type == ItemType.Equip)
		{
			_selectedItemIcon = UIHelper.AddGridItem(ItemDescUi.ItemGrid, ItemIconComponent.PrefabAuto);
			_selectedItemIcon.Open(dataByCode);
			SetupEquipItem(dataByCode);
			UIHelper.SetLabelText(ItemDescUi.Label_ItemName, ItemNameBuilder.GetItemName(dataByCode));
			UIHelper.EnableGameObject(MaterialUi.WidgetMaterial, enable: false);
			UIHelper.EnableGameObject(ItemDescUi.WidgetItemDesc, enable: true);
		}
	}

	private void SetupEquipItem(ItemData itemData)
	{
		UIHelper.EnableGameObject(ItemDescUi.WidgetEquipSubInfo, enable: true);
		UIHelper.EnableGameObject(ItemDescUi.WidgetStatRoot, enable: true);
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemData.Code);
		if (data != null)
		{
			long statValue = 0L;
			int num = ((data.SubData.ReqLevel <= 0) ? 1 : data.SubData.ReqLevel);
			data.SubData.GetMainStat(out var statType, out statValue, 0, data.SubData.DetailIndex);
			SetupUniqueAndClass(data.SubData);
			UIHelper.SetLabelText(ItemDescUi.Label_EquipLevel, num);
			UIHelper.SetLabelText(ItemDescUi.Label_MainStatType, statType);
			UIHelper.SetLabelText(ItemDescUi.Label_MainStatValue, statValue);
			_statListComponent = StatListComponent.Create(ItemDescUi.WidgetStatRoot, 5);
			int num2 = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex)?.OptionCount ?? 0;
			if (num2 > 0)
			{
				_statListComponent.AddOptionSubject(LocalizationManager.GetString(EquipEnchant.RANDOM_OPTION));
			}
			for (int i = 1; i <= num2; i++)
			{
				GameRoot.Instance.EquipItemLevelDataContainer.GetEquipItemOptionLevel(i);
				string typeName = LocalizationManager.GetString(Equip.EQUIP_OPTION_STAT_TEXT);
				_statListComponent.AddCustomStat(typeName, null, _statListComponent.OptionClosedStatValueColor);
			}
			StatListComponent.AddOptionStat(_statListComponent, data.GetSubData<EquipItemData>(), null);
		}
	}

	private void SetupClassTagIcon(EquipItemData equipItemData)
	{
		if (ItemDescUi.ClassTagGrid == null || equipItemData == null)
		{
			return;
		}
		CreatureTagType[] tagType = equipItemData.TagType;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(tagType))
		{
			return;
		}
		if (GameRoot.Instance.EquipItemDataContainer.ContainsAllClassTag(equipItemData.EquipCode))
		{
			ClassTagIconComponent classTagIconComponent = UIHelper.AddGridItem(ItemDescUi.ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			_classTagIconComponentList.Add(classTagIconComponent);
			classTagIconComponent.OpenAllClassTag(null);
			UIHelper.RepositionGrid(ItemDescUi.ClassTagGrid);
			return;
		}
		for (int num = tagType.Length - 1; num >= 0; num--)
		{
			CreatureTagType tagType2 = tagType[num];
			ClassTagIconComponent classTagIconComponent2 = UIHelper.AddGridItem(ItemDescUi.ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			if (!(classTagIconComponent2 == null))
			{
				_classTagIconComponentList.Add(classTagIconComponent2);
				classTagIconComponent2.Open(tagType2, null);
			}
		}
		UIHelper.RepositionGrid(ItemDescUi.ClassTagGrid);
	}

	private void SetupUniqueWidget(int heroIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data != null)
		{
			UIHelper.SetLabelText(ItemDescUi.Label_CreatureLimit, data.Name);
			AssetTexture heroIcon = UIHelper.GetHeroIcon2(heroIndex, 3);
			UIHelper.ResetPlaneAssetTextureAsync(this, ItemDescUi.UniqueHeroTexture, heroIcon);
		}
	}

	private void SetupUniqueAndClass(EquipItemData equipItemData)
	{
		if (equipItemData != null)
		{
			bool flag = !__Extension_Method_ICollection__.IsNullOrEmpty(equipItemData.CreatureIndex);
			if (flag)
			{
				SetupUniqueWidget(equipItemData.CreatureIndex[0]);
			}
			else
			{
				SetupClassTagIcon(equipItemData);
			}
			UIHelper.EnableGameObject(ItemDescUi.WidgetUnique, flag);
			UIHelper.EnableGameObject(ItemDescUi.ClassTagGrid, !flag);
		}
	}

	public void OnClickCraft()
	{
		if (SprkEssenceCraftTickets.TryCraft(this, _onCraft))
		{
			return;
		}
		if (_onCraft != null && CraftUi.SelectedData != null)
		{
			Action<int, int, int> onCraft = _onCraft;
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(CraftUi.SelectedData.ItemCode);
			if (dataByCode == null)
			{
				return;
			}
			int craftIndex = CraftUi.SelectedData.CraftIndex;
			int index = dataByCode.Index;
			long currentCount = _currentCount;
			Vespa_System_Action_Extension_Method.fire(onCraft, craftIndex, index, checked((int)currentCount));
		}
		Reset();
		Close();
	}

	public void OnClickPlus()
	{
		int number = GetNumber();
		int num = 1;
		int num2 = number;
		checked
		{
			SetNumber(unchecked((long)num2) + unchecked((long)num));
		}
	}

	public void OnClickMinus()
	{
		int number = GetNumber();
		int num = 1;
		int num2 = number;
		checked
		{
			SetNumber(unchecked((long)num2) - unchecked((long)num));
		}
	}

	public void OnClickMax()
	{
		SetNumber(_max);
	}

	public void OnNumberChanged()
	{
		if (String_Extension_Method.IsNullOrEmpty(MaterialUi.NumberInput.value))
		{
			int num = 1;
			_currentCount = num;
			SetMaterial();
		}
		else
		{
			ValidateNumber();
		}
	}

	public void OnNumberSubmit()
	{
		ValidateNumber();
	}

	public void OnToggleChanged()
	{
		EToggle eToggle = EToggle.None;
		Category[] categories = CraftUi.Categories;
		foreach (Category category in categories)
		{
			if (category.Toggle.value)
			{
				eToggle = category.Type;
				break;
			}
		}
		if (CraftUi.Toggle != eToggle && eToggle != EToggle.None)
		{
			ClearMaterial();
			ClearCraftItem();
			SelectCategory(eToggle);
			UIHelper.EnableGameObject(MaterialUi.WidgetMaterial, enable: true);
			UIHelper.EnableGameObject(ItemDescUi.WidgetItemDesc, enable: false);
		}
	}

	public void OnClickCloseButton()
	{
		Reset();
		Close();
	}

	public override void OnSystemBackButton()
	{
		OnClickCloseButton();
	}

	void CraftItemSelectComponent.Listener.OnClick(int craftIndex)
	{
		CraftItemData data = GameRoot.Instance.CraftItemDataContainer.GetData(craftIndex);
		if (data == null)
		{
			return;
		}
		MaterialUi.ShowSelectRoot(isShow: true);
		CraftUi.SelectedData = data;
		foreach (CraftItemSelectComponent craftItemSelectComponent in _craftItemSelectComponentList)
		{
			craftItemSelectComponent.SetSelected(data.CraftIndex);
		}
		SetMinMax();
		int num = 1;
		SetNumber(num);
		SetMaterialInfoTitle(0);
	}

	void CraftItemSelectComponent.Listener.OnItemDescClick(int craftIndex)
	{
		CraftItemData data = GameRoot.Instance.CraftItemDataContainer.GetData(craftIndex);
		if (data != null)
		{
			SetItemDesc(data);
		}
	}

	public void OnClickCloseItemDesc()
	{
		UIHelper.EnableGameObject(MaterialUi.WidgetMaterial, enable: true);
		UIHelper.EnableGameObject(ItemDescUi.WidgetItemDesc, enable: false);
	}

	void CraftSortComponent.Listener.OnClick(CraftSortData optionData, bool onHead)
	{
		if (onHead)
		{
			switch (optionData.Category)
			{
			case CraftSortCategory.Tier:
				CraftUi.TierRoot.TogglePopup();
				break;
			case CraftSortCategory.EquipPartType:
				CraftUi.EquipPartRoot.TogglePopup();
				break;
			}
			return;
		}
		switch (optionData.Category)
		{
		case CraftSortCategory.Tier:
			_tierHead.Open(this, optionData, onHead: true);
			CraftUi.Tier = (short)optionData.Value;
			break;
		case CraftSortCategory.EquipPartType:
			_equipPartHead.Open(this, optionData, onHead: true);
			CraftUi.PartType = (EquipPartType)optionData.BackGroundIndex;
			break;
		}
		ClearCraftItem();
		SelectCategory(EToggle.Equip);
	}

	private EToggle GetSelectedItemToggleType(ItemData selectedItemData)
	{
		EToggle eToggle = EToggle.ETC;
		ItemType craftCategoryTypeUsingItemCode = GetCraftCategoryTypeUsingItemCode(selectedItemData);
		if (craftCategoryTypeUsingItemCode == ItemType.None)
		{
			return eToggle;
		}
		return GetToggleType(eToggle, craftCategoryTypeUsingItemCode);
	}

	private static ItemType GetCraftCategoryTypeUsingItemCode(ItemData selectedItemData)
	{
		ItemType result = ItemType.None;
		foreach (CraftItemData allDatum in GameRoot.Instance.CraftItemDataContainer.GetAllData())
		{
			if (allDatum.ItemCode == selectedItemData.Code)
			{
				result = allDatum.ItemType;
			}
		}
		return result;
	}

	private EToggle GetToggleType(EToggle toggleType, ItemType craftCategoryType)
	{
		Category[] categories = CraftUi.Categories;
		foreach (Category category in categories)
		{
			if (Array.Exists(category.ItemTypes, (ItemType t) => t == craftCategoryType))
			{
				toggleType = category.Type;
				break;
			}
		}
		return toggleType;
	}

	private void ForceSelectCraftItem(ItemData selectedItemData)
	{
		CraftItemSelectComponent craftItemSelectComponent = _craftItemSelectComponentList.Find((CraftItemSelectComponent item) => item._itemData == selectedItemData);
		if (craftItemSelectComponent != null)
		{
			craftItemSelectComponent.OnClick();
			CraftUi.SelectGrid.ScrollTo(craftItemSelectComponent, 0f, 0f);
		}
	}

	private void SetMaterialInfoTitle(int reqCount)
	{
		UIHelper.EnableGameObject(MaterialUi.Label_MaterialRequired, reqCount > 0);
		UIHelper.EnableGameObject(MaterialUi.Label_MaterialTitle, reqCount <= 0);
		if (reqCount > 0)
		{
			string stringDirect = LocalizationManager.GetStringDirect("CRAFT_ITEMCOMPONENT_NECESSARY", reqCount);
			UIHelper.SetLabelText(MaterialUi.Label_MaterialRequired, stringDirect);
		}
	}
}
