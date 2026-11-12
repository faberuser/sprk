namespace NShared.B2G_ChangeMaster;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_change_master";

	public long AccountId { get; set; }

	public bool IsMaster { get; set; }

	public long MasterAccountId { get; set; }

	public long OldMasterAccountId { get; set; }

	public string MasterHost { get; set; }
}
