using NNguiExtension;
using NVespa.NPrefab2;

namespace NGame2.NBattleView.NUIOn3d2;

public class Damage : OnFieldDisplayBase
{
	public static PrefabAuto<Damage> PrefabAuto_Normal = new PrefabAuto<Damage>("Resources/Prefab2/UIOn3d2/DamageNormal", isGlobal: false, EPrefabType.Ngui, pooling: true, touchMaterial: true);

	public static PrefabAuto<Damage> PrefabAuto_Skill = new PrefabAuto<Damage>("Resources/Prefab2/UIOn3d2/DamageSkill", isGlobal: false, EPrefabType.Ngui, pooling: true, touchMaterial: true);

	public static PrefabAuto<Damage> PrefabAuto_Critical = new PrefabAuto<Damage>("Resources/Prefab2/UIOn3d2/DamageCritical", isGlobal: false, EPrefabType.Ngui, pooling: true, touchMaterial: true);

	public static PrefabAuto<Damage> PrefabAuto_Block = new PrefabAuto<Damage>("Resources/Prefab2/UIOn3d2/DamageBlock", isGlobal: false, EPrefabType.Ngui, pooling: true, touchMaterial: true);

	public static PrefabAuto<Damage> PrefabAuto_Heal = new PrefabAuto<Damage>("Resources/Prefab2/UIOn3d2/DamageHeal", isGlobal: false, EPrefabType.Ngui, pooling: true, touchMaterial: true);

	public UIStringSprite StringSprite;

	private static bool _visibleDamage = true;

	public bool Open(int creatureUid, long damage)
	{
		if (StringSprite != null)
		{
			if (!SprkArenaPractice.ShowDamage(_visibleDamage))
			{
				StringSprite.enabled = false;
			}
			else
			{
				StringSprite.printText = damage.ToString();
			}
		}
		return Open(creatureUid);
	}

	public static void SetVisibleDamage(bool visible)
	{
		_visibleDamage = visible;
	}
}
