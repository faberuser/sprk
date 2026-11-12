namespace NShared;

[BattlePacket]
public class BanPickAccountInfo
{
	public long AccountId { get; set; }

	public int[] PickHeroIndices { get; set; }

	public int[] BanHeroIndices { get; set; }
}
