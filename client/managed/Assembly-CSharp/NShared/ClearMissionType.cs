namespace NShared;

[JsonSerializable(Javascript = true)]
public enum ClearMissionType
{
	None,
	TeamLevelUp,
	StageClear,
	HeroMission,
	NpcMission
}
