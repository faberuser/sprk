using System.Collections.Generic;

namespace NShared;

public static class AccessoryPartTypeHelper
{
	public static IEnumerable<AccessoryPartType> PartTypes()
	{
		for (AccessoryPartType partType = AccessoryPartType.Head; partType < AccessoryPartType.__MAX__; partType++)
		{
			yield return partType;
		}
	}
}
