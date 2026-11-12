namespace NShared;

[JsonSerializable(Javascript = true)]
public enum NewbieMissionConditionType
{
	None,
	CheckTeamLevel,
	CompleteDungeon,
	NewbieBuyItemBonusCount
}
