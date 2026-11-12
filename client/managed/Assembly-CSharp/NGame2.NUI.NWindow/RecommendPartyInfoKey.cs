using NShared;

namespace NGame2.NUI.NWindow;

internal class RecommendPartyInfoKey : Tuple<int, int, DungeonDifficultyType>
{
	public RecommendPartyInfoKey(int i_item1, int i_item2, DungeonDifficultyType i_item3)
		: base(i_item1, i_item2, i_item3)
	{
	}
}
