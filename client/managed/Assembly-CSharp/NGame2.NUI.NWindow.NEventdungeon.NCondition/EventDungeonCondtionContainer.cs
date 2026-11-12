using System.Collections.Generic;
using NVespa;

namespace NGame2.NUI.NWindow.NEventdungeon.NCondition;

public class EventDungeonCondtionContainer : SingletonAuto<EventDungeonCondtionContainer>
{
	private readonly Dictionary<string, IEventDungeonCondition> _conditionDictionary = new Dictionary<string, IEventDungeonCondition>();

	public EventDungeonCondtionContainer()
	{
		Register(new OpenCheckTime());
		Register(new OpenPrevDungeonClear());
		Register(new OpenOwnedHero());
		Register(new OpenChapterDungeonClear());
	}

	public void Register<T>(T condition) where T : IEventDungeonCondition
	{
		string name = condition.GetType().Name;
		_conditionDictionary.Remove(name);
		_conditionDictionary.Add(name, condition);
	}

	public IEventDungeonCondition GetCondition(string name)
	{
		if (String_Extension_Method.IsNullOrEmpty(name))
		{
			return null;
		}
		_conditionDictionary.TryGetValue(name, out var value);
		return value;
	}
}
