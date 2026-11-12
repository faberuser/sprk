using NGameAsset;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class SkillTypeIconComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/Component/Icon/SkillTypeIconComponent";

	public static PrefabAuto<SkillTypeIconComponent> PrefabAuto = new PrefabAuto<SkillTypeIconComponent>("Prefab2/Component/Icon/SkillTypeIconComponent");

	public UITexture IconTexture;

	public void Open(SkillAttrType skillAttrType)
	{
		AssetTexture attrTypeIcon = UIHelper.GetAttrTypeIcon(skillAttrType.ToString());
		InitIconTexture(attrTypeIcon);
	}

	public void Open(string customName)
	{
		AssetTexture attrTypeIcon = UIHelper.GetAttrTypeIcon(customName);
		InitIconTexture(attrTypeIcon);
	}

	private void InitIconTexture(AssetTexture texture)
	{
		if ((bool)IconTexture)
		{
			UIHelper.ResetPlaneAssetTextureAsync(this, IconTexture, texture);
		}
	}
}
