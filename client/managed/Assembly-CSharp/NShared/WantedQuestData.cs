using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class WantedQuestData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string CreatureCode { get; set; }

	[Key(2)]
	public int BattleChapterIndex { get; set; }

	[Key(3)]
	public int BattleDungeonIndex { get; set; }

	[Key(4)]
	public int RoyalPoint { get; set; }

	[Key(5)]
	public int Stamina { get; set; }

	[Key(6)]
	public float SpeedFactor { get; set; }

	[Key(7)]
	public int PathIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "WantedQuestData Index=" + Index;
	}
}
