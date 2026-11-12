namespace NShared.B2M_Ping;

[JsonSerializable(Javascript = true)]
public class IntegrityInfo
{
	public string Host { get; set; }

	public BattleIntegrityCheckInfo Info { get; set; }
}
