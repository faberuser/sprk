namespace NShared;

[BattlePacket]
public class BattleBanPickEndNot
{
	public BanPickAccountInfo[] BanPickAccountInfo { get; set; }

	public MatchedAccountInfo[] AccountInfos { get; set; }
}
