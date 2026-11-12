using System.Collections.Generic;
using NGame2.NUI;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NPrefab2;

namespace NGame2.NBattleView.NUIOn3D;

public class LootItem : OnFieldDisplayBase
{
	public static PrefabAuto<LootItem> PrefabAuto = new PrefabAuto<LootItem>("Resources/Prefab2/UIOn3D/LootItem");

	public UITexture ItemIcon;

	public List<UISprite> GradeSprites = new List<UISprite>();

	public void SetItem(ItemData itemData)
	{
		AssetTexture itemIcon = UIHelper.GetItemIcon2(itemData.Code);
		UIHelper.ResetPlaneAssetTextureAsync(this, ItemIcon, itemIcon);
		int num = Math.ClampOver(itemData.Grade, 0, GradeSprites.Count);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(GradeSprites); i++)
		{
			UIHelper.EnableGameObject(GradeSprites[i], i == num);
		}
	}
}
