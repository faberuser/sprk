using NGame2.NUI.NHelper.NAffix;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class PunishmentLevelAbilityComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/Raid/PunishmentLevelSelectAbilityComponent";

	public static PrefabAuto<PunishmentLevelAbilityComponent> PrefabAuto = new PrefabAuto<PunishmentLevelAbilityComponent>("Resources/Prefab2/Component/Raid/PunishmentLevelSelectAbilityComponent");

	public UIBundleLoader IconBundleLoader;

	public UILabel DescLabel;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(AffixData affixData, int level)
	{
		string affixDesc = AffixHelper.GetAffixDesc(affixData, level);
		UIHelper.SetLabelText(DescLabel, affixDesc);
		UIHelper.SetBundleTexture(IconBundleLoader, affixData.IconPath);
	}
}
