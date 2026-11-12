namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class BattleBanPickInfoChanged
{
	public PlayerBattleBanPickInfo BattleBanPickInfo { get; set; }
}
