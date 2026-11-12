using System.Collections.Generic;

namespace NGame2.NGuild;

public class PenaltyChecker
{
	private Dictionary<GuildPenaltyType, IGuildPenalty> _dicsPenalty;

	public PenaltyChecker()
	{
		if (_dicsPenalty == null)
		{
			_dicsPenalty = new Dictionary<GuildPenaltyType, IGuildPenalty>();
			RegistPenalty<GuildContentsRestrict>();
			RegistPenalty<GuildWithdraw>();
		}
	}

	private void RegistPenalty<T>() where T : IGuildPenalty, new()
	{
		T val = new T();
		_dicsPenalty.Add(val.GetPenaltyType(), val);
	}

	public IGuildPenalty GetPenalty(GuildPenaltyType type)
	{
		if (!_dicsPenalty.ContainsKey(type))
		{
			return null;
		}
		return _dicsPenalty[type];
	}

	public IGuildPenalty GetValidPenalty()
	{
		Dictionary<GuildPenaltyType, IGuildPenalty>.Enumerator enumerator = _dicsPenalty.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.OnPenalty())
			{
				return enumerator.Current.Value;
			}
		}
		return null;
	}
}
