using NVespa.NPrefab2;

namespace NGame2.NBattleView.NUIOn3d2;

public class DamageConstant : OnFieldDisplayBase
{
	public static PrefabAuto<DamageConstant> Miss_PrefabAuto = new PrefabAuto<DamageConstant>("Resources/Prefab2/UIOn3d2/DamageMiss", isGlobal: false, EPrefabType.Ngui, pooling: true, touchMaterial: true);

	public static PrefabAuto<DamageConstant> ImmunePhysical_PrefabAuto = new PrefabAuto<DamageConstant>("Resources/Prefab2/UIOn3d2/DamageImmunePhysical", isGlobal: false, EPrefabType.Ngui, pooling: true, touchMaterial: true);

	public static PrefabAuto<DamageConstant> ImmuneMagical_PrefabAuto = new PrefabAuto<DamageConstant>("Resources/Prefab2/UIOn3d2/DamageImmuneMagical", isGlobal: false, EPrefabType.Ngui, pooling: true, touchMaterial: true);

	public new bool Open(int creatureUid)
	{
		return base.Open(creatureUid);
	}
}
