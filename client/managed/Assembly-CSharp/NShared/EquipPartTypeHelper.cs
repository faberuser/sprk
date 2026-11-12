using System.Collections.Generic;

namespace NShared;

public static class EquipPartTypeHelper
{
	public static IEnumerable<EquipPartType> EquippableTypes()
	{
		for (EquipPartType partType = EquipPartType.Weapon; partType < EquipPartType.__MAX__; partType++)
		{
			yield return partType;
		}
	}
}
