namespace NShared;

[JsonSerializable(Javascript = true)]
public class PartyDungeonInfo : IAccountAttrKey<PartyDungeonKey>
{
	PartyDungeonKey IAccountAttrKey<PartyDungeonKey>.Key => new PartyDungeonKey(ChapterIndex, DungeonIndex);

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int DungeonDifficulty { get; set; }

	public int BattleScore { get; set; }

	public string CreatedTime { get; set; }

	public string CompletedTime { get; set; }
}
