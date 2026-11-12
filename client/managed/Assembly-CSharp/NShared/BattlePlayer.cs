namespace NShared;

public class BattlePlayer
{
	private BattlePlayerCreateInfo _playerCreateInfo;

	public long Id => _playerCreateInfo.Id;

	public TeamType TeamType => _playerCreateInfo.TeamType;

	public BattlePlayer(BattleInstance battleInstance, BattlePlayerCreateInfo playerCreateInfo)
	{
		_playerCreateInfo = playerCreateInfo;
	}
}
