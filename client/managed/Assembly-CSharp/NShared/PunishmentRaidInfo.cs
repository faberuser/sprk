namespace NShared;

[JsonSerializable(Javascript = true)]
public class PunishmentRaidInfo : IAccountAttrKey<PunishmentRaidKey>
{
	PunishmentRaidKey IAccountAttrKey<PunishmentRaidKey>.Key => new PunishmentRaidKey(RaidIndex, RaidLevel);

	public int RaidIndex { get; set; }

	public int RaidLevel { get; set; }

	public int ClearCheckIndex { get; set; }

	public string ClearedTime { get; set; }
}
