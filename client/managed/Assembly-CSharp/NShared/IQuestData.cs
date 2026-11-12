namespace NShared;

public interface IQuestData
{
	int QuestIndex { get; }

	int Step { get; }

	int ReqProgress { get; }

	string Title { get; }

	string Desc { get; }

	string Goal { get; }

	int RewardIndex { get; }

	string MoveTaskType { get; }

	string[] MoveTaskValue { get; }

	string[] Variable1 { get; }

	string[] Variable2 { get; }

	string[] Variable3 { get; }
}
