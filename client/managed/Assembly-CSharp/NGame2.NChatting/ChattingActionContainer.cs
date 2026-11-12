using System.Collections.Generic;

namespace NGame2.NChatting;

public class ChattingActionContainer
{
	private readonly Dictionary<string, IChattingAction> _actionDictionary;

	public ChattingActionContainer()
	{
		_actionDictionary = new Dictionary<string, IChattingAction>();
	}

	public void Register(IChattingAction action)
	{
		string name = action.GetName();
		_actionDictionary.Remove(name);
		_actionDictionary.Add(name, action);
	}

	public IChattingAction GetAction(string name)
	{
		IChattingAction value = null;
		_actionDictionary.TryGetValue(name, out value);
		return value;
	}
}
