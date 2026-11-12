using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class SubQuestData : IDataKeySelector<SubQuestDataKey>, IQuestData
{
	SubQuestDataKey IDataKeySelector<SubQuestDataKey>.Key => new SubQuestDataKey(QuestIndex, Step);

	int IQuestData.QuestIndex => QuestIndex;

	int IQuestData.Step => Step;

	int IQuestData.ReqProgress => GetReqProgress();

	string IQuestData.Title => Title;

	string IQuestData.Desc => Desc;

	string IQuestData.Goal => Goal;

	int IQuestData.RewardIndex => RewardIndex;

	string IQuestData.MoveTaskType => MoveTaskType.ToString();

	string[] IQuestData.MoveTaskValue => MoveTaskValue;

	string[] IQuestData.Variable1 => Variable1;

	string[] IQuestData.Variable2 => Variable2;

	string[] IQuestData.Variable3 => Variable3;

	[Key(0)]
	public int QuestIndex { get; set; }

	[Key(1)]
	public int Group { get; set; }

	[Key(2)]
	public int Step { get; set; }

	[Key(3)]
	public int OldIndex { get; set; }

	[Key(4)]
	public string Title { get; set; }

	[Key(5)]
	public string Desc { get; set; }

	[Key(6)]
	public string Goal { get; set; }

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
	public ClientActivityType MoveTaskType { get; set; }

	[Key(13)]
	public string[] MoveTaskValue { get; set; }

	[Key(14)]
	public string[] Variable1 { get; set; }

	[Key(15)]
	public string[] Variable2 { get; set; }

	[Key(16)]
	public string[] Variable3 { get; set; }

	[Key(17)]
	public int RewardIndex { get; set; }

	[Key(18)]
	public int TutorialIndex { get; set; }

	[Key(19)]
	public string OpenConditionLocalKey { get; set; }

	[Key(20)]
	public int VisualRewardIndex { get; set; }

	public SubQuestData GetNextData()
	{
		return GameRoot.Instance.SubQuestDataContainer.GetData(new SubQuestDataKey(QuestIndex, Step + 1));
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
