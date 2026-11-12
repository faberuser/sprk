namespace NShared.B2G_SetRaidHp;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_set_raid_hp";

	public long MasterId { get; set; }

	public int RaidIndex { get; set; }

	public int RaidLevel { get; set; }

	public long MaxHp { get; set; }

	public long CurrentHp { get; set; }

	public bool BattleEnd { get; set; }

	public bool Win { get; set; }

	public long TotalDamageHp { get; set; }
}
