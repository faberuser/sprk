using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class OpenTowerOfOrdeal : BaseNodeEvent
{
	private bool _maxStarHeroExist;

	public void SetMaxStarHeroExist(bool maxStarHeroExist)
	{
		_maxStarHeroExist = maxStarHeroExist;
	}

	public override bool Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		if (!base.Check(argList, out tableChapterIndex, out tableDungeonIndex))
		{
			return false;
		}
		return _maxStarHeroExist;
	}
}
