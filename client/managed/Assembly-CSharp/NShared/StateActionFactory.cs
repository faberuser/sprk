using System.Collections.Generic;
using NShared.StateAction;

namespace NShared;

public class StateActionFactory
{
	private bool _initialized;

	private Dictionary<StateActionType, IStateAction> _actionDictionary;

	public void Init(bool forceInitialize = false)
	{
		if ((!_initialized || forceInitialize) && GameRoot.Instance != null && GameRoot.Instance.StateActionDataContainer != null && GameRoot.Instance.StateActionDataContainer.IsLoaded)
		{
			_initialized = true;
			InitAction();
		}
	}

	private void InitAction()
	{
		_actionDictionary = new Dictionary<StateActionType, IStateAction>();
		_actionDictionary.Add(StateActionType.SetState, new SetState());
		_actionDictionary.Add(StateActionType.ClearState, new ClearState());
		_actionDictionary.Add(StateActionType.UseSkill, new UseSkill());
		_actionDictionary.Add(StateActionType.GetDamage, new GetDamage());
	}

	public void Process(BattleCreature creature, IStateCondition condition, IStateModifier stateModifier)
	{
		Init();
		if (condition == null)
		{
			return;
		}
		string stateType = condition.StateType;
		_ = condition.ConditionType;
		IList<StateActionData> datas_by_StateAndCondition = GameRoot.Instance.StateActionDataContainer.GetDatas_by_StateAndCondition(condition.StateType, condition.ConditionType);
		if (datas_by_StateAndCondition == null || datas_by_StateAndCondition.Count == 0)
		{
			return;
		}
		foreach (StateActionData item3 in datas_by_StateAndCondition)
		{
			if (item3.ReqSkillIndex != null && item3.ReqSkillIndex.Length != 0)
			{
				bool flag = true;
				for (int i = 0; i < item3.ReqSkillIndex.Length; i++)
				{
					if (creature.GetUserSkillLevel(item3.ReqSkillIndex[i]) <= 0)
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			if (!condition.Check(item3))
			{
				continue;
			}
			foreach (Tuple<StateActionType, string[]> action in item3.GetActionList())
			{
				StateActionType item = action.Item1;
				string[] item2 = action.Item2;
				CreateAction(item)?.Run(creature, stateType, item2, stateModifier);
			}
		}
	}

	private IStateAction CreateAction(StateActionType actionType)
	{
		IStateAction value = null;
		_actionDictionary.TryGetValue(actionType, out value);
		return value;
	}
}
