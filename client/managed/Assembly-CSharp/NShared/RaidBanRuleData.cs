namespace NShared;

[JsonSerializable]
public class RaidBanRuleData
{
	public RaidBanType BanType { get; set; }

	public string BanValue { get; set; }
}
