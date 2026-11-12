using System.Collections.Generic;

public static class SprkLightBattlePortrait
{
	private static readonly HashSet<int> fallbackCostumes = new HashSet<int>
	{
		580, 680, 780, 880, 980, 1080, 1180, 1280, 1380, 1680,
		1780, 1880, 1980, 2080, 2180, 2280, 2380, 2480, 2680, 2780,
		2880, 2980, 3180, 3280, 3380, 3580, 3680, 3780, 3880, 3980,
		4080, 4180, 4280, 4380, 4480, 4680, 4880, 4980, 5080, 5280,
		5780, 5880, 5980, 6080, 6180, 6380, 6580, 6780, 6880, 6980,
		7080, 7180, 7280, 7480, 7580, 7680, 7780, 7880, 7980, 8080,
		8180, 8280, 8780, 8880, 8980, 9180, 9280, 9380, 9480, 9880,
		10080, 10180, 10280
	};

	public static string Resolve(string heroCode, int star, int costumeIndex)
	{
		if (!fallbackCostumes.Contains(costumeIndex) || string.IsNullOrEmpty(heroCode))
		{
			return null;
		}
		return "Battle/Portrait/" + heroCode.Replace('.', '/') + "_5";
	}
}
