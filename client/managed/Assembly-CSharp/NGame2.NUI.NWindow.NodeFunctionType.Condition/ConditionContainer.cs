using System.Collections.Generic;
using NVespa;

namespace NGame2.NUI.NWindow.NodeFunctionType.Condition;

public class ConditionContainer : SingletonAuto<ConditionContainer>
{
	public Dictionary<string, ICondition> dic = new Dictionary<string, ICondition>();

	public void Regist<T>(T iCondition) where T : ICondition
	{
		string name = iCondition.GetType().Name;
		dic.Remove(name);
		dic.Add(name, iCondition);
	}

	public ICondition GetCondition(string key)
	{
		dic.TryGetValue(key, out var value);
		return value;
	}
}
