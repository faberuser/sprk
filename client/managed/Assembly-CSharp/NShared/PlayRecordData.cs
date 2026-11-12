using System.Collections.Generic;
using MessagePack;
using NShared.NUtil;

namespace NShared;

[MessagePackObject(false)]
public class PlayRecordData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public PlayRecordCategory Category { get; set; }

	[Key(2)]
	public string SubCategoryTitle { get; set; }

	[Key(3)]
	public string Title { get; set; }

	[Key(4)]
	public QuestActionType StateType { get; set; }

	[Key(5)]
	public int[] StateValue { get; set; }

	[Key(6)]
	public QuestActionType ActionType { get; set; }

	[Key(7)]
	public PlayRecordActionCalculateType ActionCalculateType { get; set; }

	[Key(8)]
	public string ActionValueAttribute { get; set; }

	[Key(9)]
	public string ConditionKey1 { get; set; }

	[Key(10)]
	public CheckType ConditionCompare1 { get; set; }

	[Key(11)]
	public string[] ConditionValue1 { get; set; }

	[Key(12)]
	public string ConditionKey2 { get; set; }

	[Key(13)]
	public CheckType ConditionCompare2 { get; set; }

	[Key(14)]
	public string[] ConditionValue2 { get; set; }

	[Key(15)]
	public string ConditionKey3 { get; set; }

	[Key(16)]
	public CheckType ConditionCompare3 { get; set; }

	[Key(17)]
	public string[] ConditionValue3 { get; set; }

	[Key(18)]
	public PlayRecordDisplayType DisplayType { get; set; }

	[Key(19)]
	public PlayRecordDisplayValueType DisplayValueType { get; set; }

	[Key(20)]
	public bool AppendComma { get; set; }

	[Key(21)]
	public string MoveTaskType { get; set; }

	[Key(22)]
	public string[] MoveTaskValue { get; set; }

	[Key(23)]
	public string ButtonLabelText { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public IEnumerable<PlayRecordCondition> GetConditions()
	{
		yield return new PlayRecordCondition
		{
			Key = ConditionKey1,
			Compare = ConditionCompare1,
			Value = ConditionValue1
		};
		yield return new PlayRecordCondition
		{
			Key = ConditionKey2,
			Compare = ConditionCompare2,
			Value = ConditionValue2
		};
		yield return new PlayRecordCondition
		{
			Key = ConditionKey3,
			Compare = ConditionCompare3,
			Value = ConditionValue3
		};
	}

	public override string ToString()
	{
		return "[Index=" + Index + " Title=" + Title + "]";
	}
}
