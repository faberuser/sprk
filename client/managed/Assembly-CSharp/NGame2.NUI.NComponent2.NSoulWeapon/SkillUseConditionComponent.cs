using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NSoulWeapon;

public class SkillUseConditionComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/Component/SoulWeapon/SkillUseConditionComponent";

	public static PrefabAuto<SkillUseConditionComponent> PrefabAuto = new PrefabAuto<SkillUseConditionComponent>("Prefab2/Component/SoulWeapon/SkillUseConditionComponent");

	public UILabel UseConditionLabel;

	public void Open()
	{
	}
}
