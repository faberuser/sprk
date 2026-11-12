using System;
using System.Collections.Generic;
using NGame2.NUI.NWindow.NEntryMenu.NCondition;
using NVespa;

namespace NGame2.NUI.NWindow.NEntryMenu;

public class ConditionContainer : SingletonAuto<ConditionContainer>
{
	private readonly Dictionary<string, ICondition> _conditionDictionary = new Dictionary<string, ICondition>();

	public ConditionContainer()
	{
		Register(new GuildExist());
		Register(new HasMaxStarHero());
		Register(new AnyRaidMinClearLevel());
		Register(new DevOnly());
		Register(new EventExist());
		Register(new EventNotExist());
		Register(new HasHero());
		Register(new GuildActivityRestrict());
		Register(new ClearDungeon());
		Register(new OpenContents());
		Register(new IsOpenGatewayToTrial());
		Register(new CompleteTutorial());
	}

	public void Register<T>(T condition) where T : ICondition
	{
		string name = condition.GetType().Name;
		_conditionDictionary.Remove(name);
		_conditionDictionary.Add(name, condition);
	}

	public ICondition GetCondition(string name)
	{
		_conditionDictionary.TryGetValue(name, out var value);
		return value;
	}

	public ICondition GetCondition(Type type)
	{
		string name = type.Name;
		return GetCondition(name);
	}
}
