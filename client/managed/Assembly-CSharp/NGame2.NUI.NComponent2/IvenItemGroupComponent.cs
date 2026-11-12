using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class IvenItemGroupComponent : LinkFactoryComponent, InitOnCreate
{
	public enum ItemGroupType
	{
		Fixed,
		Random,
		Added
	}

	public interface Listener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/InvenItemGroupComponent";

	public static PrefabAuto<IvenItemGroupComponent> PrefabAuto = new PrefabAuto<IvenItemGroupComponent>("Resources/Prefab2/Window/Inventory/InvenItemGroupComponent");

	public UIWidget Root;

	public UILabel Label_Name;

	public UILabel Label_Desc;

	public UISimpleGrid DropItemGrid;

	public UISprite Sprite_Grid;

	public UISprite[] Icons;

	private List<RandomBoxDropIconComponent> _listItemIconComponent = new List<RandomBoxDropIconComponent>();

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(_listItemIconComponent);
	}

	public void Open(ItemGroupType groupType, string desc, ref List<RewardElementData> _rewardElementList, int randomGroupIndex = -1, int dupFactor = 0)
	{
		SetItemGroupInfo(groupType, desc, ref _rewardElementList, randomGroupIndex, dupFactor);
	}

	public void SetItemGroupInfo(ItemGroupType groupType, string desc, ref List<RewardElementData> _rewardElementList, int randomGroupIndex, int dupFactor)
	{
		string empty = string.Empty;
		empty = groupType switch
		{
			ItemGroupType.Fixed => LocalizationManager.GetStringDirect("ITEMGROUPNAME_FIXED"), 
			ItemGroupType.Random => LocalizationManager.GetStringDirect("ITEMGROUPNAME_RANDOM", randomGroupIndex.ToString()), 
			ItemGroupType.Added => LocalizationManager.GetStringDirect("ITEMGROUPNAME_ADDED"), 
			_ => LocalizationManager.GetStringDirect("ITEMGROUPNAME_FIXED"), 
		};
		Label_Name.text = empty;
		Label_Desc.text = desc;
		for (int i = 0; i < Icons.Length; i++)
		{
			UIHelper.EnableGameObject(Icons[i], enable: false);
		}
		UIHelper.EnableGameObject(Icons[(int)groupType], enable: true);
		SetItemGroupIcons(ref _rewardElementList, randomGroupIndex);
		UIHelper.RepositionGrid(DropItemGrid);
		ResetSpriteHeight();
	}

	public void ResetSpriteHeight()
	{
		int num = _listItemIconComponent.Count / DropItemGrid.Grid.maxPerLine;
		int num2 = _listItemIconComponent.Count % DropItemGrid.Grid.maxPerLine;
		num++;
		if (num2 == 0)
		{
			num--;
		}
		float num3 = (float)num * DropItemGrid.Grid.cellHeight + 40f;
		Sprite_Grid.height = (int)num3;
	}

	public void SetItemGroupIcons(ref List<RewardElementData> _rewardElementList, int index)
	{
		foreach (RewardElementData _rewardElement in _rewardElementList)
		{
			RandomBoxDropIconComponent randomBoxDropIconComponent = DropItemGrid.AddGridItem(RandomBoxDropIconComponent.PrefabAuto);
			randomBoxDropIconComponent.Open(_rewardElement, disableFixed: true);
			_listItemIconComponent.Add(randomBoxDropIconComponent);
		}
	}
}
