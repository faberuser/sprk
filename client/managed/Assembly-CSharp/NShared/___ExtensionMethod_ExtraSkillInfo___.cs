using System.Collections.Generic;

namespace NShared;

public static class ___ExtensionMethod_ExtraSkillInfo___
{
	public static void AddRangeFromIndex(this List<ExtraSkillInfo> This, int[] skillIndices)
	{
		if (skillIndices != null)
		{
			for (int i = 0; i < skillIndices.Length; i++)
			{
				This.Add(new ExtraSkillInfo(skillIndices[i]));
			}
		}
	}
}
