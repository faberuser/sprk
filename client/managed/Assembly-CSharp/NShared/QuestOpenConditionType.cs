namespace NShared;

[JsonSerializable(Javascript = true)]
public enum QuestOpenConditionType
{
	None,
	EventPeriodAble,
	EventPeriodShow,
	ClearQuestIndex,
	ClearQuestNDungeon,
	TakeAllReward,
	OpenKey,
	OpenKeyNTakeAllReward,
	EventBuffNTakeAllReward,
	OverKey
}
