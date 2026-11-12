using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NItemIconSub;
using NGame2.NUI.NHelper.NItem;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ItemListBook : BaseWindow, ItemIconComponent.Listener, OptionSellComponent.Listener
{
	public interface Listener
	{
		void OnEnterItemBook();
	}

	[Serializable]
	public class ItemArchiveFeature
	{
		public ArchiveSubType SubType;

		public UIWidget Tab;

		public UIWidget Info;
	}

	[Serializable]
	public class SubTypeToggleGroup
	{
		public UIToggle UniqueWeapon;

		public UIToggle UniqueTreasure;

		public UIToggle Artifact;

		public UIToggle Equip;

		public UIToggle Normal;

		public UIToggle Rune;
	}

	[Serializable]
	public class ClassTypeToggleGroup
	{
		public UIToggle All;

		public UIToggle Warrior;

		public UIToggle Knight;

		public UIToggle Assassin;

		public UIToggle Archer;

		public UIToggle Mechanic;

		public UIToggle Wizard;

		public UIToggle Priest;

		public void Init()
		{
			All.Set(state: true);
			Warrior.Set(state: false);
			Knight.Set(state: false);
			Assassin.Set(state: false);
			Archer.Set(state: false);
			Mechanic.Set(state: false);
			Wizard.Set(state: false);
			Priest.Set(state: false);
		}
	}

	[Serializable]
	public class EquipPartTypeToggleGroup
	{
		public UIToggle Weapon;

		public UIToggle Armor;

		public UIToggle SecondGear;

		public UIToggle Accessory;

		public UIToggle Orb;

		public UIToggle Treasure;

		public void Init()
		{
			Weapon.Set(state: true);
			Armor.Set(state: false);
			SecondGear.Set(state: false);
			Accessory.Set(state: false);
			Orb.Set(state: false);
			Treasure.Set(state: false);
		}
	}

	[Serializable]
	public class NormalTypeToggleGroup
	{
		public UIToggle Growth;

		public UIToggle Consumable;

		public UIToggle Etc;

		public void Init()
		{
			Growth.Set(state: true);
			Consumable.Set(state: false);
			Etc.Set(state: false);
		}
	}

	[Serializable]
	public class RuneTypeToggleGroup
	{
		public UIToggle NormalRune;

		public UIToggle PunishmentRune;

		public void Init()
		{
			NormalRune.Set(state: true);
			PunishmentRune.Set(state: false);
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/IllustratedBook/ItemListBook";

	private Listener _listener;

	public List<ItemArchiveFeature> FeatureList = new List<ItemArchiveFeature>();

	public UIScrollGrid ItemGrid;

	public SubTypeToggleGroup SubTypeToggle;

	public ClassTypeToggleGroup ClassTypeToggle;

	public EquipPartTypeToggleGroup EquipPartTypeToggle;

	public NormalTypeToggleGroup NormalTypeToggle;

	public RuneTypeToggleGroup RuneTypeToggle;

	public ArchiveItemInfo ArchiveItemInfo;

	public UISprite SpriteGauge;

	public UILabel LabelGauge;

	private ArchiveSubType _subType;

	private CreatureTagType _classType;

	private EquipPartType _equipPartType;

	private ArchiveSubType2 _subType2;

	private ItemType _runeType;

	private int selectedEquipGrade;

	private Dictionary<ArchiveSubType, Action> toggleActionDic = new Dictionary<ArchiveSubType, Action>();

	private List<ItemIconComponent> itemIconComponentList = new List<ItemIconComponent>();

	[Space(10f)]
	[Header("EquipItemGradeComboBox")]
	public UIPopupRoot GradeRoot;

	public UISimpleGrid GradeHead;

	public UIScrollGrid GradeGrid;

	protected OptionSellComponent _gradeHead;

	protected List<OptionSellComponent> _gradeList = new List<OptionSellComponent>();

	protected OptionSellData _allOptionSellData;

	public void Open(Listener listener)
	{
		_listener = listener;
		base.Open(delegate
		{
			Init();
		});
		GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Count();
	}

	public void Init()
	{
		toggleActionDic.Add(ArchiveSubType.UniqueWeapon, EnableUniqueWeapon);
		toggleActionDic.Add(ArchiveSubType.UniqueTreasure, EnableUniqueTreasure);
		toggleActionDic.Add(ArchiveSubType.Artifact, EnableArtifact);
		toggleActionDic.Add(ArchiveSubType.EquipItem, EnableEquip);
		toggleActionDic.Add(ArchiveSubType.NormalItem, EnableNormal);
		toggleActionDic.Add(ArchiveSubType.Rune, EnableRune);
		_subType = ArchiveSubType.None;
		OnToggle();
		InitComboBox();
		EquipPartTypeToggle.Treasure.transform.parent.gameObject.SetActive(value: false);
		EquipPartTypeToggle.Orb.transform.parent.gameObject.SetActive(value: false);
		RuneTypeToggle.PunishmentRune.transform.parent.gameObject.SetActive(value: false);
	}

	private void InitComboBox()
	{
		OptionSellData[] categoryData = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.Grade);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(categoryData))
		{
			return;
		}
		if (__Extension_Method_ICollection__.COUNT(categoryData) > 1)
		{
			Array.Sort(categoryData, delegate(OptionSellData a, OptionSellData b)
			{
				ItemGradeData data = GameRoot.Instance.ItemGradeDataContainer.GetData(a.Value);
				ItemGradeData data2 = GameRoot.Instance.ItemGradeDataContainer.GetData(b.Value);
				byte b2 = data?.Score ?? 1;
				byte value = data2?.Score ?? 1;
				return b2.CompareTo(value) * -1;
			});
		}
		int width = 0;
		UIWidget component = GradeHead.GetComponent<UIWidget>();
		if (component != null)
		{
			width = component.width;
		}
		selectedEquipGrade = 0;
		if (_allOptionSellData == null)
		{
			_allOptionSellData = new OptionSellData();
			_allOptionSellData = categoryData[0].GetClone();
			_allOptionSellData.Value = 0;
			_allOptionSellData.GradeStringKey = LocalizationManager.GetStringDirect("BROADCAST.BROADCAST_TAG_ALL");
		}
		_gradeHead = UIHelper.AddGridItem(GradeHead, OptionSellComponent.PrefabAuto);
		_gradeHead.Open(this, _allOptionSellData, onHead: true, width);
		for (int num = -1; num < categoryData.Length; num++)
		{
			OptionSellComponent optionSellComponent = UIHelper.AddGridItem(GradeGrid, OptionSellComponent.PrefabAuto);
			optionSellComponent.Open(this, (num < 0) ? _allOptionSellData : categoryData[num], onHead: false, width);
			_gradeList.Add(optionSellComponent);
		}
		UIHelper.RepositionGrid(GradeHead);
		UIHelper.RepositionGrid(GradeGrid);
	}

	public override void Reset()
	{
		base.Reset();
		toggleActionDic.Clear();
		ClearItemItemComponent();
		SubTypeToggle.UniqueWeapon.Set(state: true);
		FactoryBase.SafeReleaseComponent(ref _gradeHead);
		FactoryBase.ReleaseAndClearList(_gradeList);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		ArchiveItemInfo.ChainUpdate();
	}

	private void ClearItemItemComponent()
	{
		FactoryBase.ReleaseAndClearList(itemIconComponentList);
	}

	public void OnClickItem()
	{
		_listener.OnEnterItemBook();
	}

	public void OnToggle()
	{
		ArchiveSubType archiveSubType = ArchiveSubType.None;
		if (SubTypeToggle.UniqueWeapon.value)
		{
			archiveSubType = ArchiveSubType.UniqueWeapon;
		}
		else if (SubTypeToggle.UniqueTreasure.value)
		{
			archiveSubType = ArchiveSubType.UniqueTreasure;
		}
		else if (SubTypeToggle.Artifact.value)
		{
			archiveSubType = ArchiveSubType.Artifact;
		}
		else if (SubTypeToggle.Equip.value)
		{
			archiveSubType = ArchiveSubType.EquipItem;
		}
		else if (SubTypeToggle.Normal.value)
		{
			archiveSubType = ArchiveSubType.NormalItem;
		}
		else if (SubTypeToggle.Rune.value)
		{
			archiveSubType = ArchiveSubType.Rune;
		}
		if (archiveSubType != ArchiveSubType.None && _subType != archiveSubType)
		{
			_subType = archiveSubType;
			if (toggleActionDic.ContainsKey(_subType))
			{
				Vespa_System_Action_Extension_Method.fire(toggleActionDic[_subType]);
			}
		}
	}

	private void SetupItemInfo()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(itemIconComponentList))
		{
			ItemIconComponent itemIconComponent = itemIconComponentList.First();
			SetupItemInfo(itemIconComponent.ItemData);
		}
	}

	private void SetupItemInfo(ItemData itemData)
	{
		ArchiveItemInfo.Reset();
		ArchiveItemInfo.Init(_subType, itemData);
	}

	private void SetupGauge(UISprite spriteGauge, UILabel labelGauge, int totalCount, int curCount)
	{
		if (curCount >= totalCount)
		{
			string text = $"{((totalCount != 0) ? 100 : 0):N2}% ({curCount}/{totalCount})";
			UIHelper.SetSpriteFillAmount(spriteGauge, (totalCount != 0) ? 1 : 0);
			UIHelper.SetLabelText(labelGauge, text);
		}
		else
		{
			float num = Mathf.Clamp((float)curCount / (float)totalCount, 0f, 100f);
			string text2 = $"{num * 100f:N2}% ({curCount}/{totalCount})";
			UIHelper.SetLabelText(labelGauge, text2);
			UIHelper.SetSpriteFillAmount(spriteGauge, num);
		}
	}

	private void EnableUniqueWeapon()
	{
		SetupFeature(_subType);
		_classType = CreatureTagType.None;
		ClassTypeToggle.Init();
		SetupGrid();
		SelectFirstItemIcon();
		SetupItemInfo();
	}

	private void EnableUniqueTreasure()
	{
		SetupFeature(_subType);
		_classType = CreatureTagType.None;
		ClassTypeToggle.Init();
		SetupGrid();
		SelectFirstItemIcon();
		SetupItemInfo();
	}

	private void EnableArtifact()
	{
		SetupFeature(_subType);
		SetupGrid();
		SelectFirstItemIcon();
		SetupItemInfo();
	}

	private void EnableEquip()
	{
		SetupFeature(_subType);
		_equipPartType = EquipPartType.Weapon;
		EquipPartTypeToggle.Init();
		SetupGrid();
		SelectFirstItemIcon();
		SetupItemInfo();
	}

	private void EnableNormal()
	{
		SetupFeature(_subType);
		_subType2 = ArchiveSubType2.Growth;
		NormalTypeToggle.Init();
		SetupGrid();
		SelectFirstItemIcon();
		SetupItemInfo();
	}

	private void EnableRune()
	{
		SetupFeature(_subType);
		_runeType = ItemType.Rune;
		RuneTypeToggle.Init();
		SetupGrid();
		SelectFirstItemIcon();
		SetupItemInfo();
	}

	private void SetupFeature(ArchiveSubType subType)
	{
		ItemArchiveFeature itemArchiveFeature = null;
		foreach (ItemArchiveFeature feature in FeatureList)
		{
			UIHelper.EnableGameObject(feature.Tab, feature.SubType == subType);
			if (feature.SubType == subType)
			{
				itemArchiveFeature = feature;
			}
		}
		if (itemArchiveFeature != null)
		{
			UIHelper.EnableGameObject(itemArchiveFeature.Tab, enable: true);
		}
	}

	private void SetupTab()
	{
	}

	private void SetupInfo()
	{
	}

	private void SetupGridUniqueEquip()
	{
		PlayerArchiveData[] playerArchiveDatas = GameRoot.Instance.PlayerArchiveDataContainer.GetDataBySubType(_subType);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(playerArchiveDatas))
		{
			return;
		}
		int totalCount = 0;
		int ownCount = 0;
		GameRoot.Instance.EquipItemDataContainer.ForEachData(delegate(EquipItemData equipItemData)
		{
			int[] array = null;
			PlayerArchiveData[] array2 = playerArchiveDatas;
			foreach (PlayerArchiveData playerArchiveData in array2)
			{
				if (playerArchiveData.ConditionKey3.Equals("DetailIndex"))
				{
					array = playerArchiveData.ConditionValue3;
				}
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(array))
			{
				bool flag = false;
				int[] array3 = array;
				for (int i = 0; i < array3.Length; i++)
				{
					if (array3[i] == equipItemData.DetailIndex)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					if (_classType != CreatureTagType.None)
					{
						bool flag2 = false;
						CreatureTagType[] tagType = equipItemData.TagType;
						for (int i = 0; i < tagType.Length; i++)
						{
							if (tagType[i] == _classType)
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							return;
						}
					}
					ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(equipItemData.EquipCode);
					if (dataByCode != null && dataByCode.IsArchive)
					{
						ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
						itemIconComponent.Open(dataByCode, this);
						bool flag3 = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.IsContainItem(dataByCode.Index);
						itemIconComponent.SetIconGrayScale(!flag3);
						if (flag3)
						{
							ownCount++;
						}
						totalCount++;
						int itemAwake = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.GetItemAwake(dataByCode.Index);
						itemIconComponent.SetStar(itemAwake);
						itemIconComponentList.Add(itemIconComponent);
					}
				}
			}
		});
		UIHelper.RepositionGrid(ItemGrid);
		SetupGauge(SpriteGauge, LabelGauge, totalCount, ownCount);
		Debug.LogWarning($"{ownCount} / {totalCount}");
	}

	private void SetupGridEquipItem()
	{
		PlayerArchiveData[] playerArchiveDatas = GameRoot.Instance.PlayerArchiveDataContainer.GetDataBySubType(_subType);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(playerArchiveDatas))
		{
			return;
		}
		int totalCount = 0;
		int ownCount = 0;
		List<ItemData> equipItemDataList = new List<ItemData>();
		GameRoot.Instance.ItemDataContainer.ForEachData(delegate(ItemData itemData)
		{
			if (itemData.IsArchive && itemData.Type == ItemType.Equip && (selectedEquipGrade == 0 || itemData.Grade == selectedEquipGrade))
			{
				EquipItemData subData = itemData.GetSubData<EquipItemData>();
				if (subData != null && subData.PartType == _equipPartType)
				{
					equipItemDataList.Add(itemData);
				}
			}
		});
		ItemData[] array = equipItemDataList.ToArray();
		Array.Sort(array, (ItemData lhs, ItemData rhs) => SortItem(lhs, rhs, ItemComparer.ItemListBookSort));
		Array.ForEach(array, delegate(ItemData itemData)
		{
			List<int> list = new List<int>();
			PlayerArchiveData[] array2 = playerArchiveDatas;
			foreach (PlayerArchiveData playerArchiveData in array2)
			{
				if (playerArchiveData.ConditionKey2.Equals("ItemGrade"))
				{
					list.AddRange(playerArchiveData.ConditionValue2);
				}
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
			{
				bool flag = false;
				foreach (int item in list)
				{
					if (item == itemData.Grade)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
					itemIconComponent.Open(itemData, this);
					bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.IsContainItem(itemData.Index);
					itemIconComponent.SetIconGrayScale(!flag2);
					if (flag2)
					{
						ownCount++;
					}
					totalCount++;
					itemIconComponentList.Add(itemIconComponent);
				}
			}
		});
		UIHelper.RepositionGrid(ItemGrid);
		SetupGauge(SpriteGauge, LabelGauge, totalCount, ownCount);
		Debug.LogWarning($"{ownCount} / {totalCount}");
	}

	private void SetupGridNormalItem()
	{
		PlayerArchiveData[] playerArchiveDatas = GameRoot.Instance.PlayerArchiveDataContainer.GetDataBySubType(_subType, _subType2);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(playerArchiveDatas))
		{
			return;
		}
		int totalCount = 0;
		int ownCount = 0;
		List<ItemData> itemDataList = new List<ItemData>();
		GameRoot.Instance.ItemDataContainer.ForEachData(delegate(ItemData itemData)
		{
			if (itemData.IsArchive && itemData.Type != ItemType.Equip)
			{
				itemDataList.Add(itemData);
			}
		});
		ItemData[] array = itemDataList.ToArray();
		Array.Sort(array, (ItemData lhs, ItemData rhs) => SortItem(lhs, rhs, ItemComparer.ItemListBookSort));
		Array.ForEach(array, delegate(ItemData itemData)
		{
			List<ItemType> list = new List<ItemType>();
			PlayerArchiveData[] array2 = playerArchiveDatas;
			foreach (PlayerArchiveData playerArchiveData in array2)
			{
				if (playerArchiveData.ConditionKey1.Equals("ItemType"))
				{
					list.AddRange(playerArchiveData.ConditionValue1);
				}
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
			{
				bool flag = false;
				foreach (ItemType item in list)
				{
					if (item == itemData.Type)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
					itemIconComponent.Open(itemData, this);
					bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.IsContainItem(itemData.Index);
					itemIconComponent.SetIconGrayScale(!flag2);
					if (flag2)
					{
						ownCount++;
					}
					totalCount++;
					itemIconComponentList.Add(itemIconComponent);
				}
			}
		});
		UIHelper.RepositionGrid(ItemGrid);
		SetupGauge(SpriteGauge, LabelGauge, totalCount, ownCount);
		Debug.LogWarning($"{ownCount} / {totalCount}");
	}

	private void SetupGridRune()
	{
		ItemData[] array = GameRoot.Instance.ItemDataContainer.GetItemDataListByType(_runeType).ToArray();
		Array.Sort(array, (ItemData lhs, ItemData rhs) => SortItem(lhs, rhs, ItemComparer.ItemListBookSort));
		int num = 0;
		int num2 = 0;
		foreach (ItemData itemData in array)
		{
			if (itemData.IsArchive)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(itemData, this);
				itemIconComponentList.Add(itemIconComponent);
				bool flag = GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.IsContainItem(itemData.Index);
				itemIconComponent.SetIconGrayScale(!flag);
				if (flag)
				{
					num2++;
				}
				num++;
			}
		}
		UIHelper.RepositionGrid(ItemGrid);
		SetupGauge(SpriteGauge, LabelGauge, num, num2);
	}

	private void SetupGrid()
	{
		ClearItemItemComponent();
		if (_subType == ArchiveSubType.UniqueWeapon || _subType == ArchiveSubType.UniqueTreasure || _subType == ArchiveSubType.Artifact)
		{
			SetupGridUniqueEquip();
		}
		else if (_subType == ArchiveSubType.EquipItem)
		{
			SetupGridEquipItem();
		}
		else if (_subType == ArchiveSubType.NormalItem)
		{
			SetupGridNormalItem();
		}
		else if (_subType == ArchiveSubType.Rune)
		{
			SetupGridRune();
		}
	}

	private int SortItem(ItemData lhs, ItemData rhs, Comparison<ItemData> SortFunc)
	{
		if (lhs == rhs)
		{
			return 0;
		}
		if (lhs == null || rhs == null)
		{
			if (lhs != null)
			{
				return -1;
			}
			if (rhs != null)
			{
				return 1;
			}
			return 0;
		}
		return SortFunc(lhs, rhs);
	}

	private void ResetSubToggle()
	{
		_classType = CreatureTagType.None;
		_equipPartType = EquipPartType.Weapon;
		_subType2 = ArchiveSubType2.Growth;
	}

	public void OnToggleUniqueTab()
	{
		CreatureTagType creatureTagType = CreatureTagType.None;
		if (ClassTypeToggle.All.value)
		{
			creatureTagType = CreatureTagType.None;
		}
		else if (ClassTypeToggle.Warrior.value)
		{
			creatureTagType = CreatureTagType.Warrior;
		}
		else if (ClassTypeToggle.Knight.value)
		{
			creatureTagType = CreatureTagType.Knight;
		}
		else if (ClassTypeToggle.Assassin.value)
		{
			creatureTagType = CreatureTagType.Assassin;
		}
		else if (ClassTypeToggle.Archer.value)
		{
			creatureTagType = CreatureTagType.Archer;
		}
		else if (ClassTypeToggle.Mechanic.value)
		{
			creatureTagType = CreatureTagType.Mechanic;
		}
		else if (ClassTypeToggle.Wizard.value)
		{
			creatureTagType = CreatureTagType.Wizard;
		}
		else if (ClassTypeToggle.Priest.value)
		{
			creatureTagType = CreatureTagType.Priest;
		}
		if (_classType != creatureTagType)
		{
			ResetSubToggle();
			_classType = creatureTagType;
			SetupGrid();
			SelectFirstItemIcon();
			SetupItemInfo();
		}
	}

	public void OnToggleEquipTab()
	{
		EquipPartType equipPartType = EquipPartType.None;
		if (EquipPartTypeToggle.Weapon.value)
		{
			equipPartType = EquipPartType.Weapon;
		}
		else if (EquipPartTypeToggle.Armor.value)
		{
			equipPartType = EquipPartType.Armor;
		}
		else if (EquipPartTypeToggle.SecondGear.value)
		{
			equipPartType = EquipPartType.SecondGear;
		}
		else if (EquipPartTypeToggle.Accessory.value)
		{
			equipPartType = EquipPartType.Accessory;
		}
		else if (EquipPartTypeToggle.Orb.value)
		{
			equipPartType = EquipPartType.Orb;
		}
		else if (EquipPartTypeToggle.Treasure.value)
		{
			equipPartType = EquipPartType.Treasure;
		}
		if (equipPartType != EquipPartType.None && _equipPartType != equipPartType)
		{
			ResetSubToggle();
			_equipPartType = equipPartType;
			SetupGrid();
			SelectFirstItemIcon();
			SetupItemInfo();
		}
	}

	public void OnToggleNormalTab()
	{
		ArchiveSubType2 archiveSubType = ArchiveSubType2.None;
		if (NormalTypeToggle.Growth.value)
		{
			archiveSubType = ArchiveSubType2.Growth;
		}
		else if (NormalTypeToggle.Consumable.value)
		{
			archiveSubType = ArchiveSubType2.Consumable;
		}
		else if (NormalTypeToggle.Etc.value)
		{
			archiveSubType = ArchiveSubType2.Etc;
		}
		if (archiveSubType != ArchiveSubType2.None && _subType2 != archiveSubType)
		{
			ResetSubToggle();
			_subType2 = archiveSubType;
			SetupGrid();
			SelectFirstItemIcon();
			SetupItemInfo();
		}
	}

	public void OnToggleRuneTab()
	{
		ItemType itemType = ItemType.None;
		if (RuneTypeToggle.NormalRune.value)
		{
			itemType = ItemType.Rune;
		}
		else if (RuneTypeToggle.PunishmentRune.value)
		{
			itemType = ItemType.PunishmentRune;
		}
		if (itemType != ItemType.None && _runeType != itemType)
		{
			ResetSubToggle();
			_runeType = itemType;
			SetupGrid();
			SelectFirstItemIcon();
			SetupItemInfo();
		}
	}

	private void SelectFirstItemIcon()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(itemIconComponentList))
		{
			ItemIconComponent itemIconComponent = itemIconComponentList.First();
			SelectItemIcon(itemIconComponent);
		}
	}

	private void SelectItemIcon(ItemIconComponent itemIconComponent)
	{
		foreach (ItemIconComponent itemIconComponent2 in itemIconComponentList)
		{
			itemIconComponent2.SetSelected(selected: false, SelectSubComponent.ESelectType.Select_BorderBook);
		}
		itemIconComponent.SetSelected(selected: true, SelectSubComponent.ESelectType.Select_BorderBook);
	}

	public void OnClick(ItemIconComponent itemIconComponent)
	{
		SelectItemIcon(itemIconComponent);
		SetupItemInfo(itemIconComponent.ItemData);
	}

	void OptionSellComponent.Listener.OnClick(OptionSellData optionData, bool onHead)
	{
		if (onHead)
		{
			if (optionData.Category == OptionSellCategory.Grade)
			{
				GradeRoot.TogglePopup();
			}
			return;
		}
		if (optionData.Category == OptionSellCategory.Grade)
		{
			int num = _gradeList.FindIndex((OptionSellComponent sellComponent) => sellComponent.OptionData == optionData);
			if (num >= 0)
			{
				selectedEquipGrade = _gradeList[num].GetValue();
			}
			_gradeHead.Refresh(optionData);
			GradeRoot.HidePopup();
		}
		SetupGrid();
		SelectFirstItemIcon();
		SetupItemInfo();
	}

	List<int> OptionSellComponent.Listener.GetSelectedGrade()
	{
		return new List<int> { selectedEquipGrade };
	}
}
