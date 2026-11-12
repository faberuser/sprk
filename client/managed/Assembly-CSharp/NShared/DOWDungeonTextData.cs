using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class DOWDungeonTextData : IDataKeySelector<DOWDungeonTextKey>
{
	[Key(0)]
	public int DungeonIndex { get; set; }

	[Key(1)]
	public int Diff { get; set; }

	[Key(2)]
	public string MainRewardItemCode { get; set; }

	[Key(3)]
	public string ClearTip { get; set; }

	[Key(4)]
	public string MonsterTip { get; set; }

	[Key(5)]
	public int[] RecommendHeroes { get; set; }

	DOWDungeonTextKey IDataKeySelector<DOWDungeonTextKey>.Key => new DOWDungeonTextKey(DungeonIndex, Diff);

	public override string ToString()
	{
		return "DOWDungeonTextData Index=" + DungeonIndex + " Diff=" + Diff;
	}
}
