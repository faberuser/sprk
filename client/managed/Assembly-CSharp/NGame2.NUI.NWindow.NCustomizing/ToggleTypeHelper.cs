using System.Collections.Generic;

namespace NGame2.NUI.NWindow.NCustomizing;

public static class ToggleTypeHelper
{
	public static IEnumerable<ToggleType> Types()
	{
		for (ToggleType toggleType = ToggleType.None; toggleType < ToggleType._MAX_; toggleType++)
		{
			yield return toggleType;
		}
	}
}
