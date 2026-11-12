using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;

namespace NGame2.NUI.NWindow;

public class HeroSpecialEquipInfo : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroSpecialEquipInfo";

	private const int DefaultToggleGroup = 50;

	public List<UIToggle> ToggleList = new List<UIToggle>();

	public ArchiveItemInfo ArchiveItemInfo;

	public void Open(EquipItemInfo equipItemInfo)
	{
		Open();
		Init(equipItemInfo);
	}

	public void OpenInventoryMode(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			Open(equipItemInfo);
		}
	}

	private void Init(EquipItemInfo equipItemInfo)
	{
		SetItemInfo(equipItemInfo);
		SetToggleGroup(equipItemInfo.SlotIndex);
	}

	private void SetItemInfo(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null && equipItemInfo.EquipItemData != null)
		{
			ArchiveSubType subType = ArchiveSubType.None;
			if (equipItemInfo.EquipItemData.IsSpecialWeapon)
			{
				subType = ArchiveSubType.UniqueWeapon;
			}
			else if (equipItemInfo.EquipItemData.IsSpecialTreasure)
			{
				subType = ArchiveSubType.UniqueTreasure;
			}
			ArchiveItemInfo.Reset();
			ArchiveItemInfo.Init(subType, equipItemInfo.ItemData);
		}
	}

	private void SetToggleGroup(int equipItemInfoslotIndex)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(ToggleList))
		{
			return;
		}
		UIToggle uIToggle = null;
		for (int i = 0; i < ToggleList.Count; i++)
		{
			uIToggle = ToggleList[i];
			if (!(uIToggle == null))
			{
				uIToggle.group = 50 + equipItemInfoslotIndex;
			}
		}
	}
}
