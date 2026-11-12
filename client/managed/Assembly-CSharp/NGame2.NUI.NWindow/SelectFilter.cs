using System.Collections.Generic;
using NGame2.NAccount;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SelectFilter : BaseWindow
{
	public enum SelectType
	{
		SELECT_AVATAR,
		SELECT_HERO,
		SELECT_COSTUME,
		SELECT_EQUIP_UNIQUE,
		SELECT_EQUIP_ARTIFACT,
		SELECT_EQUIP_OPTION,
		SELECT_REGION,
		SELECT_SOULSTONE
	}

	public enum ComboBoxType
	{
		ALL = 0,
		COSTUME = 2,
		CREATURE = 3,
		OWNED = 5,
		NOT_OWNED = 4,
		PART_TYPE = 6
	}

	public delegate void Callback();

	[SerializeField]
	protected UIComboBox ComboBoxMain;

	[SerializeField]
	protected UIComboBoxSecondary ComboBoxSecondaryMain;

	[SerializeField]
	protected UIComboBox ComboBoxSub;

	private SelectType _selectType;

	private Callback _callback;

	private List<EquipPartType> _listSpecificEquipPartType;

	private bool _useSubFilter = true;

	private bool _useSecondaryMainFilter;

	public ItemData<CostumeSelectItemData> _itemData { get; private set; }

	public ComboBoxType MainComboBoxCurrentKey => (ComboBoxType)ComboBoxMain.CurrentKey;

	public int SecondaryMainComboBoxCurrentKey => ComboBoxSecondaryMain.CurrentKey;

	public int SubComboBoxCurrentKey => ComboBoxSub.CurrentKey;

	public void InitFilter(SelectType type, Callback callback, ItemData<CostumeSelectItemData> itemData = null, bool useSubFilter = true, bool useSecondaryMainFilter = false)
	{
		SprkTicketSearch.BeforeFilter(this);
		SprkArtifactSourceFilter.Reset(this);
		_selectType = type;
		_itemData = itemData;
		_callback = callback;
		_useSubFilter = useSubFilter;
		_useSecondaryMainFilter = useSecondaryMainFilter;
		InitMainComboBox();
		SprkTicketSearch.AfterFilter(this);
	}

	private void OnDisable()
	{
		if (ComboBoxSecondaryMain != null)
		{
			ComboBoxSecondaryMain.Close();
		}
	}

	private void InitMainComboBox()
	{
		ComboBoxMain.Clear();
		ComboBoxMain.AddItem(0, GetComboBoxAllTypeString());
		switch (_selectType)
		{
		case SelectType.SELECT_AVATAR:
			SetAvatarMainComboBox();
			break;
		case SelectType.SELECT_HERO:
			SetHeroMainComboBox();
			break;
		case SelectType.SELECT_COSTUME:
			SetCostumeMainComboBox();
			break;
		case SelectType.SELECT_EQUIP_UNIQUE:
		case SelectType.SELECT_EQUIP_ARTIFACT:
			SetEquipMainComboBox();
			break;
		case SelectType.SELECT_EQUIP_OPTION:
			SetEquipOptionMainComboBox();
			break;
		case SelectType.SELECT_REGION:
			SetRegionMainComboBox();
			break;
		case SelectType.SELECT_SOULSTONE:
			SetSoulStoneSelectMainComboBox();
			break;
		}
		ComboBoxMain.Reposition();
		ComboBoxMain.SelectFirstItem();
		SetSubComboBox();
		SetComboBoxCallback();
		if (ComboBoxSecondaryMain != null)
		{
			InitSecondaryMainComboBox();
		}
	}

	private void SetAvatarMainComboBox()
	{
		ComboBoxMain.AddItem(3, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_CLASS));
	}

	private void SetCostumeMainComboBox()
	{
		if (_itemData != null)
		{
			ComboBoxMain.AddItem(2, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_COSTUME));
			ComboBoxMain.AddItem(3, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_CLASS));
		}
		else
		{
			UIHelper.EnableGameObject(base.gameObject, enable: false);
		}
	}

	private void InitSecondaryMainComboBox()
	{
		ComboBoxSecondaryMain.Clear();
		if (_selectType == SelectType.SELECT_COSTUME)
		{
			SetCostumeSecondaryMainComboBox();
		}
	}

	private void SetCostumeSecondaryMainComboBox()
	{
		ComboBoxSecondaryMain.primaryComboBoxItemLabel = LocalizationManager.GetString(FilterSelect.FILTER_SELECT_COSTUME);
		ComboBoxSecondaryMain.primaryComboBoxItemKey = 2;
		IEnumerable<string> costumeSelectItemGroupNames = GetCostumeSelectItemGroupNames();
		if (costumeSelectItemGroupNames == null)
		{
			return;
		}
		foreach (string item in costumeSelectItemGroupNames)
		{
			string stringDirect = LocalizationManager.GetStringDirect("COSTUME_GROUP_SELECTITEMGROUP_" + item.ToUpper());
			int selectItemGroupKey = GameRoot.Instance.CostumeGroupDataContainer.GetSelectItemGroupKey(item);
			ComboBoxSecondaryMain.AddItem(selectItemGroupKey, stringDirect);
		}
	}

	private IEnumerable<string> GetCostumeSelectItemGroupNames()
	{
		if (_itemData != null)
		{
			if (_itemData.SubData.CostumeCategories != null)
			{
				return GameRoot.Instance.CostumeGroupDataContainer.GetSelectItemGroupList(_itemData.SubData.CostumeCategories);
			}
			if (_itemData.SubData.CostumeIndices != null)
			{
				return GameRoot.Instance.CostumeGroupDataContainer.GetSelectItemGroupList(_itemData.SubData.CostumeIndices);
			}
		}
		return null;
	}

	private void SetHeroMainComboBox()
	{
		ComboBoxMain.AddItem(5, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_OWNED));
		ComboBoxMain.AddItem(4, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_NOT_OWNED));
	}

	private void SetEquipMainComboBox()
	{
		ComboBoxMain.AddItem(5, LocalizationManager.GetString((_selectType == SelectType.SELECT_EQUIP_UNIQUE) ? FilterSelect.FILTER_SELECT_OWNED_HERO : FilterSelect.FILTER_SELECT_OWNED_EQUIP));
		ComboBoxMain.AddItem(4, LocalizationManager.GetString((_selectType == SelectType.SELECT_EQUIP_UNIQUE) ? FilterSelect.FILTER_SELECT_NOT_OWNED_HERO : FilterSelect.FILTER_SELECT_NOT_OWNED_EQUIP));
	}

	private void SetEquipOptionMainComboBox()
	{
		ComboBoxMain.AddItem(3, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_CLASS));
		ComboBoxMain.AddItem(6, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_PART));
	}

	private void SetRegionMainComboBox()
	{
		CountryCodeGroupDataContainer countryCodeGroupDataContainer = GameRoot.Instance.CountryCodeGroupDataContainer;
		if (countryCodeGroupDataContainer == null)
		{
			return;
		}
		SortedDictionary<int, string> sortedDictionary = new SortedDictionary<int, string>();
		sortedDictionary.Clear();
		foreach (CountryCodeGroupData allDatum in countryCodeGroupDataContainer.GetAllData())
		{
			if (!sortedDictionary.ContainsKey(allDatum.Index))
			{
				string stringDirect = LocalizationManager.GetStringDirect(allDatum.LocalTextPath);
				sortedDictionary.Add(allDatum.Index, stringDirect);
			}
		}
		foreach (KeyValuePair<int, string> item in sortedDictionary)
		{
			ComboBoxMain.AddItem(item.Key, item.Value);
		}
	}

	private void SetSoulStoneSelectMainComboBox()
	{
		ComboBoxMain.AddItem(5, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_OWNED));
		ComboBoxMain.AddItem(4, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_NOT_OWNED));
	}

	private void SetComboBoxCallback()
	{
		ComboBoxMain.OnChanged = OnClickedMain;
		if (_useSecondaryMainFilter && ComboBoxSecondaryMain != null)
		{
			ComboBoxSecondaryMain.OnChanged = OnClickedSecondaryMain;
		}
		if (_useSubFilter && ComboBoxSub != null)
		{
			ComboBoxSub.OnChanged = OnClickedSub;
		}
	}

	private string GetComboBoxAllTypeString()
	{
		string text = "";
		if (_selectType == SelectType.SELECT_REGION)
		{
			return LocalizationManager.GetStringDirect("COUNTRY_FLAG_GROUP_ALL");
		}
		return LocalizationManager.GetString(FilterSelect.FILTER_SELECT_ALL);
	}

	private void SetSubComboBox()
	{
		if (SprkArtifactSourceFilter.Build(this, ComboBoxSub, (int)_selectType, _callback))
		{
			return;
		}
		if (!_useSubFilter || ComboBoxSub == null)
		{
			_callback();
			return;
		}
		ComboBoxSub.Clear();
		ComboBoxSub.AddItem(0, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_ALL));
		switch ((ComboBoxType)ComboBoxMain.CurrentKey)
		{
		case ComboBoxType.ALL:
			SetAllSubComboBox();
			break;
		case ComboBoxType.COSTUME:
			SetCostumeSubComboBox();
			break;
		case ComboBoxType.CREATURE:
		case ComboBoxType.NOT_OWNED:
		case ComboBoxType.OWNED:
			SetSubComboBoxCreature();
			break;
		case ComboBoxType.PART_TYPE:
			SetSubComboxPartType(_listSpecificEquipPartType);
			break;
		}
		ComboBoxSub.Reposition();
		ComboBoxSub.SelectFirstItem();
		_callback();
	}

	private void SetAllSubComboBox()
	{
		if (_selectType == SelectType.SELECT_EQUIP_ARTIFACT || _selectType == SelectType.SELECT_REGION)
		{
			return;
		}
		foreach (CreatureTagType item in FilterHelper.GetListOrderCreatureTagType())
		{
			ComboBoxSub.AddItem((int)item, LocalizationManager.GetString(item));
		}
	}

	private void SetCostumeSubComboBox()
	{
		if (_itemData != null && ComboBoxSecondaryMain.GetItemCount() > 0)
		{
			int currentKey = ComboBoxSecondaryMain.CurrentKey;
			if (_itemData.SubData.CostumeCategories != null)
			{
				SetSubComboBoxItemCostumeCategories(currentKey);
			}
			else if (_itemData.SubData.CostumeIndices != null)
			{
				SetSubComboBoxItemCostumeIndices(currentKey);
			}
		}
		if (ComboBoxSub.GetItemCount() <= 0)
		{
			ComboBoxSub.AddItem(0, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_ALL));
		}
		ComboBoxSub.SelectFirstItem();
	}

	private void SetSubComboBoxItemCostumeCategories(int selectItemGroup)
	{
		if (_itemData == null)
		{
			return;
		}
		string[] costumeCategories = _itemData.SubData.CostumeCategories;
		foreach (string text in costumeCategories)
		{
			CostumeGroupData dataByGroupName = GameRoot.Instance.CostumeGroupDataContainer.GetDataByGroupName(text);
			int selectItemGroupKey = GameRoot.Instance.CostumeGroupDataContainer.GetSelectItemGroupKey(dataByGroupName.SelectItemGroup);
			if (dataByGroupName != null && selectItemGroupKey == selectItemGroup)
			{
				ComboBoxSub.AddItem(dataByGroupName.CostumeGroupIndex, dataByGroupName.LocalizeGroupName);
			}
		}
	}

	private void SetSubComboBoxItemCostumeIndices(int selectItemGroup)
	{
		if (_itemData == null)
		{
			return;
		}
		SortedDictionary<int, string> sortedDictionary = new SortedDictionary<int, string>();
		CostumeGroupData costumeGroupData = null;
		int[] costumeIndices = _itemData.SubData.CostumeIndices;
		foreach (int key in costumeIndices)
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(key);
			if (data.Group != null)
			{
				costumeGroupData = GameRoot.Instance.CostumeGroupDataContainer.GetDataByGroupName(data.Group);
				int selectItemGroupKey = GameRoot.Instance.CostumeGroupDataContainer.GetSelectItemGroupKey(costumeGroupData.SelectItemGroup);
				if (costumeGroupData != null && selectItemGroupKey == selectItemGroup && !sortedDictionary.ContainsKey(costumeGroupData.CostumeGroupIndex))
				{
					sortedDictionary[costumeGroupData.CostumeGroupIndex] = costumeGroupData.LocalizeGroupName;
				}
			}
		}
		foreach (KeyValuePair<int, string> item in sortedDictionary)
		{
			ComboBoxSub.AddItem(item.Key, item.Value);
		}
	}

	private void SetSubComboBoxCreature()
	{
		if (_selectType == SelectType.SELECT_EQUIP_ARTIFACT)
		{
			return;
		}
		foreach (CreatureTagType item in FilterHelper.GetListOrderCreatureTagType())
		{
			ComboBoxSub.AddItem((int)item, LocalizationManager.GetString(item));
		}
	}

	public void SetSpecificEquipPartType(List<EquipPartType> list)
	{
		_listSpecificEquipPartType = list;
	}

	private void SetSubComboxPartType(List<EquipPartType> list)
	{
		foreach (EquipPartType item in FilterHelper.GetListOrderEquipPartType())
		{
			if ((list != null && list.Contains(item)) || list == null)
			{
				ComboBoxSub.AddItem((int)item, LocalizationManager.GetString(item));
			}
		}
	}

	public SelectType GetEquipItemSelectType(int[] arrayItemIndex)
	{
		ItemData itemData = null;
		EquipItemData equipItemData = null;
		foreach (int index in arrayItemIndex)
		{
			itemData = GameRoot.Instance.ItemDataContainer.GetData(index);
			if (itemData == null)
			{
				continue;
			}
			equipItemData = GameRoot.Instance.EquipItemDataContainer.GetData(itemData.Code);
			if (equipItemData != null)
			{
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(equipItemData.CreatureIndex))
				{
					return SelectType.SELECT_EQUIP_UNIQUE;
				}
				return SelectType.SELECT_EQUIP_ARTIFACT;
			}
		}
		return SelectType.SELECT_EQUIP_ARTIFACT;
	}

	public static bool CheckSameTagType(EquipItemData equipItemData, int key)
	{
		if (key == 0)
		{
			return true;
		}
		CreatureTagType[] tagType = equipItemData.TagType;
		foreach (CreatureTagType creatureTagType in tagType)
		{
			if (key == (int)creatureTagType)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckAddEquipItemByPartType(EquipItemData equipItemData)
	{
		switch (MainComboBoxCurrentKey)
		{
		case ComboBoxType.ALL:
			return CheckSameTagType(equipItemData, SubComboBoxCurrentKey);
		case ComboBoxType.CREATURE:
			return CheckSameTagType(equipItemData, SubComboBoxCurrentKey);
		case ComboBoxType.PART_TYPE:
			if (SubComboBoxCurrentKey == 0 || SubComboBoxCurrentKey == (int)equipItemData.PartType)
			{
				return true;
			}
			return false;
		default:
			return true;
		}
	}

	public bool CheckAddItem(EquipItemData equipItemData)
	{
		if (!SprkArtifactSourceFilter.Matches(this, equipItemData))
		{
			return false;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(equipItemData.CreatureIndex))
		{
			return CheckAddItemByUserItem(equipItemData);
		}
		return CheckAddEquipItemByUniqueWeapon(equipItemData);
	}

	private bool CheckAddItemByUserItem(EquipItemData equipItemData)
	{
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(equipItemData.EquipCode);
		if (dataByCode != null)
		{
			EquipItemInfo equipItemInfo = null;
			foreach (EquipItemInfo item in GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager)
			{
				if (dataByCode.Index == item.ItemIndex)
				{
					equipItemInfo = item;
					break;
				}
			}
			switch (MainComboBoxCurrentKey)
			{
			case ComboBoxType.ALL:
				return true;
			case ComboBoxType.OWNED:
				return equipItemInfo != null;
			case ComboBoxType.NOT_OWNED:
				return equipItemInfo == null;
			}
		}
		return true;
	}

	private bool CheckAddEquipItemByUniqueWeapon(EquipItemData equipItemData)
	{
		if (equipItemData.CreatureIndex.Length != 0)
		{
			int key = equipItemData.CreatureIndex[0];
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(key);
			switch (MainComboBoxCurrentKey)
			{
			case ComboBoxType.ALL:
				return CheckSameTagType(equipItemData, SubComboBoxCurrentKey);
			case ComboBoxType.OWNED:
				if (cachedHeroInfo != null)
				{
					return CheckSameTagType(equipItemData, SubComboBoxCurrentKey);
				}
				return false;
			case ComboBoxType.NOT_OWNED:
				if (cachedHeroInfo == null)
				{
					return CheckSameTagType(equipItemData, SubComboBoxCurrentKey);
				}
				return false;
			}
		}
		return true;
	}

	public bool CheckAddSoulStoneItem(ItemData itemData)
	{
		int valueInt = itemData.GetValueInt(0);
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(valueInt) != null;
		switch (MainComboBoxCurrentKey)
		{
		case ComboBoxType.ALL:
			return CheckSameSoulStoneTagType(itemData);
		case ComboBoxType.OWNED:
			if (flag)
			{
				return CheckSameSoulStoneTagType(itemData);
			}
			return false;
		case ComboBoxType.NOT_OWNED:
			if (!flag)
			{
				return CheckSameSoulStoneTagType(itemData);
			}
			return false;
		default:
			return true;
		}
	}

	private bool CheckExistUniqueItem(int heroIndex, EquipPartType partType, EquipItemSubType subType)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.CheckExistEquipItem(delegate(EquipItemInfo equipItemInfo)
		{
			EquipItemData equipItemData = equipItemInfo.EquipItemData;
			if (equipItemData == null)
			{
				return false;
			}
			if (__Extension_Method_ICollection__.COUNT(equipItemData.CreatureIndex) == 0)
			{
				return false;
			}
			if (equipItemData.CreatureIndex[0] != heroIndex)
			{
				return false;
			}
			return (equipItemData.PartType == partType && equipItemData.SubType == subType) ? true : false;
		});
	}

	private bool CheckSameSoulStoneTagType(ItemData itemData)
	{
		if (SubComboBoxCurrentKey == 0)
		{
			return true;
		}
		if (itemData == null)
		{
			return false;
		}
		CreatureTagType valueEnum = itemData.GetValueEnum<CreatureTagType>(1);
		if (SubComboBoxCurrentKey == (int)valueEnum)
		{
			return true;
		}
		return false;
	}

	private void OnClickedMain(int value)
	{
		if (value == 2 && ComboBoxSecondaryMain != null)
		{
			if (ComboBoxSecondaryMain.GetItemCount() == 0)
			{
				SetSubComboBox();
			}
			else
			{
				ComboBoxSecondaryMain.Open();
			}
		}
		else if (_useSubFilter)
		{
			SetSubComboBox();
		}
		else if (_callback != null)
		{
			_callback();
		}
	}

	private void OnClickedSecondaryMain(int value)
	{
		SetSubComboBox();
	}

	private void OnClickedSub(int value)
	{
		if (_callback != null)
		{
			_callback();
		}
	}
}
