using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class BreakSoulWeaponMaterialInfo
{
	public UIScrollGrid MaterialListScrollGrid;

	private List<ItemIconComponent> _materialItemIconComponentList = new List<ItemIconComponent>();

	public void Open(SoulWeaponInfo soulWeaponInfo)
	{
		Init(soulWeaponInfo);
	}

	private void Init(SoulWeaponInfo soulWeaponInfo)
	{
		SetMaterial(soulWeaponInfo);
	}

	private void SetMaterial(SoulWeaponInfo soulWeaponInfo)
	{
		FactoryBase.ReleaseAndClearList(_materialItemIconComponentList);
		if (soulWeaponInfo == null)
		{
			return;
		}
		SoulBreakData soulBreakData = soulWeaponInfo.GetSoulBreakData();
		if (soulBreakData == null || __Extension_Method_ICollection__.IsNullOrEmpty(soulBreakData.ItemMinMaxList))
		{
			return;
		}
		for (int i = 0; i < soulBreakData.ItemMinMaxList.Count; i++)
		{
			SoulBreakData.ItemCodeMinMaxData itemCodeMinMaxData = soulBreakData.ItemMinMaxList[i];
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCodeMinMaxData.ItemCode);
			if (dataByCode != null)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(MaterialListScrollGrid, ItemIconComponent.PrefabAuto);
				if (!(itemIconComponent == null))
				{
					itemIconComponent.Open(dataByCode);
					int minItemCount = itemCodeMinMaxData.MinItemCount;
					int maxItemCount = itemCodeMinMaxData.MaxItemCount;
					int num = minItemCount;
					long num2;
					int num3;
					checked
					{
						num2 = unchecked((long)num) + unchecked((long)maxItemCount);
						num3 = 2;
					}
					itemIconComponent.SetItemCount(num2 / num3);
					_materialItemIconComponentList.Add(itemIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(MaterialListScrollGrid);
	}
}
