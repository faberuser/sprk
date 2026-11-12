namespace NShared;

public enum PartyDungeonDifficultyType
{
	None,
	[PartyDungeonDifficultyType(DungeonDifficultyType = DungeonDifficultyType.Easy)]
	Easy,
	[PartyDungeonDifficultyType(DungeonDifficultyType = DungeonDifficultyType.Normal)]
	Normal,
	[PartyDungeonDifficultyType(DungeonDifficultyType = DungeonDifficultyType.Hard)]
	Hard,
	[PartyDungeonDifficultyType(DungeonDifficultyType = DungeonDifficultyType.Hell)]
	Hell
}
