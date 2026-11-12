using System;
using System.Collections.Generic;

namespace NShared;

public class WinDeciderFactory
{
	private readonly Dictionary<WinDeciderType, Func<IWinDecider>> _creatorDictionary = new Dictionary<WinDeciderType, Func<IWinDecider>>();

	public WinDeciderFactory()
	{
		Register<DefeatAllEnemy>(WinDeciderType.DefeatAllEnemy);
		Register<MoreCreatureAlive>(WinDeciderType.MoreCreatureAlive);
		Register<LowerBossHp>(WinDeciderType.LowerBossHp);
		Register<Survival>(WinDeciderType.Survival);
		Register<Defense>(WinDeciderType.Defense);
	}

	private void Register<T>(WinDeciderType type) where T : IWinDecider, new()
	{
		_creatorDictionary.Remove(type);
		_creatorDictionary.Add(type, () => new T());
	}

	public IWinDecider Create(WinDeciderType type, string[] values)
	{
		Func<IWinDecider> value = null;
		_creatorDictionary.TryGetValue(type, out value);
		if (value != null)
		{
			IWinDecider winDecider = value();
			winDecider.Init(type, values);
			return winDecider;
		}
		return null;
	}
}
