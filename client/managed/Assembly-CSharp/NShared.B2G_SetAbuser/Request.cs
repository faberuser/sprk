namespace NShared.B2G_SetAbuser;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_set_abuser";

	public long AccountId { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int Difficulty { get; set; }

	public string BattleVersion { get; set; }

	public string BattleLogString { get; set; }

	public string CreatureInfoString { get; set; }

	public string VerifiedCreatureInfoString { get; set; }

	public string CheckResult { get; set; }

	public string AbusingCategory { get; set; }

	public int LeftValue { get; set; }

	public int RightValue { get; set; }
}
