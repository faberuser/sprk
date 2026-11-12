namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class BattleInfoChanged
{
	public PlayerBattleInfo BattleInfo { get; set; }
}
