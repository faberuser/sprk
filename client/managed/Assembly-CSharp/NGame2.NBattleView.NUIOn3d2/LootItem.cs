using System.Collections.Generic;
using NGame2.NUI;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NPrefab2;

namespace NGame2.NBattleView.NUIOn3d2;

public class LootItem : OnFieldDisplayBase
{
	public static PrefabAuto<LootItem> PrefabAuto = new PrefabAuto<LootItem>("Resources/Prefab2/UIOn3d2/LootItem", isGlobal: false, EPrefabType.Ngui, pooling: true, touchMaterial: true);

	public UITexture IconTexture;

	public List<UISprite> GradeList = new List<UISprite>();

	public bool Open(int creatureUid, ItemData itemData)
	{
		AssetTexture itemIcon = UIHelper.GetItemIcon2(itemData.Code);
		UIHelper.ResetPlaneAssetTextureAsync(this, IconTexture, itemIcon);
		int num = Math.ClampOver(itemData.Grade, 0, GradeList.Count);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(GradeList); i++)
		{
			UIHelper.EnableGameObject(GradeList[i], i == num);
		}
		return Open(creatureUid);
	}
}
