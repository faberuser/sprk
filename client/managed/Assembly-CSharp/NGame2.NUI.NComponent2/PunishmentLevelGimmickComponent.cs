using NGame2.NUI.NHelper.NAffix;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class PunishmentLevelGimmickComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/Raid/PunishmentLevelSelectGimmickComponent";

	public static PrefabAuto<PunishmentLevelGimmickComponent> PrefabAuto = new PrefabAuto<PunishmentLevelGimmickComponent>("Resources/Prefab2/Component/Raid/PunishmentLevelSelectGimmickComponent");

	public UILabel DescLabel;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		UIHelper.SetLabelText(DescLabel, string.Empty);
	}

	public void Open(AffixData affixData, int level)
	{
		string affixDesc = AffixHelper.GetAffixDesc(affixData, level);
		UIHelper.SetLabelText(DescLabel, affixDesc);
	}
}
