using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NItemIconSub;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NHelper.NItem;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroEquipAwaken : BaseWindow, ItemIconComponent.Listener, HeroEquipListComponent.Listener, EquipAwakenScene.IDataProvider, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>, IUserObserver, IInspectorGuiDrawer
{
	public interface Listener
	{
		bool OnCloseRequest(HeroEquipAwaken caller);

		void OnAwaken(HeroEquipAwaken caller);

		void OnClickMaterial(HeroEquipAwaken caller, ItemIconComponent itemIconComponent);
	}

	public enum ESortType
	{
		Up,
		Down,
		Max
	}

	[Serializable]
	public class UpDownSpriteInfo
	{
		public UISprite SpriteUp;

		public UISprite SpriteDown;

		public void SetSprite(ESortType sortType)
		{
			UIHelper.EnableGameObject(SpriteUp, sortType == ESortType.Up);
			UIHelper.EnableGameObject(SpriteDown, sortType == ESortType.Down);
		}
	}

	public class ItemInfoAndComponent
	{
		private ItemIconComponent _targetComp;

		private ItemIconComponent _resultComp;

		public EquipItemInfo EquipItemInfo { get; private set; }

		public ItemData ItemData { get; private set; }

		public EquipItemData EquipItemData { get; private set; }

		public int BeforeStar { get; private set; }

		public int AfterStar { get; private set; }

		public StatData BeforeStat { get; private set; }

		public StatData AfterStat { get; private set; }

		public bool IsEmpty()
		{
			if (EquipItemInfo != null)
			{
				return EquipItemInfo.ItemIndex == 0;
			}
			return true;
		}

		public bool IsEqualsItem(ItemIconComponent component)
		{
			if (component.EquipItemInfo == null)
			{
				return false;
			}
			if (!Vespa_Common_Class_Extension_Method.EQUALS(_targetComp, component))
			{
				return EquipItemInfo.SlotIndex == component.EquipItemInfo.SlotIndex;
			}
			return true;
		}

		public void Create(EquipItemInfo equipItemInfo, Transform targetParent, Transform resultParent, HeroEquipAwaken owner)
		{
			Close();
			if (equipItemInfo == null || equipItemInfo.ItemIndex == 0)
			{
				return;
			}
			ItemData = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfo.ItemIndex);
			if (ItemData != null)
			{
				EquipItemData = ItemData.GetSubData<EquipItemData>();
				if (EquipItemData != null)
				{
					EquipItemInfo = equipItemInfo;
					_targetComp = ItemIconComponent.PrefabAuto.Factory.Create(targetParent);
					_targetComp.transform.localScale = Vector3.one;
					_targetComp.transform.localPosition = Vector3.zero;
					_resultComp = ItemIconComponent.PrefabAuto.Factory.Create(resultParent);
					_resultComp.transform.localScale = Vector3.one;
					_resultComp.transform.localPosition = Vector3.zero;
					BeforeStar = EquipItemInfo.Star;
					AfterStar = GetExpectedAwakenStar();
					BeforeStat = StatCalculator.GetEquipItemStatData(EquipItemInfo.ItemIndex, BeforeStar, EquipItemInfo.Level);
					AfterStat = StatCalculator.GetEquipItemStatData(EquipItemInfo.ItemIndex, AfterStar, EquipItemInfo.Level);
					EquipItemInfo equipItemInfo2 = EquipItemInfo.Clone();
					equipItemInfo2.Star = AfterStar;
					equipItemInfo2.SoulWeaponInfo = EquipItemInfo.SoulWeaponInfo;
					_targetComp.Open(ItemData, EquipItemInfo, owner);
					_resultComp.Open(ItemData, equipItemInfo2, null);
					_resultComp.SetStarBlink();
				}
			}
		}

		public void Close()
		{
			if (!(_targetComp == null))
			{
				FactoryBase.ReleaseComponent(_targetComp);
				_targetComp = null;
				EquipItemInfo = null;
				if (!(_resultComp == null))
				{
					FactoryBase.ReleaseComponent(_resultComp);
					_resultComp = null;
				}
			}
		}

		public int GetExpectedAwakenStar()
		{
			if (EquipItemInfo == null)
			{
				return 0;
			}
			int b = EquipItemInfo.CLI_GetMaxStar_for_Awaken();
			return Mathf.Min(BeforeStar + 1, b);
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Equip/HeroEquipAwaken";

	public ItemInfoAndComponent TargetItem = new ItemInfoAndComponent();

	[Header("[AwakeRoot]")]
	public Transform EventRoot;

	public UILabel Label_UpperMsg;

	public Transform TargetEquipItemRoot;

	public Transform ResultEquipItemRoot;

	public UIAnimationScrollGrid MaterialEquipItemGrid;

	public UIWidget WidgetChance;

	public UILabel LabelBaseChance;

	public UILabel LabelAddedChance;

	public UILabel PriceLabel;

	public UILabel OriginalPriceLabel;

	public UILabelAligner PriceLabelAligner;

	public UISprite EventButtonImage;

	public UIPanel StatChangePanel;

	public UIScrollView StatChangeScrollView;

	public UIButton Button_Awaken;

	public float MaterialGridMaxWidth;

	[Header("[EquipListRoot]")]
	public UIScrollGrid AwakeInvenItemListGrid;

	public UIScrollGrid HeroEquipItemGrid;

	public UpDownSpriteInfo SortReqLevelSprites;

	public UpDownSpriteInfo SortGradeSprites;

	public UpDownSpriteInfo SortStarSprites;

	public UpDownSpriteInfo SortHeroLevelSprites;

	public UpDownSpriteInfo SortHeroGradeSprites;

	public UIWidget HeroEquipWidget;

	public UIToggle HeroEquipToggle;

	public UIWidget InventoryEquipWidget;

	public UIToggle InventoryEquipToggle;

	public Transform HeroEquipRoot;

	public Transform InventoryEquipRoot;

	public UILabel Label_NoAwakenEquipItem;

	[Header("[Color Template]")]
	public Color StatFieldColor = new Color32(231, 214, 196, byte.MaxValue);

	public Color StatBeforeValueColor = new Color32(168, 159, 142, byte.MaxValue);

	public Color StatAfterColor = new Color32(2, byte.MaxValue, 218, byte.MaxValue);

	public Color32 SuccessRateColor = Color.white;

	public Color32 OverSuccessRateColor = new Color32(byte.MaxValue, 233, 0, byte.MaxValue);

	public Color NormalGoldLabelColor;

	public Color WarningGoldLabelColor;

	private Listener _listener;

	private Dictionary<int, List<EquipItemInfo>> _DictAwakeInvenItems;

	private List<ItemIconComponent> _ListAwakeInvenItems = new List<ItemIconComponent>();

	private List<ItemIconComponent> _ListMaterialItems = new List<ItemIconComponent>();

	private List<ItemIconComponent> _ListSelectedItems = new List<ItemIconComponent>();

	private List<ItemIconComponent> _ListPublicAwakenItemComponents = new List<ItemIconComponent>();

	private List<HeroEquipListComponent> _ListAwakeEquipItems = new List<HeroEquipListComponent>();

	private List<ItemIconComponent[]> _ListPublicAwakenItemMaterialPair = new List<ItemIconComponent[]>();

	private StatListComponent _statListComponent;

	private List<EquipItemInfo> _ListCanUseAwakeItems = new List<EquipItemInfo>();

	private List<CachedHeroInfo> _ListHeros = new List<CachedHeroInfo>();

	private List<int> InvenItemIndices = new List<int>();

	private List<ItemInfo> _ListPublicAwakenItems = new List<ItemInfo>();

	private ESortType _ReqLevelSortType;

	private ESortType _GradeSortType;

	private ESortType _StarSortType;

	private ESortType _HeroLevelSortType;

	private ESortType _HeroGradeSortType;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
		Init();
	}

	private void Init()
	{
		if (AwakeInvenItemListGrid != null)
		{
			AwakeInvenItemListGrid.Grid.onCustomSort = GridCustomCompareInven;
		}
		if (HeroEquipItemGrid != null)
		{
			HeroEquipItemGrid.Grid.onCustomSort = GridCustomCompareHero;
		}
		InitToggleButton();
		UpdateAwakeUITask();
		SetupSortButton();
		SetupPublicAwakenItemList();
		SetupAwakenInvenItemList();
		SetupHeroEquipAwakeList();
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.DiscountAwakeEquip, null);
		UIHelper.EnableGameObject(EventRoot, effectiveEvent != null);
		UIHelper.EnableGameObject(EventButtonImage, effectiveEvent != null);
	}

	private void SetupPublicAwakenItemList(bool ReloadList = true)
	{
		if (ReloadList)
		{
			_ListPublicAwakenItems.Clear();
			List<ItemInfo> publicAwakenItemList = GlobalAutoBehaviour<AccountManager>.instance.GetPublicAwakenItemList();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(publicAwakenItemList))
			{
				_ListPublicAwakenItems.AddRange(publicAwakenItemList);
			}
		}
		_ListPublicAwakenItems.Sort(delegate(ItemInfo l, ItemInfo r)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(l.ItemIndex);
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(r.ItemIndex);
			if (data == null || data2 == null)
			{
				return 0;
			}
			ItemComparer.SortClass sortClass = new ItemComparer.SortClass();
			if (TargetItem.EquipItemInfo != null)
			{
				if (TargetItem.EquipItemInfo.IsUniqueWeaponWithoutClassWeapon)
				{
					sortClass.Add(ItemComparer.Compare, data.Type == ItemType.PublicUniqueEquipAwaken, data2.Type == ItemType.PublicUniqueEquipAwaken, isAscending: false);
				}
				else if (TargetItem.EquipItemInfo.IsUniqueTreasure)
				{
					sortClass.Add(ItemComparer.Compare, data.Type == ItemType.PublicTreasureAwaken, data2.Type == ItemType.PublicTreasureAwaken, isAscending: false);
				}
			}
			sortClass.Add(ItemComparer.Compare, data.AwakenStar, data2.AwakenStar, _StarSortType == ESortType.Up);
			return sortClass.Sort();
		});
		if (ReloadList)
		{
			ClearPublicAwakenIconList();
			_ListPublicAwakenItems.ForEach(delegate(ItemInfo itemInfo)
			{
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
				for (int i = 0; i < itemInfo.Count; i++)
				{
					ItemIconComponent itemIconComponent = UIHelper.AddGridItem(AwakeInvenItemListGrid, ItemIconComponent.PrefabAuto);
					itemIconComponent.Open(data, this);
					itemIconComponent.SetStar(data.AwakenStar);
					_ListPublicAwakenItemComponents.Add(itemIconComponent);
				}
			});
		}
		UIHelper.RepositionGrid(AwakeInvenItemListGrid);
		UpdatePublicAwakeItemListTask();
	}

	private void SetupAwakenInvenItemList(bool ReloadList = true)
	{
		if (ReloadList)
		{
			ClearInvenItemList();
			_DictAwakeInvenItems = GlobalAutoBehaviour<AccountManager>.instance.GetCanUseAwakeEquipItemList(noTechnomagic: true);
			Dictionary<int, List<EquipItemInfo>>.KeyCollection.Enumerator enumerator = _DictAwakeInvenItems.Keys.GetEnumerator();
			while (enumerator.MoveNext())
			{
				InvenItemIndices.Add(enumerator.Current);
			}
		}
		SortAwakeInvenItemList();
		if (ReloadList)
		{
			ClearInvenItemIconList();
			_ListCanUseAwakeItems.ForEach(delegate(EquipItemInfo equipItemInfo)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(AwakeInvenItemListGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(equipItemInfo, this);
				_ListAwakeInvenItems.Add(itemIconComponent);
			});
		}
		UIHelper.RepositionGrid(AwakeInvenItemListGrid);
		UpdateAwakeEquipItemListTask();
	}

	private void SetupHeroEquipAwakeList(bool ReloadList = true)
	{
		if (ReloadList)
		{
			ClearHeroList();
			CachedHeroInfo[] array = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ToArray();
			foreach (CachedHeroInfo item in array)
			{
				_ListHeros.Add(item);
			}
		}
		SortAwakeEquipItemList();
		if (ReloadList)
		{
			ClearEquipItemIconList();
			_ListHeros.ForEach(delegate(CachedHeroInfo heroInfo)
			{
				HeroEquipListComponent heroEquipListComponent = UIHelper.AddGridItem(HeroEquipItemGrid, HeroEquipListComponent.PrefabAuto);
				heroEquipListComponent.Open(heroInfo, this, selectable: false, useDispatch: true);
				_ListAwakeEquipItems.Add(heroEquipListComponent);
				heroEquipListComponent.ForEachItemComponent(delegate(ItemIconComponent itemComponent)
				{
					itemComponent.SetDisabled(disabled: true);
					EquipItemInfo equipItemInfo = itemComponent.EquipItemInfo;
					if (equipItemInfo.CLI_CanAwaken())
					{
						bool flag = false;
						if (equipItemInfo.IsUniqueWeaponWithoutClassWeapon)
						{
							flag = GlobalAutoBehaviour<AccountManager>.instance.CheckHavePublicUniqueEquipAwakenItem();
						}
						else if (equipItemInfo.IsUniqueTreasure)
						{
							flag = GlobalAutoBehaviour<AccountManager>.instance.CheckHavePublicTreasureAwakenItem();
						}
						if (_DictAwakeInvenItems.ContainsKey(equipItemInfo.ItemIndex) || flag)
						{
							itemComponent.SetDisabled(disabled: false);
						}
					}
				});
			});
		}
		UIHelper.RepositionGrid(HeroEquipItemGrid);
	}

	private void InitToggleButton()
	{
		HeroEquipToggle.Set(state: true);
		InventoryEquipToggle.Set(state: false);
		UpdateTab();
	}

	private void SetupSortButton()
	{
		SortReqLevelSprites.SetSprite(_ReqLevelSortType);
		SortGradeSprites.SetSprite(_GradeSortType);
		SortStarSprites.SetSprite(_StarSortType);
		SortHeroLevelSprites.SetSprite(_HeroLevelSortType);
		SortHeroGradeSprites.SetSprite(_HeroGradeSortType);
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Close()
	{
		base.Close();
		ResetHeroEquipAwaken();
	}

	public void ResetHeroEquipAwaken()
	{
		ClearIcon();
		ClearMaterialEquipIcon();
		ClearStatChange();
		ClearInvenItemList();
		ClearInvenItemIconList();
		ClearPublicAwakenIconList();
		ClearHeroList();
		ClearEquipItemIconList();
		_ReqLevelSortType = ESortType.Down;
		_GradeSortType = ESortType.Down;
		_StarSortType = ESortType.Down;
		_HeroGradeSortType = ESortType.Down;
		_HeroLevelSortType = ESortType.Down;
		_listener = null;
	}

	private void ClearIcon()
	{
		TargetItem.Close();
		UpdateTab();
		SprkForgeAwakenChance.Sync(this);
	}

	private void ClearMaterialEquipIcon()
	{
		_ListSelectedItems.ForEach(delegate(ItemIconComponent x)
		{
			x.SetSelected(selected: false);
		});
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_ListSelectedItems);
		FactoryBase.ReleaseAndClearList(_ListMaterialItems);
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_ListMaterialItems);
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_ListPublicAwakenItemMaterialPair);
	}

	private void ClearStatChange()
	{
		if (!(_statListComponent == null))
		{
			FactoryBase.ReleaseComponent(_statListComponent);
			_statListComponent = null;
		}
	}

	private void ClearInvenItemList()
	{
		_DictAwakeInvenItems = null;
		_ListCanUseAwakeItems.Clear();
		InvenItemIndices.Clear();
	}

	private void ClearInvenItemIconList()
	{
		FactoryBase.ReleaseAndClearList(_ListAwakeInvenItems);
	}

	private void ClearPublicAwakenIconList()
	{
		FactoryBase.ReleaseAndClearList(_ListPublicAwakenItemComponents);
	}

	private void ClearHeroList()
	{
		_ListHeros.Clear();
	}

	private void ClearEquipItemIconList()
	{
		FactoryBase.ReleaseAndClearList(_ListAwakeEquipItems);
	}

	public void Refresh()
	{
		if (IsOpen())
		{
			SetupExpectedInfo();
		}
	}

	public int[] GetMaterialEquipSlotIndexArray()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_ListMaterialItems))
		{
			return null;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_ListMaterialItems); i++)
		{
			if (_ListMaterialItems[i].EquipItemInfo != null)
			{
				list.Add(_ListMaterialItems[i].EquipItemInfo.SlotIndex);
			}
		}
		return list.ToArray();
	}

	public int[] GetMaterialPublicAwakenItemIndexArray()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_ListMaterialItems))
		{
			return null;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = __Extension_Method_ICollection__.COUNT(_ListMaterialItems);
		for (int i = 0; i < num; i++)
		{
			ItemType type = _ListMaterialItems[i].ItemData.Type;
			if (type == ItemType.PublicTreasureAwaken || type == ItemType.PublicUniqueEquipAwaken)
			{
				int index = _ListMaterialItems[i].ItemData.Index;
				if (dictionary.ContainsKey(index))
				{
					dictionary[index]++;
				}
				else
				{
					dictionary.Add(index, 1);
				}
			}
		}
		int[] array = new int[__Extension_Method_ICollection__.COUNT(dictionary) * 2];
		int num2 = 0;
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			array[num2] = item.Key;
			num2++;
			array[num2] = item.Value;
			num2++;
		}
		return array;
	}

	private int GetCurrentReqGold()
	{
		if (TargetItem.EquipItemInfo == null)
		{
			return 0;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_ListMaterialItems))
		{
			return 0;
		}
		int detailIndex = TargetItem.EquipItemData.DetailIndex;
		int afterStar = TargetItem.AfterStar;
		short tier = TargetItem.EquipItemData.Tier;
		int reqGold = GameRoot.Instance.EquipAwakenPriceDataContainer.GetReqGold(detailIndex, afterStar, tier);
		int num = __Extension_Method_ICollection__.COUNT(_ListMaterialItems);
		return reqGold * num;
	}

	private long GetDiscountGold(long reqGold)
	{
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.DiscountAwakeEquip, null);
		int num = 0;
		long result = num;
		if (effectiveEvent != null)
		{
			int bonusPercentage = effectiveEvent.BonusPercentage;
			long num2 = checked(reqGold * bonusPercentage);
			int num3 = 100;
			result = num2 / num3;
		}
		return result;
	}

	private void SetupExpectedInfo()
	{
		if (TargetItem.EquipItemInfo == null)
		{
			return;
		}
		ClearStatChange();
		_statListComponent = StatListComponent.Create(StatChangePanel, 2);
		bool flag = _statListComponent.AddStatChange(TargetItem.BeforeStat, TargetItem.AfterStat, all: false, StatListComponent.EStatChangeTemplate.ForStarUpgrade, new StatListComponent.ColorInfo(StatFieldColor), new StatListComponent.ColorInfo(StatBeforeValueColor), new StatListComponent.ColorInfo(StatAfterColor), new StatListComponent.ColorInfo(StatAfterColor));
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(TargetItem.EquipItemInfo.ItemIndex);
		if (data != null)
		{
			int num = 1;
			while (true)
			{
				int fixedOptionIndex = data.GetSubData<EquipItemData>().GetFixedOptionIndex(num);
				if (fixedOptionIndex == 0)
				{
					break;
				}
				bool flag2 = _statListComponent.AddOptionChangeSkill(fixedOptionIndex, TargetItem.BeforeStar, TargetItem.AfterStar, StatListComponent.EOptionChangeTemplate.ForStarUpgrade, new StatListComponent.ColorInfo(StatBeforeValueColor), new StatListComponent.ColorInfo(StatAfterColor));
				flag = flag || flag2;
				num++;
			}
		}
		foreach (EquipItemOptionInfo optionInfo in TargetItem.EquipItemInfo.GetOptionInfoList())
		{
			_statListComponent.AddOptionChange(optionInfo, TargetItem.BeforeStar, TargetItem.AfterStar, all: false, StatListComponent.EStatChangeTemplate.ForStarUpgrade, new StatListComponent.ColorInfo(StatFieldColor), new StatListComponent.ColorInfo(StatBeforeValueColor), new StatListComponent.ColorInfo(StatAfterColor), new StatListComponent.ColorInfo(StatAfterColor));
		}
		if ((bool)StatChangeScrollView)
		{
			StatChangeScrollView.ResetPosition();
		}
	}

	public void SetTargetItem(EquipItemInfo itemInfo)
	{
		ClearIcon();
		ClearMaterialEquipIcon();
		if (itemInfo != null)
		{
			ItemIconComponent.Listener listener = this;
			ItemIconComponent itemIconComponent = _ListAwakeInvenItems.Find((ItemIconComponent comp) => comp.EquipItemInfo.SlotIndex == itemInfo.SlotIndex);
			if (itemIconComponent == null)
			{
				foreach (HeroEquipListComponent listAwakeEquipItem in _ListAwakeEquipItems)
				{
					ItemIconComponent itemIconComponent2 = listAwakeEquipItem.FindEquipItem(itemInfo.SlotIndex);
					if (itemIconComponent2 != null)
					{
						itemIconComponent = itemIconComponent2;
						listener = listAwakeEquipItem;
						break;
					}
				}
			}
			if (itemInfo.CLI_CanAwaken())
			{
				if (itemIconComponent != null)
				{
					itemIconComponent.Open(itemInfo, listener);
					SetupEquipIcon(itemIconComponent);
				}
				else
				{
					SetupEquipIcon(itemInfo);
				}
				SetupExpectedInfo();
			}
		}
		UpdateAwakeUITask();
		UpdateAwakeEquipItemListTask();
		UpdatePublicAwakeItemListTask();
	}

	public void SelectAwakenItem(EquipItemInfo itemInfo)
	{
		ItemIconComponent itemIconComponent = _ListAwakeInvenItems.Find((ItemIconComponent comp) => comp.EquipItemInfo.SlotIndex == itemInfo.SlotIndex);
		if (itemIconComponent != null && OnClickEquipItem(itemIconComponent))
		{
			SetupUIByForgeShortCut();
			return;
		}
		if (itemIconComponent == null)
		{
			foreach (HeroEquipListComponent listAwakeEquipItem in _ListAwakeEquipItems)
			{
				ItemIconComponent itemIconComponent2 = listAwakeEquipItem.FindEquipItem(itemInfo.SlotIndex);
				if (itemIconComponent2 != null)
				{
					itemIconComponent = itemIconComponent2;
					break;
				}
			}
		}
		if (itemIconComponent != null)
		{
			if (OnClickEquipItem(itemIconComponent))
			{
				SetupUIByForgeShortCut();
			}
		}
		else if ((itemInfo.IsUniqueWeaponWithoutClassWeapon && GlobalAutoBehaviour<AccountManager>.instance.CheckHavePublicUniqueEquipAwakenItem()) || (itemInfo.IsUniqueTreasure && GlobalAutoBehaviour<AccountManager>.instance.CheckHavePublicTreasureAwakenItem()))
		{
			SetupUIByForgeShortCut();
		}
	}

	public void SetupUIByForgeShortCut()
	{
		UpdateAwakeUITask();
		UpdateAwakeEquipItemListTask();
		UpdatePublicAwakeItemListTask();
		UIHelper.EnableGameObject(HeroEquipRoot, enable: false);
		SetupPublicAwakenItemList(ReloadList: false);
		SetupAwakenInvenItemList(ReloadList: false);
	}

	private void SetupEquipIcon(ItemIconComponent itemIconComponent)
	{
		EquipItemInfo equipItemInfo = itemIconComponent.EquipItemInfo;
		TargetItem.Create(equipItemInfo, TargetEquipItemRoot, ResultEquipItemRoot, this);
		UpdateTab();
	}

	private void SetupEquipIcon(EquipItemInfo equipItemInfo)
	{
		TargetItem.Create(equipItemInfo, TargetEquipItemRoot, ResultEquipItemRoot, this);
		UpdateTab();
	}

	private void SetupMaterialEquipIcon(ItemIconComponent itemIconComponent)
	{
		ItemData itemData = itemIconComponent.ItemData;
		EquipItemInfo equipItemInfo = itemIconComponent.EquipItemInfo;
		if (equipItemInfo != null && equipItemInfo.ItemIndex != 0)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfo.ItemIndex);
			if (data != null && GetTotalAwakenSuccessRate() < 100)
			{
				ItemIconComponent itemIconComponent2 = UIHelper.AddGridItem(MaterialEquipItemGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent2.Open(data, equipItemInfo, this);
				_ListMaterialItems.Add(itemIconComponent2);
				UIHelper.EnableGameObject(itemIconComponent2, enable: true);
				itemIconComponent.SetSelected(selected: true);
				_ListSelectedItems.Add(itemIconComponent);
			}
		}
		else if (itemData != null && GetTotalAwakenSuccessRate() < 100)
		{
			ItemIconComponent itemIconComponent3 = UIHelper.AddGridItem(MaterialEquipItemGrid, ItemIconComponent.PrefabAuto);
			itemIconComponent3.Open(itemData, this);
			itemIconComponent3.SetStar(itemData.AwakenStar);
			_ListMaterialItems.Add(itemIconComponent3);
			_ListPublicAwakenItemMaterialPair.Add(new ItemIconComponent[2] { itemIconComponent, itemIconComponent3 });
			UIHelper.EnableGameObject(itemIconComponent3, enable: true);
			itemIconComponent.SetSelected(selected: true);
			_ListSelectedItems.Add(itemIconComponent);
		}
	}

	private bool OnClickEquipItem(ItemIconComponent itemIconComponent)
	{
		if (itemIconComponent.EquipItemInfo == null)
		{
			return false;
		}
		if (IsEmptyTarget())
		{
			if (itemIconComponent.IsDisabled())
			{
				return false;
			}
			if (!itemIconComponent.EquipItemInfo.CLI_CanAwaken())
			{
				return false;
			}
			SetupEquipIcon(itemIconComponent);
			SetupExpectedInfo();
			return true;
		}
		if (TargetItem.IsEqualsItem(itemIconComponent))
		{
			ClearIcon();
			ClearMaterialEquipIcon();
			return true;
		}
		if (!itemIconComponent.EquipItemInfo.CLI_CanUse_as_AwakenMaterial() || itemIconComponent.EquipItemInfo.ItemIndex != TargetItem.EquipItemInfo.ItemIndex)
		{
			return false;
		}
		ItemIconComponent itemIconComponent2 = _ListMaterialItems.Find((ItemIconComponent x) => x.EquipItemInfo != null && (Vespa_Common_Class_Extension_Method.EQUALS(x, itemIconComponent) || x.EquipItemInfo.SlotIndex == itemIconComponent.EquipItemInfo.SlotIndex));
		ItemIconComponent itemIconComponent3 = _ListSelectedItems.Find((ItemIconComponent x) => x.EquipItemInfo != null && (Vespa_Common_Class_Extension_Method.EQUALS(x, itemIconComponent) || x.EquipItemInfo.SlotIndex == itemIconComponent.EquipItemInfo.SlotIndex));
		if (itemIconComponent2 != null && itemIconComponent3 != null)
		{
			itemIconComponent3.SetSelected(selected: false);
			_ListSelectedItems.Remove(itemIconComponent3);
			_ListMaterialItems.Remove(itemIconComponent2);
			FactoryBase.ReleaseComponent(itemIconComponent2);
			itemIconComponent2 = null;
			return true;
		}
		if (GetTotalAwakenSuccessRate() >= 100)
		{
			return false;
		}
		if (itemIconComponent.EquipItemInfo.Star >= 3)
		{
			if (GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<EquipConfirmPopup>() != null)
			{
				return false;
			}
			string message = LocalizationManager.GetString(EquipAwaken.EQUIP_AWAKEN_CONFIRM_USE_MATERIAL_MSG);
			string confirmLabel = LocalizationManager.GetString(NShared.NLocalization.Common.USE);
			string cancelLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CANCEL);
			EquipConfirmPopup.Open(message, confirmLabel, cancelLabel, itemIconComponent.EquipItemInfo, delegate
			{
				SetupMaterialEquipIcon(itemIconComponent);
				UpdateAwakeUITask();
				UpdateAwakeEquipItemListTask();
				UpdatePublicAwakeItemListTask();
			});
			return false;
		}
		SetupMaterialEquipIcon(itemIconComponent);
		return true;
	}

	private bool OnClickPublicAwakenItem(ItemIconComponent itemIconComponent)
	{
		if (itemIconComponent.EquipItemInfo != null)
		{
			return false;
		}
		if (IsEmptyTarget())
		{
			return false;
		}
		ItemIconComponent itemIconComponent2 = _ListMaterialItems.Find((ItemIconComponent x) => Vespa_Common_Class_Extension_Method.EQUALS(x, itemIconComponent));
		ItemIconComponent itemIconComponent3 = _ListSelectedItems.Find((ItemIconComponent x) => Vespa_Common_Class_Extension_Method.EQUALS(x, itemIconComponent));
		int index = -1;
		if (itemIconComponent2 != null || itemIconComponent3 != null)
		{
			if (itemIconComponent2 == null)
			{
				Tuple<int, ItemIconComponent> publicAwakenMaterialPair = GetPublicAwakenMaterialPair(itemIconComponent3);
				if (publicAwakenMaterialPair != null)
				{
					index = publicAwakenMaterialPair.Item1;
					itemIconComponent2 = publicAwakenMaterialPair.Item2;
				}
			}
			else if (itemIconComponent3 == null)
			{
				Tuple<int, ItemIconComponent> publicAwakenMaterialPair2 = GetPublicAwakenMaterialPair(itemIconComponent2);
				if (publicAwakenMaterialPair2 != null)
				{
					index = publicAwakenMaterialPair2.Item1;
					itemIconComponent3 = publicAwakenMaterialPair2.Item2;
				}
			}
		}
		if (itemIconComponent2 != null && itemIconComponent3 != null)
		{
			itemIconComponent3.SetSelected(selected: false);
			_ListSelectedItems.Remove(itemIconComponent3);
			_ListMaterialItems.Remove(itemIconComponent2);
			_ListPublicAwakenItemMaterialPair.RemoveAt(index);
			FactoryBase.ReleaseComponent(itemIconComponent2);
			itemIconComponent2 = null;
			return true;
		}
		if (GetTotalAwakenSuccessRate() >= 100)
		{
			return false;
		}
		switch (itemIconComponent.ItemData.Type)
		{
		case ItemType.PublicUniqueEquipAwaken:
			if (!TargetItem.EquipItemInfo.IsUniqueWeaponWithoutClassWeapon)
			{
				return false;
			}
			if (!CanUseAwakenStone(itemIconComponent))
			{
				return false;
			}
			break;
		case ItemType.PublicTreasureAwaken:
			if (!TargetItem.EquipItemInfo.IsUniqueTreasure)
			{
				return false;
			}
			if (!CanUseAwakenStone(itemIconComponent))
			{
				return false;
			}
			break;
		default:
			return false;
		}
		SetupMaterialEquipIcon(itemIconComponent);
		return true;
	}

	private Tuple<int, ItemIconComponent> GetPublicAwakenMaterialPair(ItemIconComponent itemIconComponent)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_ListPublicAwakenItemMaterialPair))
		{
			return null;
		}
		int num = __Extension_Method_ICollection__.COUNT(_ListPublicAwakenItemMaterialPair);
		for (int i = 0; i < num; i++)
		{
			if (Vespa_Common_Class_Extension_Method.EQUALS(_ListPublicAwakenItemMaterialPair[i][0], itemIconComponent))
			{
				return Tuple.Create(i, _ListPublicAwakenItemMaterialPair[i][1]);
			}
			if (Vespa_Common_Class_Extension_Method.EQUALS(_ListPublicAwakenItemMaterialPair[i][1], itemIconComponent))
			{
				return Tuple.Create(i, _ListPublicAwakenItemMaterialPair[i][0]);
			}
		}
		return null;
	}

	void HeroEquipListComponent.Listener.OnClick(HeroPortraitComponent component)
	{
	}

	void HeroEquipListComponent.Listener.OnClick(ItemIconComponent component)
	{
		if (OnClickEquipItem(component))
		{
			UpdateAwakeUITask();
			SetupPublicAwakenItemList(ReloadList: false);
			SetupAwakenInvenItemList(ReloadList: false);
		}
	}

	void HeroEquipListComponent.Listener.OnClickThis(int heroIndex)
	{
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (OnClickEquipItem(itemIconComponent) || OnClickPublicAwakenItem(itemIconComponent))
		{
			UpdateAwakeUITask();
			UpdateAwakeEquipItemListTask();
			UpdatePublicAwakeItemListTask();
		}
	}

	private void UpdateAwakeUITask()
	{
		bool flag = IsEmptyTarget();
		if (!flag && !TargetItem.EquipItemInfo.CLI_CanAwaken())
		{
			ClearIcon();
			UpdateAwakeUITask();
			SprkForgeAwakenChance.Sync(this);
			return;
		}
		bool flag2 = IsEmptyMaterial();
		if (flag && flag2)
		{
			UIHelper.SetLabelText(Label_UpperMsg, LocalizationManager.GetString(EquipAwaken.EQUIP_AWAKEN_MSG_SELECT_TARGET));
			UIHelper.EnableGameObject(WidgetChance, enable: false);
			ClearStatChange();
			UIHelper.SetLabelText(PriceLabel, "0");
			UIHelper.EnableGameObject(OriginalPriceLabel, enable: false);
			if (PriceLabelAligner != null)
			{
				PriceLabelAligner.Align();
			}
		}
		else
		{
			UIHelper.SetLabelText(Label_UpperMsg, LocalizationManager.GetString(EquipAwaken.EQUIP_AWAKEN_MSG_SELECT_MATERIAL));
			UIHelper.EnableGameObject(WidgetChance, enable: true);
			if (TargetItem.EquipItemInfo != null && !__Extension_Method_ICollection__.IsNullOrEmpty(_ListMaterialItems))
			{
				int totalMaterialBaseChance = GetTotalMaterialBaseChance();
				int targetEquipItemFailBonus = GetTargetEquipItemFailBonus();
				UIHelper.SetLabelText(LabelBaseChance, totalMaterialBaseChance + "%");
				UIHelper.SetLabelText(LabelAddedChance, "+" + targetEquipItemFailBonus + "%");
			}
			else
			{
				UIHelper.EnableGameObject(WidgetChance, enable: false);
			}
		}
		int currentReqGold = GetCurrentReqGold();
		long num = currentReqGold;
		long discountGold = GetDiscountGold(num);
		int num2 = 0;
		bool flag3 = (ulong)discountGold > (ulong)num2;
		long num3;
		if (!flag3)
		{
			num3 = num;
		}
		else
		{
			long val = checked(num - discountGold);
			int num4 = 0;
			num3 = System.Math.Max(val, num4);
		}
		long num5 = num3;
		bool flag4 = num5 <= GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold;
		UIHelper.EnableGameObject(OriginalPriceLabel, flag3);
		UIHelper.SetLabelText(OriginalPriceLabel, Vespa_Int_Extension_Method.ToCommaString(num));
		UIHelper.EnableButton(Button_Awaken, !flag && !flag2 && flag4);
		UIHelper.SetLabelText(PriceLabel, Vespa_Int_Extension_Method.ToCommaString(num5));
		UIHelper.SetLabelColor(PriceLabel, flag4 ? NormalGoldLabelColor : WarningGoldLabelColor);
		if (PriceLabelAligner != null)
		{
			PriceLabelAligner.Align();
		}
		SprkForgeAwakenChance.Sync(this);
	}

	private int GetTotalMaterialBaseChance()
	{
		if (TargetItem == null)
		{
			return 0;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_ListMaterialItems))
		{
			return 0;
		}
		int star = TargetItem.EquipItemInfo.Star + 1;
		int num = 0;
		int maxAwakenPoint = GameRoot.Instance.EquipAwakenRatioDataContainer.GetMaxAwakenPoint(star);
		int num2 = 0;
		foreach (ItemIconComponent listMaterialItem in _ListMaterialItems)
		{
			EquipItemInfo equipItemInfo = listMaterialItem.EquipItemInfo;
			EquipAwakenPointData equipAwakenPointData = null;
			equipAwakenPointData = ((equipItemInfo != null) ? GameRoot.Instance.EquipAwakenPointDataContainer.GetData(equipItemInfo.Star) : GameRoot.Instance.EquipAwakenPointDataContainer.GetData(listMaterialItem.ItemData.AwakenStar));
			int awakenPoint = equipAwakenPointData.AwakenPoint;
			num += awakenPoint;
		}
		if (num > maxAwakenPoint)
		{
			int num3 = num / maxAwakenPoint;
			int num4 = num % maxAwakenPoint;
			num2 += num3 * 100;
			if (num4 > 0)
			{
				EquipAwakenRatioData data = GameRoot.Instance.EquipAwakenRatioDataContainer.GetData(star, num4);
				if (data != null)
				{
					num2 += data.SuccessRatio;
				}
			}
			UIHelper.SetLabelColor(LabelBaseChance, OverSuccessRateColor);
		}
		else
		{
			num2 = GameRoot.Instance.EquipAwakenRatioDataContainer.GetData(star, num).SuccessRatio;
			UIHelper.SetLabelColor(LabelBaseChance, SuccessRateColor);
		}
		return num2;
	}

	private int GetTargetEquipItemFailBonus()
	{
		if (TargetItem == null)
		{
			return 0;
		}
		return TargetItem.EquipItemInfo.UpgradeStarFailBonus;
	}

	private int GetTotalAwakenSuccessRate()
	{
		return GetTotalMaterialBaseChance() + GetTargetEquipItemFailBonus();
	}

	private void SetSelectTextVisible(UILabel Label_Text, TweenAlpha tweenAlpha, bool Visible)
	{
		UIHelper.EnableGameObject(Label_Text, Visible);
		if (tweenAlpha != null)
		{
			tweenAlpha.enabled = Visible;
		}
	}

	private void UpdatePublicAwakeItemListTask()
	{
		bool num = IsEmptyTarget();
		bool flag = !num && TargetItem.EquipItemInfo.IsUniqueWeaponWithoutClassWeapon;
		bool flag2 = !num && TargetItem.EquipItemInfo.IsUniqueTreasure;
		int num2 = __Extension_Method_ICollection__.COUNT(_ListPublicAwakenItemComponents);
		if (!num)
		{
			if (!TargetItem.EquipItemInfo.CLI_CanAwaken())
			{
				ClearIcon();
				ClearMaterialEquipIcon();
				SprkCompactMaterialGrid();
				return;
			}
			int totalAwakenSuccessRate = GetTotalAwakenSuccessRate();
			for (int i = 0; i < num2; i++)
			{
				ItemIconComponent itemIconComponent = _ListPublicAwakenItemComponents[i];
				if (_ListSelectedItems.Contains(itemIconComponent))
				{
					SprkSetMaterialVisibility(itemIconComponent, gray: false);
					continue;
				}
				if (totalAwakenSuccessRate >= 100)
				{
					SprkSetMaterialVisibility(itemIconComponent, gray: true);
					continue;
				}
				switch (itemIconComponent.ItemData.Type)
				{
				case ItemType.PublicUniqueEquipAwaken:
				{
					bool flag4 = CanUseAwakenStone(itemIconComponent);
					bool gray2 = !flag || !flag4;
					SprkSetMaterialVisibility(itemIconComponent, gray2);
					break;
				}
				case ItemType.PublicTreasureAwaken:
				{
					bool flag3 = CanUseAwakenStone(itemIconComponent);
					bool gray = !flag2 || !flag3;
					SprkSetMaterialVisibility(itemIconComponent, gray);
					break;
				}
				default:
					SprkSetMaterialVisibility(itemIconComponent, gray: true);
					break;
				}
			}
		}
		else
		{
			for (int j = 0; j < num2; j++)
			{
				SprkSetMaterialVisibility(_ListPublicAwakenItemComponents[j], gray: true);
			}
		}
		CalcRepositionMaterialGrid();
		SprkCompactMaterialGrid();
	}

	private void UpdateAwakeEquipItemListTask()
	{
		bool num = IsEmptyTarget();
		IsEmptyMaterial();
		if (!num)
		{
			if (!TargetItem.EquipItemInfo.CLI_CanAwaken())
			{
				ClearIcon();
				ClearMaterialEquipIcon();
				SprkCompactMaterialGrid();
				return;
			}
			ItemIconComponent itemIconComponent = _ListAwakeInvenItems.Find((ItemIconComponent comp) => Vespa_Common_Class_Extension_Method.EQUALS(comp.EquipItemInfo, TargetItem.EquipItemInfo));
			if (itemIconComponent != null)
			{
				string text = LocalizationManager.GetString(EquipUpgrade.EQUIP_UPGRADE_PROGRESSING);
				itemIconComponent.SetCustomLabel(text, Color.yellow);
			}
			int totalAwakenSuccessRate = GetTotalAwakenSuccessRate();
			for (int num2 = 0; num2 < _ListAwakeInvenItems.Count; num2++)
			{
				ItemIconComponent itemIconComponent2 = _ListAwakeInvenItems[num2];
				if (itemIconComponent2.EquipItemInfo != null)
				{
					if (TargetItem.IsEmpty())
					{
						SprkSetMaterialVisibility(itemIconComponent2, gray: false);
					}
					else if (itemIconComponent2.EquipItemInfo.ItemIndex != TargetItem.EquipItemInfo.ItemIndex)
					{
						SprkSetMaterialVisibility(itemIconComponent2, gray: true);
					}
					else if (itemIconComponent2.EquipItemInfo.SlotIndex == TargetItem.EquipItemInfo.SlotIndex || _ListSelectedItems.Contains(itemIconComponent2))
					{
						SprkSetMaterialVisibility(itemIconComponent2, gray: false);
					}
					else if (totalAwakenSuccessRate < 100)
					{
						SprkSetMaterialVisibility(itemIconComponent2, !itemIconComponent2.EquipItemInfo.CLI_CanUse_as_AwakenMaterial());
					}
					else
					{
						SprkSetMaterialVisibility(itemIconComponent2, gray: true);
					}
				}
			}
			_ListAwakeInvenItems.ForEach(delegate(ItemIconComponent comp)
			{
				if (!comp.EquipItemInfo.CLI_CanAwaken() && comp.EquipItemInfo.CLI_CanUse_as_AwakenMaterial())
				{
					comp.SetCustomLabel(string.Empty, Color.white);
				}
			});
		}
		else
		{
			ItemIconComponent itemIconComponent3 = _ListAwakeInvenItems.Find((ItemIconComponent comp) => comp.IsSubComponentActive<CustomLabelSubComponent.ComponentProxy>());
			if (itemIconComponent3 != null)
			{
				itemIconComponent3.SetCustomLabel(string.Empty, Color.white);
			}
			_ListAwakeInvenItems.ForEach(delegate(ItemIconComponent comp)
			{
				comp.SetIconGrayScale(grayScale: false);
				comp.gameObject.SetActive(value: false);
				comp.gameObject.SetActive(value: true);
				if (!comp.EquipItemInfo.CLI_CanAwaken() && comp.EquipItemInfo.CLI_CanUse_as_AwakenMaterial())
				{
					string text2 = LocalizationManager.GetString(EquipAwaken.EQUIP_AWAKEN_AWAKEN_MAX);
					comp.SetCustomLabel(text2, Color.yellow);
				}
			});
		}
		UIHelper.EnableGameObject(Label_NoAwakenEquipItem, _ListAwakeInvenItems.Count == 0);
		CalcRepositionMaterialGrid();
		SprkCompactMaterialGrid();
	}

	private bool CanUseAwakenStone(ItemIconComponent comp)
	{
		AwakenStoneData subData = comp.ItemData.GetSubData<AwakenStoneData>();
		if (subData == null)
		{
			return false;
		}
		if (!subData.IsAllowNPC && !__Extension_Method_ICollection__.IsNullOrEmpty(TargetItem.EquipItemData.CreatureIndex))
		{
			int key = TargetItem.EquipItemData.CreatureIndex[0];
			if (GameRoot.Instance.HeroFriendlyDataContainer.GetData(key).NPC_Check == 1)
			{
				return false;
			}
		}
		if (__Extension_Method_ICollection__.COUNT(subData.ItemIndices) != 0)
		{
			return TypeIterator.Contains(subData.ItemIndices, TargetItem.EquipItemInfo.ItemIndex);
		}
		return true;
	}

	private bool IsEmptyTarget()
	{
		return TargetItem.IsEmpty();
	}

	private bool IsEmptyMaterial()
	{
		return __Extension_Method_ICollection__.IsNullOrEmpty(_ListMaterialItems);
	}

	public void OnClickReqLevelSort()
	{
		SortAwakeInvenItemList(ref _ReqLevelSortType, SortReqLevelSprites);
	}

	public void OnClickGradeSort()
	{
		SortAwakeInvenItemList(ref _GradeSortType, SortGradeSprites);
	}

	public void OnClickStarSort()
	{
		SortAwakeInvenItemList(ref _StarSortType, SortStarSprites);
	}

	public void OnClickHeroLevelSort()
	{
		SortAwakeEquipItemList(ref _HeroLevelSortType, SortHeroLevelSprites);
	}

	public void OnClickHeroGradeSort()
	{
		SortAwakeEquipItemList(ref _HeroGradeSortType, SortHeroGradeSprites);
	}

	private void SortAwakeInvenItemList(ref ESortType sortType, UpDownSpriteInfo SpriteInfo)
	{
		sortType++;
		if (sortType == ESortType.Max)
		{
			sortType = ESortType.Up;
		}
		SpriteInfo.SetSprite(sortType);
		new DelegateTask(delegate
		{
			SetupPublicAwakenItemList(ReloadList: false);
			SetupAwakenInvenItemList(ReloadList: false);
		}).Commit();
	}

	private void SortAwakeInvenItemList()
	{
		_ListCanUseAwakeItems.Clear();
		InvenItemIndices.Sort(delegate(int l, int r)
		{
			int num2 = 0;
			ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(l);
			ItemData<EquipItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(r);
			if (data == data2)
			{
				return 0;
			}
			num2 = ItemComparer.CompareRefrence(data, data2, null);
			if (num2 != 0 || (data == null && data2 == null))
			{
				return num2;
			}
			EquipItemData subData = data.GetSubData<EquipItemData>();
			EquipItemData subData2 = data2.GetSubData<EquipItemData>();
			num2 = ItemComparer.CompareRefrence(subData, subData2, null);
			if (num2 != 0 || (subData == null && subData2 == null))
			{
				return num2;
			}
			ItemComparer.SortClass sortClass = new ItemComparer.SortClass();
			sortClass.Add(ItemComparer.Compare, data.Tutorial, data2.Tutorial, isAscending: false).Add(ItemComparer.Compare, subData.ReqLevel, subData2.ReqLevel, _ReqLevelSortType == ESortType.Up).Add(ItemComparer.Compare_ItemGrade, data.Grade, data2.Grade, _GradeSortType == ESortType.Up)
				.Add(ItemComparer.Compare_WeaponType, subData.PartType, subData2.PartType, isAscending: true)
				.Add(ItemComparer.Compare, subData.SubType, subData2.SubType, isAscending: true)
				.Add(ItemComparer.Compare, l, r, isAscending: true);
			return sortClass.Sort();
		});
		if (TargetItem.EquipItemInfo != null && InvenItemIndices.Contains(TargetItem.EquipItemInfo.ItemIndex))
		{
			InvenItemIndices.Remove(TargetItem.EquipItemInfo.ItemIndex);
			InvenItemIndices.Insert(0, TargetItem.EquipItemInfo.ItemIndex);
		}
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(InvenItemIndices); num++)
		{
			int key = InvenItemIndices[num];
			List<EquipItemInfo> list = _DictAwakeInvenItems[key];
			list.Sort(delegate(EquipItemInfo l, EquipItemInfo r)
			{
				ItemComparer.SortClass sortClass = new ItemComparer.SortClass();
				sortClass.Add(ItemComparer.Compare, l.Star, r.Star, _StarSortType == ESortType.Up).Add(ItemComparer.Compare, l.Level, r.Level, isAscending: false).Add(ItemComparer.Compare_CreateTime, l.CreateDateTime, r.CreateDateTime, isAscending: true);
				return sortClass.Sort();
			});
			_ListCanUseAwakeItems.AddRange(list);
		}
	}

	public static int CompareValue<T>(ESortType sortType, T a, T b) where T : IComparable
	{
		return sortType switch
		{
			ESortType.Up => a.CompareTo(b), 
			ESortType.Down => b.CompareTo(a), 
			_ => 0, 
		};
	}

	private void SortAwakeEquipItemList(ref ESortType sortType, UpDownSpriteInfo SpriteInfo)
	{
		sortType++;
		if (sortType == ESortType.Max)
		{
			sortType = ESortType.Up;
		}
		SpriteInfo.SetSprite(sortType);
		SetupHeroEquipAwakeList(ReloadList: false);
	}

	private void SortAwakeEquipItemList()
	{
		_ListHeros.Sort(delegate(CachedHeroInfo l, CachedHeroInfo r)
		{
			int num = CompareValue(_HeroLevelSortType, l.Level, r.Level);
			if (num == 0)
			{
				num = CompareValue(_HeroGradeSortType, l.Star, r.Star);
			}
			if (num == 0)
			{
				num = CompareValue(_HeroGradeSortType, l.Transcended, r.Transcended);
			}
			if (num == 0)
			{
				num = CompareValue(ESortType.Up, l.HeroIndex, r.HeroIndex);
			}
			return num;
		});
	}

	public void RefreshItemList()
	{
		SetupPublicAwakenItemList();
		SetupAwakenInvenItemList();
		SetupHeroEquipAwakeList();
	}

	private void CalcRepositionMaterialGrid()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_ListMaterialItems))
		{
			float width = MaterialEquipItemGrid.ClippingPanel.width;
			int width2 = MaterialEquipItemGrid.PivotWidget.width;
			float num = MaterialGridMaxWidth * (float)__Extension_Method_ICollection__.COUNT(_ListMaterialItems);
			if (num > width)
			{
				float num2 = num - width;
				float cellWidth = (float)width2 - num2 / (float)(__Extension_Method_ICollection__.COUNT(_ListMaterialItems) - 1);
				MaterialEquipItemGrid.Grid.cellWidth = cellWidth;
			}
			else
			{
				MaterialEquipItemGrid.Grid.cellWidth = MaterialGridMaxWidth;
			}
			MaterialEquipItemGrid.RepositionGrid();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo equipStatInfo)
	{
	}

	public void OnClickClose()
	{
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	public void OnClickUpgrade()
	{
		if (IsEmptyMaterial() || IsEmptyTarget() || !TargetItem.EquipItemInfo.CLI_CanAwaken())
		{
			return;
		}
		if (GetTotalMaterialBaseChance() + GetTargetEquipItemFailBonus() > 100)
		{
			string stringDirect = LocalizationManager.GetStringDirect("EQUIP_AWAKE_RATIO_NOTICE");
			MessagePopup3Button.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), stringDirect, delegate
			{
				if (_listener != null)
				{
					_listener.OnAwaken(this);
				}
			}, null);
		}
		else if (_listener != null)
		{
			_listener.OnAwaken(this);
		}
	}

	private void UpdateTab()
	{
		if (TargetItem != null && TargetItem.EquipItemInfo != null)
		{
			HeroEquipToggle.Set(state: false);
			InventoryEquipToggle.Set(state: true);
			HeroEquipToggle.enabled = false;
			InventoryEquipToggle.enabled = false;
			UIHelper.SetWidgetGrayScale(HeroEquipWidget, grayScale: true);
			UIHelper.SetWidgetGrayScale(InventoryEquipWidget, grayScale: true);
		}
		else
		{
			HeroEquipToggle.Set(HeroEquipRoot.gameObject.activeSelf);
			InventoryEquipToggle.Set(InventoryEquipRoot.gameObject.activeSelf);
			HeroEquipToggle.enabled = true;
			InventoryEquipToggle.enabled = true;
			UIHelper.SetWidgetGrayScale(HeroEquipWidget, grayScale: false);
			UIHelper.SetWidgetGrayScale(InventoryEquipWidget, grayScale: false);
		}
	}

	public void OnEquipMenuToggleChanged()
	{
		UIHelper.EnableGameObject(HeroEquipRoot, (bool)HeroEquipToggle && HeroEquipToggle.value);
		UIHelper.EnableGameObject(InventoryEquipRoot, (bool)InventoryEquipToggle && InventoryEquipToggle.value);
	}

	StatData EquipAwakenScene.IDataProvider.GetBeforeStat()
	{
		return TargetItem.BeforeStat;
	}

	StatData EquipAwakenScene.IDataProvider.GetAfterStat()
	{
		return TargetItem.AfterStat;
	}

	int EquipAwakenScene.IDataProvider.GetBeforeStar()
	{
		return TargetItem.BeforeStar;
	}

	int EquipAwakenScene.IDataProvider.GetAfterStar()
	{
		return TargetItem.AfterStar;
	}

	EquipItemData EquipAwakenScene.IDataProvider.GetTargetEquipItemData()
	{
		return TargetItem.EquipItemData;
	}

	EquipItemInfo EquipAwakenScene.IDataProvider.GetTargetEquipItemInfo()
	{
		return TargetItem.EquipItemInfo;
	}

	Color EquipAwakenScene.IDataProvider.GetFieldColor()
	{
		return StatFieldColor;
	}

	Color EquipAwakenScene.IDataProvider.GetBeforeColor()
	{
		return StatBeforeValueColor;
	}

	Color EquipAwakenScene.IDataProvider.GetAfterColor()
	{
		return StatAfterColor;
	}

	private T GetComponet<T>(Transform tf) where T : Component
	{
		if (tf == null)
		{
			return null;
		}
		return tf.GetComponent<T>();
	}

	private int GridCustomCompareInven(Transform a, Transform b)
	{
		ItemIconComponent componet = GetComponet<ItemIconComponent>(a);
		if (componet == null)
		{
			return 1;
		}
		ItemIconComponent componet2 = GetComponet<ItemIconComponent>(b);
		if (componet2 == null)
		{
			return -1;
		}
		bool flag = componet.ItemData != null && componet.EquipItemInfo == null;
		bool flag2 = componet2.ItemData != null && componet2.EquipItemInfo == null;
		if (flag && flag2)
		{
			int num = 0;
			int value = 0;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_ListPublicAwakenItems); i++)
			{
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(_ListPublicAwakenItems[i].ItemIndex);
				if (componet.ItemData.Index == data.Index)
				{
					num = i;
				}
				if (componet2.ItemData.Index == data.Index)
				{
					value = i;
				}
			}
			return num.CompareTo(value);
		}
		if (flag)
		{
			return -1;
		}
		if (flag2)
		{
			return 1;
		}
		int num2 = _ListCanUseAwakeItems.IndexOf(componet.EquipItemInfo);
		int value2 = _ListCanUseAwakeItems.IndexOf(componet2.EquipItemInfo);
		return num2.CompareTo(value2);
	}

	private int GridCustomCompareHero(Transform a, Transform b)
	{
		HeroEquipListComponent componet = GetComponet<HeroEquipListComponent>(a);
		if (componet == null || componet.HeroInfo == null)
		{
			return 1;
		}
		HeroEquipListComponent componet2 = GetComponet<HeroEquipListComponent>(b);
		if (componet2 == null || componet2.HeroInfo == null)
		{
			return -1;
		}
		int num = _ListHeros.IndexOf(componet.HeroInfo);
		int value = _ListHeros.IndexOf(componet2.HeroInfo);
		return num.CompareTo(value);
	}

	public HeroEquipAwaken()
	{
		_ReqLevelSortType = ESortType.Down;
		_GradeSortType = ESortType.Down;
		_StarSortType = ESortType.Down;
		_HeroLevelSortType = ESortType.Down;
		_HeroGradeSortType = ESortType.Down;
	}

	private static void SprkSetMaterialVisibility(ItemIconComponent icon, bool gray)
	{
		icon.SetIconGrayScale(gray);
		icon.gameObject.SetActive(!gray);
	}

	private void SprkCompactMaterialGrid()
	{
		AwakeInvenItemListGrid.Grid.hideInactive = true;
		UIHelper.RepositionGrid(AwakeInvenItemListGrid);
		AwakeInvenItemListGrid.Grid.RequestRepositionNextFrame();
	}
}
