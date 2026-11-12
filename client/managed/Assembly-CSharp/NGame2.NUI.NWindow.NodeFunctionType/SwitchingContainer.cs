using System.Collections.Generic;
using NGame2.NUI.NWindow.NodeFunctionType.Condition;
using NVespa;

namespace NGame2.NUI.NWindow.NodeFunctionType;

public class SwitchingContainer : SingletonAuto<SwitchingContainer>
{
	public Dictionary<string, ICondition> dic = new Dictionary<string, ICondition>();

	public SwitchingContainer()
	{
		Regist(new GuildExist());
		Regist(new ClearDungeon());
	}

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
