using System;

namespace NShared;

public class DungeonDifficultyTypeAttribute : Attribute
{
	public PartyDungeonDifficultyType PartyDungeonDifficultyType = PartyDungeonDifficultyType.Easy;
}
