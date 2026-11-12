using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class RandomBoxItemListPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/RandomBoxItemListPopup";

	public UIScrollGrid DropItemGrid;

	public UITexture Texture_Item;

	private List<RandomBoxDropIconComponent> DropItemList = new List<RandomBoxDropIconComponent>();

	public static void Open(ItemData itemData, List<RewardElementData> rewardElementList)
	{
		WindowPrefab<RandomBoxItemListPopup> windowPrefab = new WindowPrefab<RandomBoxItemListPopup>("Resources/Prefab2/Window/MessagePopup/RandomBoxItemListPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(itemData, rewardElementList);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(DropItemList);
	}

	private void OpenInternal(ItemData itemData, List<RewardElementData> rewardElementList)
	{
		base.Open();
		if (itemData == null)
		{
			return;
		}
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Item, UIHelper.GetItemIcon2(itemData.Code));
		foreach (RewardElementData rewardElement in rewardElementList)
		{
			RandomBoxDropIconComponent randomBoxDropIconComponent = DropItemGrid.AddGridItem(RandomBoxDropIconComponent.PrefabAuto);
			randomBoxDropIconComponent.Open(rewardElement, disableFixed: true);
			DropItemList.Add(randomBoxDropIconComponent);
		}
		UIHelper.RepositionGrid(DropItemGrid);
	}
}
