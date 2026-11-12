using NShared.NUtil;

namespace NShared;

public class BattlePlayerContainer
{
	private readonly BattleInstance _battleInstance;

	private readonly IndexableDictionay<long, BattlePlayer> _playerDictionary;

	public BattlePlayerContainer(BattleInstance battleInstance)
	{
		_battleInstance = battleInstance;
		_playerDictionary = new IndexableDictionay<long, BattlePlayer>(32);
	}

	public BattlePlayer Create(BattlePlayerCreateInfo playerCreateInfo)
	{
		if (playerCreateInfo == null)
		{
			return null;
		}
		long id = playerCreateInfo.Id;
		BattlePlayer battlePlayer = new BattlePlayer(_battleInstance, playerCreateInfo);
		if (_playerDictionary.TryAdd(id, battlePlayer))
		{
			return battlePlayer;
		}
		return null;
	}

	public BattlePlayer GetPlayer(long playerId)
	{
		if (!_playerDictionary.TryGetValue(playerId, out var value))
		{
			return null;
		}
		return value;
	}

	public BattlePlayer GetPlayerByTeam(TeamType teamType)
	{
		for (int i = 0; i < _playerDictionary.Count; i++)
		{
			BattlePlayer valueByIndex = _playerDictionary.GetValueByIndex(i);
			if (valueByIndex.TeamType == teamType)
			{
				return valueByIndex;
			}
		}
		return null;
	}

	public long[] GetAllPlayerIds()
	{
		long[] array = new long[_playerDictionary.Count];
		for (int i = 0; i < _playerDictionary.Count; i++)
		{
			array[i] = _playerDictionary.GetKeyByIndex(i);
		}
		return array;
	}
}
