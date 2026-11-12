using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class EclipseDungeonRewardPopup : BaseWindow
{
	public static string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/EclipseDungeonRewardPopup";

	public UIScrollGrid RewardScrollGrid;

	private List<ItemIconComponent> _rewardItemIconComponentList = new List<ItemIconComponent>();

	public new void Open()
	{
		base.Open();
		Init();
	}

	private void Init()
	{
		SetRewardList();
	}

	private void SetRewardList()
	{
		FactoryBase.ReleaseAndClearList(_rewardItemIconComponentList);
		IList<SoulWeaponEtherData> allData = GameRoot.Instance.SoulWeaponEtherDataContainer.GetAllData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			return;
		}
		for (int i = 0; i < allData.Count; i++)
		{
			SoulWeaponEtherData soulWeaponEtherData = allData[i];
			if (soulWeaponEtherData == null)
			{
				continue;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(soulWeaponEtherData.ItemCode);
			if (dataByCode != null)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(RewardScrollGrid, ItemIconComponent.PrefabAuto);
				if (!(itemIconComponent == null))
				{
					itemIconComponent.Open(dataByCode);
					_rewardItemIconComponentList.Add(itemIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(RewardScrollGrid);
	}
}
