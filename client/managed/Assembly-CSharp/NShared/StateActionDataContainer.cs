using System.Collections.Generic;
using MessagePack;
using NShared.NJit;

namespace NShared;

public class StateActionDataContainer : JitDataContainerListOnly<StateActionData>
{
	[MessagePackObject(false)]
	[JitKey]
	public struct ConditionKey
	{
		[Key(0)]
		public string State;

		[Key(1)]
		public StateConditionType ConditionType;

		public ConditionKey(string state, StateConditionType conditionType)
		{
			State = state;
			ConditionType = conditionType;
		}

		public bool Equals(ConditionKey other)
		{
			if (State == other.State)
			{
				return ConditionType == other.ConditionType;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return JitOption.BuildHash(State, ConditionType);
		}

		public override string ToString()
		{
			return $"{GetType().Name}({State}, {ConditionType})";
		}
	}

	public const string TABLENAME = "StateActionTable";

	private JitDictionary<ConditionKey, List<StateActionData>> _lists_by_StateAndCondition = new JitDictionary<ConditionKey, List<StateActionData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("StateActionTable");

	public StateActionDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_StateAndCondition);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (StateActionData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_StateAndCondition.JsonLoad_AddToListValue(new ConditionKey(item.StateType, item.ConditionType), item);
		}
	}

	public IList<StateActionData> GetDatas_by_StateAndCondition(string state, StateConditionType conditionType)
	{
		return _lists_by_StateAndCondition.GetOrDefault(new ConditionKey(state, conditionType));
	}
}
