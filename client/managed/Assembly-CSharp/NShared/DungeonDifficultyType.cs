namespace NShared;

[JsonSerializable(Javascript = true)]
public enum DungeonDifficultyType
{
	[DungeonDifficultyType(PartyDungeonDifficultyType = PartyDungeonDifficultyType.Easy)]
	Easy,
	[DungeonDifficultyType(PartyDungeonDifficultyType = PartyDungeonDifficultyType.Normal)]
	Normal,
	[DungeonDifficultyType(PartyDungeonDifficultyType = PartyDungeonDifficultyType.Hard)]
	Hard,
	[DungeonDifficultyType(PartyDungeonDifficultyType = PartyDungeonDifficultyType.Hell)]
	Hell
}
