using System.Collections.Generic;

namespace NGame2.NLogin.NMiniGame;

public class GameEventPool : IGameEventPool
{
	private Dictionary<string, List<IGameEvent>> _gameEventPool = new Dictionary<string, List<IGameEvent>>();

	public IGameEvent Pop(string typeName)
	{
		if (!_gameEventPool.ContainsKey(typeName))
		{
			return null;
		}
		List<IGameEvent> list = _gameEventPool[typeName];
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			return null;
		}
		IGameEvent result = list[0];
		list.RemoveAt(0);
		return result;
	}

	public void Push(IGameEvent gameEvent)
	{
		string name = gameEvent.GetType().Name;
		if (!_gameEventPool.ContainsKey(name))
		{
			_gameEventPool[name] = new List<IGameEvent>();
		}
		gameEvent.Reset();
		_gameEventPool[name].Add(gameEvent);
	}

	public void Clear()
	{
		Dictionary<string, List<IGameEvent>>.Enumerator enumerator = _gameEventPool.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.Clear();
		}
		_gameEventPool.Clear();
	}
}
