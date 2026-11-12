namespace NShared.B2G_BattleStart;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_battle_start";

	public long AccountId { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int DungeonDifficulty { get; set; }

	public int RaidIndex { get; set; }

	public int RaidLevel { get; set; }

	public int[] AffixIndices { get; set; }
}
