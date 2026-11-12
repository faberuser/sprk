using System;
using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class StateActionData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => 0;

	[Key(0)]
	public string StateType { get; set; }

	[Key(1)]
	public int[] ReqSkillIndex { get; set; }

	[Key(2)]
	public StateConditionType ConditionType { get; set; }

	[Key(3)]
	public int[] ConditionValue { get; set; }

	[Key(4)]
	public StateActionType ActionType1 { get; set; }

	[Key(5)]
	public string[] ActionValue1 { get; set; }

	[Key(6)]
	public StateActionType ActionType2 { get; set; }

	[Key(7)]
	public string[] ActionValue2 { get; set; }

	[Key(8)]
	public StateActionType ActionType3 { get; set; }

	[Key(9)]
	public string[] ActionValue3 { get; set; }

	public bool CheckConditionValue(int value)
	{
		if (ConditionValue == null || ConditionValue.Length == 0)
		{
			return true;
		}
		int num = ConditionValue[0];
		int val = ((ConditionValue.Length >= 2) ? ConditionValue[1] : num);
		int num2 = Math.Min(num, val);
		int num3 = Math.Max(num, val);
		if (value >= num2)
		{
			return value <= num3;
		}
		return false;
	}

	public List<Tuple<StateActionType, string[]>> GetActionList()
	{
		return new List<Tuple<StateActionType, string[]>>
		{
			new Tuple<StateActionType, string[]>(ActionType1, ActionValue1),
			new Tuple<StateActionType, string[]>(ActionType2, ActionValue2),
			new Tuple<StateActionType, string[]>(ActionType3, ActionValue3)
		};
	}
}
