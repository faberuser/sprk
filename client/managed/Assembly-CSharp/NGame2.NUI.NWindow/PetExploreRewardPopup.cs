using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class PetExploreRewardPopup : BaseWindow
{
	public const string PrefabPath = "Prefab2/Window/Pet/Popup/PetExploreRewardInfoPopup";

	public UIScrollGrid ScrollGrid;

	private List<ItemIconComponent> _listComponent = new List<ItemIconComponent>();

	public static void Open(int itemIndex)
	{
		WindowPrefab<PetExploreRewardPopup> windowPrefab = new WindowPrefab<PetExploreRewardPopup>("Prefab2/Window/Pet/Popup/PetExploreRewardInfoPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(itemIndex);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal(int itemIndex)
	{
		Open(delegate
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
			if (data != null)
			{
				CreateItemIconComponent(data.Code, 1);
				UIHelper.RepositionGrid(ScrollGrid);
			}
		}, null);
	}

	private void CreateItemIconComponent(string itemCode, int depth, int itemMinCount = 1, int itemMaxCount = 1)
	{
		if (depth > 4 || String_Extension_Method.IsNullOrEmpty(itemCode))
		{
			return;
		}
		PackageItemData data = GameRoot.Instance.PackageItemDataContainer.GetData(itemCode);
		if (data == null)
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
			if (dataByCode == null)
			{
				return;
			}
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ScrollGrid, ItemIconComponent.PrefabAuto);
			itemIconComponent.Open(dataByCode);
			if (itemIconComponent != null)
			{
				if (itemMinCount != itemMaxCount)
				{
					itemIconComponent.SetItemMinMax(itemMinCount, itemMaxCount);
				}
				else if (itemMaxCount > 1)
				{
					itemIconComponent.SetItemCount(itemMaxCount);
				}
				_listComponent.Add(itemIconComponent);
			}
			return;
		}
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(data.RewardIndex1);
		if (data2 == null)
		{
			return;
		}
		List<RewardElementData> rewardItemList = data2.GetRewardItemList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardItemList); i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			if (rewardElementData != null)
			{
				CreateItemIconComponent(rewardElementData.ItemCode, depth + 1, rewardElementData.ItemMin, rewardElementData.ItemMax);
			}
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ReleaseItemIconComponent();
	}

	private void ReleaseItemIconComponent()
	{
		FactoryBase.ReleaseAndClearList(_listComponent);
	}
}
