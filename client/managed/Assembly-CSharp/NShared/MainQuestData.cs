using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class MainQuestData : IQuestData, IDataKeySelector<int>
{
	int IQuestData.QuestIndex => QuestIndex;

	int IQuestData.Step => Step;

	int IQuestData.ReqProgress => GetReqProgress();

	string IQuestData.Title => Title;

	string IQuestData.Desc => Desc;

	string IQuestData.Goal => Desc;

	int IQuestData.RewardIndex => RewardIndex;

	string IQuestData.MoveTaskType => MoveTaskType;

	string[] IQuestData.MoveTaskValue => MoveTaskValue;

	string[] IQuestData.Variable1 => Variable1;

	string[] IQuestData.Variable2 => Variable2;

	string[] IQuestData.Variable3 => Variable2;

	[Key(0)]
	public int QuestIndex { get; set; }

	[Key(1)]
	public int Step { get; set; }

	[Key(2)]
	public string Title { get; set; }

	[Key(3)]
	public string Desc { get; set; }

	[Key(4)]
	public string BackgroundPath { get; set; }

	[Key(5)]
	public string[] Variable1 { get; set; }

	[Key(6)]
	public string[] Variable2 { get; set; }

	[Key(7)]
	public QuestActionType StateType { get; set; }

	[Key(8)]
	public string[] StateValue { get; set; }

	[Key(9)]
	public bool ClientOnly { get; set; }

	[Key(10)]
	public int ReqProgress { get; set; }

	[Key(11)]
	public bool AutoComplete { get; set; }

	[Key(12)]
	public string MoveTaskType { get; set; }

	[Key(13)]
	public string[] MoveTaskValue { get; set; }

	[Key(14)]
	public int RewardIndex { get; set; }

	int IDataKeySelector<int>.Key => QuestIndex;

	public override string ToString()
	{
		return "MainQuestData Index=" + QuestIndex;
	}

	public int GetReqProgress()
	{
		if (ReqProgress > 0)
		{
			return ReqProgress;
		}
		return 1;
	}
}
