using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class RewardItemListComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Resources/Prefab2/Component/Reward/RewardItemListComponent";

	public static PrefabAuto<RewardItemListComponent> PrefabAuto = new PrefabAuto<RewardItemListComponent>("Resources/Prefab2/Component/Reward/RewardItemListComponent");

	public UILabel Label_ItemTypeGradeDesc;

	public UISimpleGrid RewardItemListGrid;

	public UISprite Sprite_DescBack;

	private ItemType _itemType;

	private int _ItemGrade;

	private List<ItemIconComponent> _ListItemComponent = new List<ItemIconComponent>();

	public void Open(ItemType itemType, int itemGrade, List<ItemData> ListItemData)
	{
		_itemType = itemType;
		_ItemGrade = itemGrade;
		SetupItemTypeGradeDesc();
		SetupItemList(ListItemData);
	}

	public void Open(ItemType itemType, int itemGrade, List<Tuple<ItemData, int, int>> ListItemData)
	{
		_itemType = itemType;
		_ItemGrade = itemGrade;
		SetupItemTypeGradeDesc();
		SetupItemList(ListItemData);
	}

	public void Close()
	{
		ResetItemList();
	}

	public int GetComponentHeight()
	{
		int num = (_ListItemComponent.Count - 1) / RewardItemListGrid.Grid.maxPerLine + 1;
		return (int)Mathf.Abs(RewardItemListGrid.transform.localPosition.y) + num * (int)RewardItemListGrid.Grid.cellHeight;
	}

	private void ResetItemList()
	{
		FactoryBase.ReleaseAndClearList(_ListItemComponent);
	}

	private void SetupItemTypeGradeDesc()
	{
		UIHelper.SetLabelText(Label_ItemTypeGradeDesc, GetItemListDescText(_itemType, _ItemGrade));
	}

	private void SetupItemList(List<ItemData> ListItemData)
	{
		if (ListItemData == null)
		{
			return;
		}
		HashSet<int> dupMap = new HashSet<int>();
		ListItemData.ForEach(delegate(ItemData e)
		{
			if (!dupMap.Contains(e.Index))
			{
				dupMap.Add(e.Index);
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(RewardItemListGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(e);
				_ListItemComponent.Add(itemIconComponent);
			}
		});
		UIHelper.RepositionGrid(RewardItemListGrid);
	}

	private void SetupItemList(List<Tuple<ItemData, int, int>> ListItemData)
	{
		if (ListItemData == null)
		{
			return;
		}
		HashSet<int> dupMap = new HashSet<int>();
		ListItemData.ForEach(delegate(Tuple<ItemData, int, int> e)
		{
			ItemData item = e.Item1;
			_ = e.Item2;
			_ = e.Item3;
			if (!dupMap.Contains(item.Index))
			{
				dupMap.Add(item.Index);
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(RewardItemListGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(item);
				_ListItemComponent.Add(itemIconComponent);
			}
		});
		UIHelper.RepositionGrid(RewardItemListGrid);
	}

	public static string GetItemListDescText(ItemType itemType, int Grade)
	{
		_ = string.Empty;
		if (itemType == ItemType.Equip)
		{
			return LocalizationManager.GetStringDirect("EQUIPITEM_GRADE_" + Grade);
		}
		return LocalizationManager.GetString(itemType);
	}

	public void ShowBonusAssignedItem(List<Tuple<ItemData, int, int>> ListItemData, BattleType battleType)
	{
		for (int i = 0; i < _ListItemComponent.Count; i++)
		{
			ItemIconComponent itemIconComponent = _ListItemComponent[i];
			if (!(itemIconComponent == null))
			{
				ItemData itemData = itemIconComponent.ItemData;
				long itemCount = itemIconComponent.ItemCount;
				long itemMaxCount = itemIconComponent.ItemMaxCount;
				UIHelper.ShowBonusItemEventByMinMax(itemIconComponent, itemData.Index, itemCount, itemMaxCount, battleType);
			}
		}
	}
}
