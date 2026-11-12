using UnityEngine;

namespace NGame2.NUI.NGuildRaid;

public class GuildRaidStepColorInfo
{
	public Color GetStepColor(int step)
	{
		Color color = Color.white;
		switch (step)
		{
		case 1:
			ColorUtility.TryParseHtmlString("#e7e7e7", out color);
			break;
		case 2:
			ColorUtility.TryParseHtmlString("#a9e23a", out color);
			break;
		case 3:
			ColorUtility.TryParseHtmlString("#42a1f1", out color);
			break;
		case 4:
			ColorUtility.TryParseHtmlString("#e259ff", out color);
			break;
		case 5:
			ColorUtility.TryParseHtmlString("#ff6816", out color);
			break;
		default:
			ColorUtility.TryParseHtmlString("#ffe140", out color);
			break;
		}
		return color;
	}
}
